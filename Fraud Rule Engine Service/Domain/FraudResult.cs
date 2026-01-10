using System;
using System.Collections.Generic;

namespace Fraud_Rule_Engine_Service.Domain
{
    public class FraudResult
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TransactionId { get; set; }
        public bool IsFraud { get; set; }
        public IList<string> MatchedRules { get; set; } = new List<string>();
        public int Score { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}