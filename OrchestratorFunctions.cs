using System;
using System.Collections.Generic;
using System.Linq;
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

            string transcodeLocation = null;
            string thumbnailLocation = null;
            string withIntoLocation = null;

            try
            {
                var bitRates = new[]{ 1000,2000,3000,4000 };
                var transcodeTasks = new List<Task<VideoFileInfo>>();

                foreach (var bitRate in bitRates)
                {
                    var info = new VideoFileInfo { Location = videoLocation, BitRate = bitRate };
                    var task = context.CallActivityAsync<VideoFileInfo>("TranscodeVideo", info);
                    transcodeTasks.Add(task);
                }
                var transcodeResults = await Task.WhenAll(transcodeTasks);


                transcodeLocation = transcodeResults.OrderByDescending(r => r.BitRate)
                    .Select(r => r.Location)
                    .First();

                //Retry for times after 5 seconds.
                thumbnailLocation = await context.CallActivityWithRetryAsync<string>("ExtractThumbnail", 
                    new RetryOptions(TimeSpan.FromSeconds(5), 4) { Handle = e => e.InnerException is InvalidOperationException},
                    transcodeLocation);

                withIntoLocation = await context.CallActivityAsync<string>("PrependIntro", thumbnailLocation);
            }
            catch (System.Exception ex)
            {
                log.LogInformation($"Caught an error from activity: {ex.Message}");
                await context.CallActivityAsync<string>("Cleanup", new[] { transcodeLocation, thumbnailLocation, withIntoLocation });

                return new
                {
                    Error = "Failed to process upload video",
                    Message = ex.Message
                };
            };

            return new
            {
                TranscodeLocation = transcodeLocation,
                ThumbnailLocation = thumbnailLocation,
                WithIntoLocation = withIntoLocation
            };



        }
    }
}