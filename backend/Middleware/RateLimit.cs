using StackExchange.Redis;
using System.Net;
using System.Security.Claims;

namespace Backend.Middleware;

public class RateLimitMiddleware : IMiddleware
{
    private readonly IConnectionMultiplexer _redis;
    private const int UnauthenticatedLimit = 10;
    private const int AuthenticatedLimit = 100;
    private const int WindowSeconds = 60;

    public RateLimitMiddleware(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!_redis.IsConnected)
        {
            await next(context);
            return;
        }

        try
        {
            var db = _redis.GetDatabase();
            string cacheKey;
            int maxRequests;

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                cacheKey = $"ratelimit:user:{userId}";
                maxRequests = AuthenticatedLimit;
            }
            else
            {
                var ip = context.Connection.RemoteIpAddress?.ToString();
                cacheKey = $"ratelimit:ip:{ip}";
                maxRequests = UnauthenticatedLimit;
            }

            long requestCount = await db.StringIncrementAsync(cacheKey);

            if (requestCount == 1)
            {
                await db.KeyExpireAsync(cacheKey, TimeSpan.FromSeconds(WindowSeconds));
            }

            if (requestCount > maxRequests)
            {
                var ttl = await db.KeyTimeToLiveAsync(cacheKey);
                if (!ttl.HasValue)
                {
                    await db.KeyExpireAsync(cacheKey, TimeSpan.FromSeconds(WindowSeconds));
                    ttl = TimeSpan.FromSeconds(WindowSeconds);
                }

                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers.Append("Retry-After", ((int)ttl.Value.TotalSeconds).ToString());
                await context.Response.WriteAsync("Probeer het later nog eens.");

                return;
            }
        }
        catch (RedisConnectionException)
        {
            // Redis tijdelijk onbereikbaar — verzoek doorgeven zonder rate limiting
        }

        await next(context);
    }

}






