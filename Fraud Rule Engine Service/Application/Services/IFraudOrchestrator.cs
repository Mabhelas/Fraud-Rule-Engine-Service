using System.Threading.Tasks;
using Fraud_Rule_Engine_Service.Domain;

namespace Fraud_Rule_Engine_Service.Application.Services
{
    public interface IFraudOrchestrator
    {
        Task<FraudResult> ExecuteRulesAsync(TransactionEvent tx);
    }
}