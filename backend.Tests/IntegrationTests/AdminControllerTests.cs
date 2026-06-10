using System.Net;
using System.Net.Http.Json;
using Backend.DTOs;
using Xunit;

namespace Backend.Tests.IntegrationTests;

public class AdminControllerTests : IntegrationTestBase
{
    public AdminControllerTests(CustomApiFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateEmployee_ShouldReturn201Created_WhenAdminProvidesValidData()
    {
        // ARRANGE
        int adminUserId = await SeedUserAsync("admin@test.nl", "Admin");
        AuthenticateClient(adminUserId, "Admin");

        var newEmployee = new CreateEmployeeDto
        {
            Email = "nieuwe.medewerker@woonwereld.nl",
            Password = "TijdelijkWachtwoord123!",
            FirstName = "Jan",
            LastName = "Jansen",
            Phone = "0612345678",
            Position = "Klantenservice"
        };

        // ACT
        var response = await _client.PostAsJsonAsync("/api/admin/employees", newEmployee);

        // ASSERT
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("succesvol aangemaakt", responseString);
    }

    [Fact]
    public async Task CreateEmployee_ShouldReturn403Forbidden_WhenEmployeeAttemptsCreation()
    {
        // ARRANGE
        int employeeUserId = await SeedUserAsync("medewerker.rechten@test.nl", "Employee");
        AuthenticateClient(employeeUserId, "Employee");

        var newEmployee = new CreateEmployeeDto
        {
            Email = "test@woonwereld.nl",
            Password = "TijdelijkWachtwoord123!",
            FirstName = "Test",
            LastName = "Test",
            Phone = "0600000000",
            Position = "Test"
        };

        // ACT
        var response = await _client.PostAsJsonAsync("/api/admin/employees", newEmployee);

        // ASSERT
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}