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
            FirstName = "Test",
            LastName = "User",
            Email = "nieuw@ikea.nl", 
            Password = "VeiligWachtwoord123!",
            FirstName = "Nieuwe",
            LastName = "Klant"
        };

        // ACT
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // ASSERT
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("Registration successful", responseString);
    }

    [Fact]
    public async Task Register_ShouldReturn409Conflict_WhenEmailAlreadyExists()
    {
        // ARRANGE
        string existingEmail = "bestaand@ikea.nl";
        await SeedUserAsync(existingEmail, "Customer");

        var request = new RegisterDto 
        { 
            Email = existingEmail, 
            Password = "AnderWachtwoord123!",
            FirstName = "Nieuwe",
            LastName = "Klant"
        };

        // ACT
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // ASSERT
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode); 
        
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("E-mailadres is al in gebruik.", responseString);
    }

    [Fact]
    public async Task Register_ShouldReturn400BadRequest_WhenRequiredFieldsAreMissing()
    {
        // ARRANGE
        var request = new 
        { 
            Email = "incompleet@ikea.nl"
            // Password, FirstName en LastName ontbreken
        };

        // ACT
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // ASSERT
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}