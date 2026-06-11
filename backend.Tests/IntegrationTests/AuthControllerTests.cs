using System.Net;
using System.Net.Http.Json;
using Xunit;
using Backend.DTOs;

namespace Backend.Tests.IntegrationTests;

public class AuthControllerTests : IClassFixture<CustomApiFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomApiFactory factory)
    {
        // Vraagt de factory om een HttpClient te maken
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldReturn200Ok_WhenValidDataIsProvided()
    {
        // ARRANGE
        var request = new RegisterDto 
        { 
            Email = "nieuw@ikea.nl", 
            Password = "VeiligWachtwoord123!",
            FirstName = "Test",
            LastName = "Gebruiker"
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