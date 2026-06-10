using System.Net;
using System.Net.Http.Json;
using Backend.DTOs;
using Xunit;

namespace Backend.Tests.IntegrationTests;

public class AuthControllerTests : IntegrationTestBase
{
    public AuthControllerTests(CustomApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_ShouldReturn200Ok_WhenValidDataIsProvided()
    {
        // ARRANGE
        var request = new RegisterDto 
        { 
            Email = "nieuw@ikea.nl", 
            Password = "VeiligWachtwoord123!",
            FirstName = "Nieuwe",
            LastName = "Klant"
        };

        // ACT
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // ASSERT
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Optioneel: Controleert de tekst in de response
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("Registration successful", responseString);
    }
}