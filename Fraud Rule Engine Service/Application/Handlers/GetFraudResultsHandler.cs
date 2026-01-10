using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Fraud_Rule_Engine_Service.Application.Queries;
using Fraud_Rule_Engine_Service.Repositories;
using Fraud_Rule_Engine_Service.Domain;

namespace Fraud_Rule_Engine_Service.Application.Handlers
{
    public class GetFraudResultsHandler : IRequestHandler<GetFraudResultsQuery, IEnumerable<FraudResult>>
    {
        private readonly IFraudRepository _repository;

        public GetFraudResultsHandler(IFraudRepository repository) => _repository = repository;

        public Task<IEnumerable<FraudResult>> Handle(GetFraudResultsQuery request, CancellationToken cancellationToken)
        {
            return _repository.GetFraudResultsAsync(request.OnlyFraud);
        }
    }
}