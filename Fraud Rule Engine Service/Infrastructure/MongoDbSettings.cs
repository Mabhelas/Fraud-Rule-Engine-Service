namespace Fraud_Rule_Engine_Service.Infrastructure
{
    public class MongoDbSettings
    {
        public string? ConnectionString { get; set; }
        public string DatabaseName { get; set; } = "frauddb";
        public string TransactionsCollection { get; set; } = "transactions";
        public string ResultsCollection { get; set; } = "fraudresults";
    }
}