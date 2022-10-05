using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace VideoProcessor
{
    public static class HttpFunctions
    {
        [FunctionName(nameof(ProcessVideoStarter))]
        public static async Task<IActionResult> ProcessVideoStarter(
                [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
                [DurableClient] IDurableOrchestrationClient starter,
                ILogger log)
        {
            string video = req.GetQueryParameterDictionary()["video"];

            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("ProcessVideoOrchestrator", null, video);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
        [FunctionName(nameof(SubmitVideoApproval))]
        public static async Task<IActionResult> SubmitVideoApproval(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route ="SubmitVideoApproval/{id}")] HttpRequest request,
            [DurableClient] IDurableOrchestrationClient client,
            [Table("Approvals", "Approval", "{id}", Connection = "AzureWebJobsStorage")] Approval approval,
            ILogger log
            )
        {
            string result = request.GetQueryParameterDictionary()["result"];

            if (result == null)
            {
                return new BadRequestObjectResult("Need an approval result");
            }

            log.LogWarning($"Sending approval result to {approval.OrchestrationId} of {result}");

            //send approval result external event to this orchestration
            await client.RaiseEventAsync(approval.OrchestrationId, "ApprovalResult", result);

            return new OkResult();
        }


        [FunctionName(nameof(StartPeriodicTask))]
        public static async Task<IActionResult> StartPeriodicTask(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest request,
            [DurableClient] IDurableOrchestrationClient client,
            ILogger log
            )
        {
            var instanceId = await client.StartNewAsync(nameof(OrchestratorFunctions.PeriodicTaskOrchestrator), null, 0);

            var payload = client.CreateHttpManagementPayload(instanceId);

            return new OkObjectResult(payload);
        }
    }
}