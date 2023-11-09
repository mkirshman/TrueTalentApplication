using System;
using System.IO;
using System.Linq;
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
            // Extract the observed on-base percentage (observedOBP) from the query parameters
            if (req.Query.TryGetValue("observedOBP", out var observedOBPValues) && observedOBPValues.Count > 0)
            {
                if (double.TryParse(observedOBPValues[0], out double observedOBP))
                {
                    // Extract the probabilities array from the query parameters
                    if (req.Query.TryGetValue("probabilities", out var probabilitiesValues) && probabilitiesValues.Count > 0)
                    {
                        // Parse the probabilities array into an array of doubles
                        var probabilities = probabilitiesValues[0].Split(',').Select(double.Parse).ToArray();

                        // Extract the prior probability (priorProbabilityB) from the query parameters
                        if (req.Query.TryGetValue("priorProbabilityB", out var priorProbabilityBValues) && priorProbabilityBValues.Count > 0)
                        {
                            if (double.TryParse(priorProbabilityBValues[0], out double priorProbabilityB))
                            {
                                // Calculate the probabilityAIfB
                                double probabilityAIfB = observedOBP;

                                // Calculate the probabilityA
                                double probabilityA = probabilities.Select(p => p * priorProbabilityB).Sum();

                                // Calculate the conditional probability using Bayes' theorem
                                double probabilityBIfA = (probabilityAIfB * priorProbabilityB) / probabilityA;

                                return new OkObjectResult(probabilityBIfA);
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