using System;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dynamitey;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace durablefunction.howtotest.OrchestratorTest
{
    public static class SagaToTestOrchestrator
    {
        [FunctionName("SagaToTestOrchestrator")]
        public static async Task<ShippingPrice> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var input = context.GetInput<SagaContext>();

            // activity to check if we ship to the specified continent
            if (!await context.CallActivityAsync<bool>("IsContinentSupported", input.Continent))
            {
                return new ShippingPrice()
                {
                    Shippable = false,
                    Message = "We aren't able to ship to your location"
                };
            }

            // activity to get proper orchestrator for continent for shipping partner
            var supplierOrchestratorToRun = await context.CallActivityAsync<string>("GetSupplierOrchestratorForContinent", input.Continent);

            // orchestrator to get the price for the shipping address
            var priceForShipment =
                await context.CallSubOrchestratorAsync<decimal>($"{supplierOrchestratorToRun}Orchestrator", input);


            // activity to publish event for Sales / marketing
            await context.CallActivityAsync("PublishCalculatedPriceActivity", (input, priceForShipment));

            return new ShippingPrice()
            {
                Shippable = true,
                Price = priceForShipment
            };
        }

        [FunctionName("CourierAOrchestrator")]
        public static async Task<decimal> CourierAOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            return 100;
        }

        [FunctionName("CourierBOrchestrator")]
        public static async Task<decimal> CourierBOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            return 120;
        }

        [FunctionName("IsContinentSupported")]
        public static async Task<bool> IsContinentSupported([ActivityTrigger] string continent, ILogger log)
        {
            var supportedContinents = new List<string>
            {
                "North America", "South America", "Europe",
            };

            return supportedContinents.Contains(continent);
        }

        [FunctionName("GetSupplierOrchestratorForContinent")]
        public static async Task<string> GetSupplierOrchestratorForContinent([ActivityTrigger] string continent, ILogger log)
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

        [FunctionName("PublishCalculatedPriceActivity")]
        public static async Task PublishCalculatedPriceActivity([ActivityTrigger] (SagaContext context, decimal price) input, ILogger log)
        {
            log.LogInformation($"{input.context.Continent}: {input.price}");
        }

        [FunctionName("SagaToTestOrchestrator_HttpStart")]
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

    public class ShippingPrice
    {
        public bool Shippable { get; set; }
        public decimal Price { get; set; }
        public string Message { get; set; }
    }

    public class SagaContext
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Continent { get; set; }
    }
}
