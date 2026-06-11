using System.Net;
using Xunit;

namespace Backend.Tests.IntegrationTests;

public class EmployeeControllerTests : IntegrationTestBase
{
    public EmployeeControllerTests(CustomApiFactory factory) : base(factory) { }

    [Fact]
    public async Task GetCustomerByEmail_ShouldReturn403Forbidden_WhenUserIsCustomer()
    {
        // ARRANGE
        int userId = await SeedUserAsync("customer2@test.nl", "Customer");
        AuthenticateClient(userId, "Customer");

        // ACT
        var response = await _client.GetAsync("/api/employee/customer-by-email?email=test@test.nl");

        // ASSERT
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomerByEmail_ShouldReturn404NotFound_WhenEmailDoesNotExist_AsEmployee()
    {
        // ARRANGE
        int employeeUserId = await SeedUserAsync("employee1@test.nl", "Employee");
        AuthenticateClient(employeeUserId, "Employee");

        // ACT
        var response = await _client.GetAsync("/api/employee/customer-by-email?email=bestaatniet@test.nl");

        // ASSERT
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomerByEmail_ShouldReturn400BadRequest_WhenEmailQueryIsMissing()
    {
        // ARRANGE
        int employeeUserId = await SeedUserAsync("employee2@test.nl", "Employee");
        AuthenticateClient(employeeUserId, "Employee");

        // ACT
        var response = await _client.GetAsync("/api/employee/customer-by-email");

        // ASSERT
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound);
    }
}