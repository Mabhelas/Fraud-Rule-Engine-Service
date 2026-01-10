using System;
using System.Threading.Tasks;
using Fraud_Rule_Engine_Service.Application.Services;
using Fraud_Rule_Engine_Service.Domain;
using Fraud_Rule_Engine_Service.Repositories;
using FluentAssertions;
using Xunit;

namespace FraudRuleEngineService.Tests.Unit
{
    public class FraudOrchestratorTests
    {
        [Fact]
        public async Task HighAmountTransaction_IsFlagged()
        {
            var repo = new InMemoryFraudRepository();
            var orchestrator = new FraudOrchestrator(repo);

            var tx = new TransactionEvent
            {
                AccountId = "acct-1",
                Amount = 15_000m,
                Merchant = "GoodShop"
            };

            var res = await orchestrator.ExecuteRulesAsync(tx);

            res.IsFraud.Should().BeTrue();
            res.MatchedRules.Should().Contain("HighAmountRule");
            res.Score.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task BlacklistedMerchant_IsFlagged()
        {
            var repo = new InMemoryFraudRepository();
            var orchestrator = new FraudOrchestrator(repo);

            var tx = new TransactionEvent
            {
                AccountId = "acct-2",
                Amount = 50m,
                Merchant = "ShadyMerchant"
            };

            var res = await orchestrator.ExecuteRulesAsync(tx);

            res.IsFraud.Should().BeTrue();
            res.MatchedRules.Should().Contain("BlacklistedMerchantRule");
        }
    }
}