using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace durablefunction.howtotest.OrchestratorTest
{
    public static class SagaToTestOrchestratorWithRetry
    {
        private static readonly RetryOptions RetryOptions = new RetryOptions(TimeSpan.FromSeconds(5), 3);

        [FunctionName("SagaToTestOrchestratorWithRetry")]
        public static async Task<ShippingPrice> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var input = context.GetInput<SagaContext>();

            // activity to check if we ship to the specified continent
            if (!await context.CallActivityWithRetryAsync<bool>("IsContinentSupportedWithRetry", RetryOptions,
                input.Continent))
            {
                return new ShippingPrice()
                {
                    Shippable = false,
                    Message = "We aren't able to ship to your location"
                };
            }

            // activity to get proper orchestrator for continent for shipping partner
            var supplierOrchestratorToRun =
                await context.CallActivityWithRetryAsync<string>("GetSupplierOrchestratorForContinentWithRetry",
                    RetryOptions, input.Continent);

            // orchestrator to get the price for the shipping address
            var priceForShipment =
                await context.CallSubOrchestratorWithRetryAsync<decimal>(
                    $"{supplierOrchestratorToRun}OrchestratorWithRetry", RetryOptions, input);


            // activity to publish event for Sales / marketing
            await context.CallActivityWithRetryAsync("PublishCalculatedPriceActivityWithRetry", RetryOptions,
                (input, priceForShipment));

            return new ShippingPrice()
            {
                Shippable = true,
                Price = priceForShipment
            };
        }

        [FunctionName("CourierAOrchestratorWithRetry")]
        public static async Task<decimal> CourierAOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            return 100;
        }

        [FunctionName("CourierBOrchestratorWithRetry")]
        public static async Task<decimal> CourierBOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            return 120;
        }

        [FunctionName("IsContinentSupportedWithRetry")]
        public static async Task<bool> IsContinentSupported([ActivityTrigger] string continent, ILogger log)
        {
            var supportedContinents = new List<string>
            {
                "North America", "South America", "Europe",
            };

            return supportedContinents.Contains(continent);
        }

        [FunctionName("GetSupplierOrchestratorForContinentWithRetry")]
        public static async Task<string> GetSupplierOrchestratorForContinent([ActivityTrigger] string continent,
            ILogger log)
        {
            var courier = "";
            switch (continent)
            {
                case "South America":
                case "North America":
                    courier = "CourierA";
                    break;
                case "Europe":
                    courier = "CourierB";
                    break;
            }

            return courier;
        }

        [FunctionName("PublishCalculatedPriceActivityWithRetry")]
        public static async Task PublishCalculatedPriceActivity(
            [ActivityTrigger] (SagaContext context, decimal price) input, ILogger log)
        {
            log.LogInformation($"{input.context.Continent}: {input.price}");
        }

        [FunctionName("SagaToTestOrchestrator_HttpStartWithRetry")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
            HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.

            string instanceId = await starter.StartNewAsync("SagaToTestOrchestrator", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

    }
}
