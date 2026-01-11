using MediatR;
using Fraud_Rule_Engine_Service.Application.Commands;
using Fraud_Rule_Engine_Service.Application.Services;
using Fraud_Rule_Engine_Service.Domain;

namespace Fraud_Rule_Engine_Service.Application.Handlers
{
    public class SubmitTransactionHandler : IRequestHandler<SubmitTransactionCommand, FraudResult>
    {
        private readonly IFraudOrchestrator _orchestrator;

        public SubmitTransactionHandler(IFraudOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        public async Task<FraudResult> Handle(SubmitTransactionCommand request, CancellationToken cancellationToken)
        {
            var tx = request.Transaction;
            // Domain validation and enrichment can happen here
            return await _orchestrator.ExecuteRulesAsync(tx);
        }
    }
}