using MediatR;
using Fraud_Rule_Engine_Service.Application.Queries;
using Fraud_Rule_Engine_Service.Repositories;
using Fraud_Rule_Engine_Service.Domain;

namespace Fraud_Rule_Engine_Service.Application.Handlers
{
    public class GetFraudResultHandler : IRequestHandler<GetFraudResultQuery, FraudResult?>
    {
        private readonly IFraudRepository _repository;

        public GetFraudResultHandler(IFraudRepository repository) => _repository = repository;

        public Task<FraudResult?> Handle(GetFraudResultQuery request, CancellationToken cancellationToken)
        {
            return _repository.GetFraudResultByIdAsync(request.Id);
        }
    }
}