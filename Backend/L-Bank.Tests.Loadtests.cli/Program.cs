using NBomber.Contracts;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using System.Net.Http.Json;

Console.WriteLine("Calling LBank Info API...");
try
{
    // Log in to get the token before accessing protected endpoints
    Console.WriteLine("Logging in to get the authentication token...");
    string token = await GetAuthToken();
    Console.WriteLine($"Token received: {token}");

    // Get the total balance before the test
    Console.WriteLine("Getting total balance before the test...");
    decimal totalBalanceBefore = await GetTotalBalance(token);
    Console.WriteLine($"Total balance before test: {totalBalanceBefore}");

    // Call the LBank Info API
    var response = await CallLBankInfoApi(token);
    Console.WriteLine("API Response:");
    Console.WriteLine(response);

    Console.WriteLine("Starting NBomber load test...");
    var scenario = CreateScenario(token);
    var postScenario = CreatePostScenario(token);

    NBomberRunner
        .RegisterScenarios(scenario, postScenario)
        .WithReportFileName("reports")
        .WithReportFolder("reports")
        .WithReportFormats(ReportFormat.Html)
        .Run();

    // Get the total balance after the test
    Console.WriteLine("Getting total balance after test...");
    decimal totalBalanceAfter = await GetTotalBalance(token);
    Console.WriteLine($"Total balance after: {totalBalanceAfter}");

    // Check if the total balance is consistent
    if (totalBalanceBefore == totalBalanceAfter)
    {
        Console.WriteLine("Total balance is consistent!");
    }
    else
    {
        Console.WriteLine("Warning: Total balance is not consistent.");
    }
}
catch (Exception ex)
{
    Console.WriteLine("An error occurred while calling the API:");
    Console.WriteLine(ex.Message);
}
Console.WriteLine("Press any key to exit");
Console.ReadKey();
return;

static async Task<string> CallLBankInfoApi(string token)
{
    using var httpClient = new HttpClient();  // Create a new HttpClient for this request

    httpClient.BaseAddress = new Uri("http://localhost:5000");
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var response = await httpClient.GetAsync("/api/v1/lbankinfo");
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsStringAsync();
}

static async Task<decimal> GetTotalBalance(string token)
{
    using var httpClient = new HttpClient();  // Create a new HttpClient for this request

    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var response = await httpClient.GetAsync("http://localhost:5000/api/v1/ledgers/totalBalance");
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<decimal>();
}

static ScenarioProps CreateScenario(string token)
{
    var httpClient = new HttpClient();  // Create a new HttpClient for this request

    // Set Authorization header before starting the scenario
    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    return Scenario.Create("http_scenario", async _ =>
    {
        var request =
            NBomber.Http.CSharp.Http.CreateRequest("GET", "http://localhost:5000/api/v1/lbankinfo")
                .WithHeader("Accept", "application/json")
                .WithHeader("Authorization", $"Bearer {token}");

        var response = await NBomber.Http.CSharp.Http.Send(httpClient, request);
        return response;
    })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 100,
                interval: TimeSpan.FromSeconds(1),
                during: TimeSpan.FromSeconds(30))
        );
}

static async Task<string> GetAuthToken()
{
    using var httpClient = new HttpClient();  // Create a new HttpClient for this request

    var loginRequest = new
    {
        username = "admin",
        password = "adminpass"
    };

    var loginResponse = await httpClient.PostAsJsonAsync("http://localhost:5000/api/v1/Login", loginRequest);
    loginResponse.EnsureSuccessStatusCode();

    var responseData = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
    return responseData?.Token ?? throw new InvalidOperationException("Failed to retrieve token.");
}

static ScenarioProps CreatePostScenario(string token)
{
    var httpClient = new HttpClient();  // Create a new HttpClient for this request

    // Set Authorization header before starting the scenario
    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    return Scenario.Create("http_post_scenario", async context =>
    {
        var booking = new Booking
        {
            SourceId = 1,
            DestinationId = 2,
            Amount = 10
        };

        var request = NBomber.Http.CSharp.Http.CreateRequest("POST", "http://localhost:5000/api/v1/bookings")
            .WithHeader("Accept", "application/json")
            .WithJsonBody(booking);

        var response = await NBomber.Http.CSharp.Http.Send(httpClient, request);
        return response;
    })
    .WithoutWarmUp()
    .WithLoadSimulations(
        Simulation.RampingInject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(1))
    );
}

public class AuthResponse
{
    public required string Token { get; set; }
}

public class Booking
{
    public int SourceId { get; set; }
    public int DestinationId { get; set; }
    public decimal Amount { get; set; }
}
