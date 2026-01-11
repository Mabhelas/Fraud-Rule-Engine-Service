using MediatR;
using Fraud_Rule_Engine_Service.Domain;

namespace Fraud_Rule_Engine_Service.Application.Queries
{
    public record GetFraudResultsQuery(bool? OnlyFraud) : IRequest<IEnumerable<FraudResult>>;
}