using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VideoProcessor
{
    public static class ActivityFunctions
    {
        [FunctionName(nameof(TranscodeVideo))]
        public static async Task<VideoFileInfo> TranscodeVideo([ActivityTrigger] VideoFileInfo inputVideo, ILogger log)
        {
            log.LogInformation($"Transcodeing {inputVideo.Location} to {inputVideo.BitRate}.");
            
            await Task.Delay(5000);

            var transcodeLocation = $"{Path.GetFileNameWithoutExtension(inputVideo.Location)}-{inputVideo.BitRate}kbps.mp4";

            return new VideoFileInfo
            {
                Location = transcodeLocation,
                BitRate = inputVideo.BitRate
            };
        }

        [FunctionName(nameof(ExtractThumbnail))]
        public static async Task<string> ExtractThumbnail([ActivityTrigger] string inputVideo, ILogger log)
        {
            log.LogInformation($"Extracting thumbnail {inputVideo}.");

            if (inputVideo.Contains("error"))
            {
                throw new InvalidOperationException("Failed to extract thumbnail");
            }

            await Task.Delay(5000);

            return $"{Path.GetFileNameWithoutExtension(inputVideo)}-thumbnail.png";
        }

        [FunctionName(nameof(PrependIntro))]
        public static async Task<string> PrependIntro([ActivityTrigger] string inputVideo, ILogger log)
        {
            log.LogInformation($"Prepending intro {inputVideo}.");

            await Task.Delay(5000);

            return $"{Path.GetFileNameWithoutExtension(inputVideo)}-withintro.mp4";
        }

        [FunctionName(nameof(Cleanup))]
        public static async Task<string> Cleanup([ActivityTrigger] string[] filesToCleanUp, ILogger log)
        {
            log.LogInformation($"Cleaning up.");

            foreach (var file in filesToCleanUp.Where(f=> f != null))
            {
                log.LogInformation($"Deleting: {file}");
                await Task.Delay(1000);
            }

            return $"Cleaned Up successfully";
        }

        [FunctionName(nameof(GetTranscodeBitRates))]
        public static int[] GetTranscodeBitRates([ActivityTrigger]object input)
        {
            return Environment.GetEnvironmentVariable("TranscodeBitRates").Split(',').Select(int.Parse).ToArray();
        }


        [FunctionName(nameof(SendApprovalRequestEmail))]
        public static void SendApprovalRequestEmail([ActivityTrigger] ApprovalInfo approvalInfo,
            [SendGrid(ApiKey ="SendGridKey")] out SendGridMessage message,
            [Table("Approvals", "AzureWebJobsStorage")] out Approval approval,
            ILogger log)
        {
            var approvalCode = Guid.NewGuid().ToString("N");

            approval = new Approval
            {

                PartitionKey = "Approval",
                RowKey = approvalCode,
                OrchestrationId = approvalInfo.OrchestrationId
            };

            var approvalEmail = new EmailAddress(Environment.GetEnvironmentVariable("ApprovalEmail"));
            var senderEmail = new EmailAddress(Environment.GetEnvironmentVariable("SenderEmail"));
            var host = Environment.GetEnvironmentVariable("Host");

            var functionAddress = $"{host}/api/SubmitVideoApproval/{approvalCode}";
            var approvedLink = functionAddress + "?result=Approved";
            var rejectedLink = functionAddress + "?result=Rejected";
            var body = $"Please review {approvalInfo.VideoLocation}<br>"
                + $"<a href=/{approvedLink}>Approve</a><br>"
                + $"<a href=/{rejectedLink}>Reject</a><br>";

            message = new SendGridMessage();
            message.Subject = "A video is awaiting for approval";
            message.From = senderEmail;
            message.AddTo(approvalEmail);
            message.HtmlContent = body;
            log.LogWarning(body);
        }
        
        [FunctionName(nameof(PublishVideo))]
        public static async Task PublishVideo([ActivityTrigger] string inputVideo, ILogger log)
        {
            log.LogInformation($"Publishing {inputVideo}.");

            //Simulate publishing
            await Task.Delay(1000);
        }

        [FunctionName(nameof(RejectVideo))]
        public static async Task RejectVideo([ActivityTrigger] string inputVideo, ILogger log)
        {
            log.LogInformation($"Rejecting {inputVideo}.");

            //Simulate rejecting
            await Task.Delay(1000);
        }



    }
}