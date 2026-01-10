using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fraud_Rule_Engine_Service.Domain;
using Fraud_Rule_Engine_Service.Repositories;

namespace Fraud_Rule_Engine_Service.Application.Services
{
    // Orchestrates domain rules; keep pure logic here and persist via repository.
    public class FraudOrchestrator : IFraudOrchestrator
    {
        private readonly IFraudRepository _repository;

        // Configurable thresholds (move to IOptions in next iteration)
        private readonly decimal _highAmountThreshold = 10_000m;
        private readonly string[] _blacklistedMerchants = new[] { "ShadyMerchant", "BadShop LLC" };
        private readonly TimeSpan _rapidWindow = TimeSpan.FromMinutes(1);
        private readonly int _rapidThreshold = 3;

        public FraudOrchestrator(IFraudRepository repository)
        {
            _repository = repository;
        }

        public async Task<FraudResult> ExecuteRulesAsync(TransactionEvent tx)
        {
            // persist raw transaction
            await _repository.SaveTransactionAsync(tx);

            var matched = new List<string>();

            if (HighAmountRule(tx)) matched.Add("HighAmountRule");
            if (BlacklistedMerchantRule(tx)) matched.Add("BlacklistedMerchantRule");
            if (await RapidSuccessionRuleAsync(tx)) matched.Add("RapidSuccessionRule");

            var result = new FraudResult
            {
                TransactionId = tx.Id,
                IsFraud = matched.Count > 0,
                MatchedRules = matched,
                Score = matched.Count,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _repository.SaveFraudResultAsync(result);
            return result;
        }

        private bool HighAmountRule(TransactionEvent tx) => tx.Amount >= _highAmountThreshold;

        private bool BlacklistedMerchantRule(TransactionEvent tx) =>
            Array.Exists(_blacklistedMerchants, m => string.Equals(m, tx.Merchant, StringComparison.OrdinalIgnoreCase));

        private async Task<bool> RapidSuccessionRuleAsync(TransactionEvent tx)
        {
            if (string.IsNullOrWhiteSpace(tx.AccountId)) return false;
            var recent = await _repository.GetRecentTransactionsAsync(tx.AccountId, _rapidWindow);
            var count = recent.Count();
            return count >= _rapidThreshold;
        }
    }
}