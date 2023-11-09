using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public static class TrueTalentApplication
{
    [FunctionName("EstimateTrueTalent")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
        try
        {
            // Extract the observed on-base probability (P(B|A)) from the query parameters
            if (req.Query.TryGetValue("observedOBP", out var observedOBPValues) && observedOBPValues.Count > 0)
            {
                if (double.TryParse(observedOBPValues[0], out double observedOBP))
                {
                    // Extract the prior probability (P(B)) from the query parameters
                    if (req.Query.TryGetValue("probabilityB", out var probabilityBValues) && probabilityBValues.Count > 0)
                    {
                        if (double.TryParse(probabilityBValues[0], out double probabilityB))
                        {
                            // Extract the prior probability (P(A)) from the query parameters
                            if (req.Query.TryGetValue("probabilityA", out var probabilityAValues) && probabilityAValues.Count > 0)
                            {
                                if (double.TryParse(probabilityAValues[0], out double probabilityA))
                                {
                                    // Calculate the conditional probability using Bayes' theorem
                                    double probabilityAIfB = (observedOBP * probabilityA) / probabilityB;

                                    return new OkObjectResult(probabilityAIfB);
                                }
                            }
                        }
                    }
                }
            }

            return new BadRequestObjectResult("Invalid or missing query parameters.");
        }
        catch (Exception ex)
        {
            log.LogError($"An error occurred: {ex.Message}");
            return new BadRequestObjectResult("An error occurred");
        }
    }
}
