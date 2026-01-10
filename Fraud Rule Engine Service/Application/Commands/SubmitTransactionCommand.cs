using MediatR;
using Fraud_Rule_Engine_Service.Domain;

namespace Fraud_Rule_Engine_Service.Application.Commands
{
    public record SubmitTransactionCommand(TransactionEvent Transaction) : IRequest<FraudResult>;
}