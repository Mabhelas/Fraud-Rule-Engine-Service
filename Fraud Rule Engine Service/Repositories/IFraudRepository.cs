using Fraud_Rule_Engine_Service.Domain;

namespace Fraud_Rule_Engine_Service.Repositories
{
    public interface IFraudRepository
    {
        Task SaveTransactionAsync(TransactionEvent tx);
        Task<IEnumerable<TransactionEvent>> GetRecentTransactionsAsync(string accountId, TimeSpan lookback);
        Task SaveFraudResultAsync(FraudResult result);
        Task<IEnumerable<FraudResult>> GetFraudResultsAsync(bool? onlyFraud = null);
        Task<FraudResult?> GetFraudResultByIdAsync(Guid id);
    }
}