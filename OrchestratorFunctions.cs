using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace VideoProcessor
{
    public static class OrchestratorFunctions
    {
        [FunctionName(nameof(ProcessVideoOrchestrator))]
        public static async Task<object> ProcessVideoOrchestrator(
               [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            log = context.CreateReplaySafeLogger(log);

            var videoLocation = context.GetInput<string>();

            var transcodeLocation = await context.CallActivityAsync<string> ("TranscodeVideo", videoLocation);

            var thumbnailLocation = await context.CallActivityAsync<string>("ExtractThumbnail", transcodeLocation);

            var withIntoLocation = await context.CallActivityAsync<string>("PrependIntro", thumbnailLocation);

            return new
            {
                Transcode = transcodeLocation,
                Thumbnail = thumbnailLocation,
                WithInto = withIntoLocation
            };
        }
    }
}