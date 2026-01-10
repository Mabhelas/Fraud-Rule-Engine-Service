using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace FraudRuleEngineService.Tests.Integration
{
    // WebApplicationFactory configured for integration tests. It injects test JwtSettings.
    public class IntegrationTestFactory : WebApplicationFactory<Program>
    {
        private const string DefaultSigningKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJkZW1vLXVzZXIiLCJyb2xlcyI6IkZyYXVkQW5hbHlzdCIsImV4cCI6MTc2NTc5OTQ4NiwiaXNzIjoiZnJhdWQtaXNzdWVyIiwiYXVkIjoiZnJhdWQtYXVkaWVuY2UifQ.Z2_3W3KL3eBYlaXKUXw69-wmW5J5NfY7lpJ34vcn-tQ";

        private readonly string _signingKey;

        // Single public ctor for xUnit's IClassFixture<T> activation
        public IntegrationTestFactory() : this(DefaultSigningKey)
        {
        }

        // Non-public overload so only one public constructor exists (xUnit requirement).
        internal IntegrationTestFactory(string signingKey)
        {
            _signingKey = signingKey;
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            // Override configuration for tests
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                var dict = new Dictionary<string, string?>
                {
                    ["JwtSettings:SigningKey"] = _signingKey,
                    ["JwtSettings:Issuer"] = "fraud-issuer",
                    ["JwtSettings:Audience"] = "fraud-audience",
                    // Ensure we do not attempt to connect to Mongo during tests (use in-memory)
                    ["MongoDbSettings:ConnectionString"] = ""
                };
                conf.AddInMemoryCollection(dict);
            });

            return base.CreateHost(builder);
        }

        // Helper to create a signed JWT for tests with specific roles
        // NOTE: Program config maps RoleClaimType = "roles", so we must emit "roles" claim name here.
        public static string CreateToken(string signingKey, string subject = "test-user", string[] roles = null, int minutesValid = 60)
        {
            roles ??= Array.Empty<string>();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var now = DateTime.UtcNow;

            var claims = new List<Claim> { new Claim(JwtRegisteredClaimNames.Sub, subject) };
            foreach (var r in roles)
                claims.Add(new Claim("roles", r)); // use "roles" to match TokenValidationParameters.RoleClaimType

            var token = new JwtSecurityToken(
                issuer: "fraud-issuer",
                audience: "fraud-audience",
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(minutesValid),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}