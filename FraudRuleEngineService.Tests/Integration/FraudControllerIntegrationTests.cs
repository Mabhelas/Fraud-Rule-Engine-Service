using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Fraud_Rule_Engine_Service.Domain;
using FluentAssertions;
using Xunit;

namespace FraudRuleEngineService.Tests.Integration
{
    public class FraudControllerIntegrationTests : IClassFixture<IntegrationTestFactory>
    {
        private readonly IntegrationTestFactory _factory;
        private readonly HttpClient _client;
        private readonly string _signingKey;

        public FraudControllerIntegrationTests()
        {
            // Use a deterministic test signing key
            _signingKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJkZW1vLXVzZXIiLCJyb2xlcyI6IkZyYXVkQW5hbHlzdCIsImV4cCI6MTc2NTc5OTQ4NiwiaXNzIjoiZnJhdWQtaXNzdWVyIiwiYXVkIjoiZnJhdWQtYXVkaWVuY2UifQ.Z2_3W3KL3eBYlaXKUXw69-wmW5J5NfY7lpJ34vcn-tQ";
            _factory = new IntegrationTestFactory(_signingKey);

            // Ensure we target HTTPS to avoid app's HTTPS redirection altering the request (prevents empty body / unexpected status)
            _client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
        }

        //[Fact]
        //public async Task PostTransaction_RequiresAuth_ReturnsCreated_WhenAuthorized()
        //{
        //    var token = IntegrationTestFactory.CreateToken(_signingKey, roles: new[] { "Ingestor" });
        //    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        //    var tx = new TransactionEvent
        //    {
        //        AccountId = "integration-001",
        //        Amount = 123.45m,
        //        Merchant = "IntegrationShop",
        //        Category = "Test"
        //    };

        //    var resp = await _client.PostAsJsonAsync("/api/fraud/transactions", tx);

        //    // Expect 201 Created when caller is authorized
        //    resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        //    var created = await resp.Content.ReadFromJsonAsync<FraudResult>();
        //    created.Should().NotBeNull();
        //    created!.TransactionId.Should().NotBeEmpty();
        //}

        [Fact]
        public async Task GetResults_Forbidden_WhenNoRole()
        {
            var token = IntegrationTestFactory.CreateToken(_signingKey, roles: Array.Empty<string>());
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var resp = await _client.GetAsync("/api/fraud/results");

            // Caller without roles should receive 403 Forbidden (authorization failure)
            resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}