using System;
using System.Collections.Generic;

namespace Fraud_Rule_Engine_Service.Domain
{
    public class TransactionEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string AccountId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "ZAR";
        public string Merchant { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public string? Location { get; set; }
        public IDictionary<string, string>? Metadata { get; set; }
    }
}