using System.Collections.Concurrent;
using Fraud_Rule_Engine_Service.Domain;

namespace Fraud_Rule_Engine_Service.Repositories
{
    public class InMemoryFraudRepository : IFraudRepository
    {
        private readonly ConcurrentDictionary<Guid, TransactionEvent> _transactions = new();
        private readonly ConcurrentDictionary<Guid, FraudResult> _results = new();

        public Task SaveTransactionAsync(TransactionEvent tx)
        {
            _transactions[tx.Id] = tx;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<TransactionEvent>> GetRecentTransactionsAsync(string accountId, TimeSpan lookback)
        {
            var cutoff = DateTimeOffset.UtcNow - lookback;
            var items = _transactions.Values
                .Where(t => string.Equals(t.AccountId, accountId, StringComparison.OrdinalIgnoreCase)
                            && t.Timestamp >= cutoff)
                .OrderByDescending(t => t.Timestamp)
                .ToArray();
            return Task.FromResult<IEnumerable<TransactionEvent>>(items);
        }

        public Task SaveFraudResultAsync(FraudResult result)
        {
            _results[result.Id] = result;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<FraudResult>> GetFraudResultsAsync(bool? onlyFraud = null)
        {
            var q = _results.Values.AsEnumerable();
            if (onlyFraud.HasValue)
            {
                q = q.Where(r => r.IsFraud == onlyFraud.Value);
            }

            // Ensure we return IEnumerable<T> (not IOrderedEnumerable<T>) wrapped in a Task
            var ordered = q.OrderByDescending(r => r.CreatedAt).AsEnumerable();
            return Task.FromResult<IEnumerable<FraudResult>>(ordered);
        }

        public Task<FraudResult?> GetFraudResultByIdAsync(Guid id)
        {
            _results.TryGetValue(id, out var result);
            return Task.FromResult(result);
        }
    }
}