using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Fraud_Rule_Engine_Service.Domain;
using Fraud_Rule_Engine_Service.Repositories;

namespace Fraud_Rule_Engine_Service.Infrastructure
{
    public class MongoFraudRepository : IFraudRepository
    {
        private readonly IMongoCollection<TransactionEvent> _txCol;
        private readonly IMongoCollection<FraudResult> _resCol;

        public MongoFraudRepository(IOptions<MongoDbSettings> options)
        {
            var settings = options.Value;
            var client = new MongoClient(settings.ConnectionString);
            var db = client.GetDatabase(settings.DatabaseName);
            _txCol = db.GetCollection<TransactionEvent>(settings.TransactionsCollection);
            _resCol = db.GetCollection<FraudResult>(settings.ResultsCollection);

            _txCol.Indexes.CreateOne(new CreateIndexModel<TransactionEvent>(
                Builders<TransactionEvent>.IndexKeys.Ascending(t => t.AccountId).Ascending(t => t.Timestamp)));
            _resCol.Indexes.CreateOne(new CreateIndexModel<FraudResult>(
                Builders<FraudResult>.IndexKeys.Ascending(r => r.TransactionId)));
        }

        public async Task SaveTransactionAsync(TransactionEvent tx)
        {
            await _txCol.ReplaceOneAsync(t => t.Id == tx.Id, tx, new ReplaceOptions { IsUpsert = true });
        }

        public async Task<IEnumerable<TransactionEvent>> GetRecentTransactionsAsync(string accountId, TimeSpan lookback)
        {
            var cutoff = DateTimeOffset.UtcNow - lookback;
            var filter = Builders<TransactionEvent>.Filter.And(
                Builders<TransactionEvent>.Filter.Eq(t => t.AccountId, accountId),
                Builders<TransactionEvent>.Filter.Gte(t => t.Timestamp, cutoff));
            var items = await _txCol.Find(filter).SortByDescending(t => t.Timestamp).ToListAsync();
            return items;
        }

        public async Task SaveFraudResultAsync(FraudResult result)
        {
            await _resCol.ReplaceOneAsync(r => r.Id == result.Id, result, new ReplaceOptions { IsUpsert = true });
        }

        public async Task<IEnumerable<FraudResult>> GetFraudResultsAsync(bool? onlyFraud = null)
        {
            var filter = onlyFraud.HasValue
                ? Builders<FraudResult>.Filter.Eq(r => r.IsFraud, onlyFraud.Value)
                : Builders<FraudResult>.Filter.Empty;

            var items = await _resCol.Find(filter).SortByDescending(r => r.CreatedAt).ToListAsync();
            return items;
        }

        public async Task<FraudResult?> GetFraudResultByIdAsync(Guid id)
        {
            var res = await _resCol.Find(r => r.Id == id).FirstOrDefaultAsync();
            return res;
        }
    }
}