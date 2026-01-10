using System;
using MediatR;
using Fraud_Rule_Engine_Service.Domain;

namespace Fraud_Rule_Engine_Service.Application.Queries
{
    public record GetFraudResultQuery(Guid Id) : IRequest<FraudResult?>;
}