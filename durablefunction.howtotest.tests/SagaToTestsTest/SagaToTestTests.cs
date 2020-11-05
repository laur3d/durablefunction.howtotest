using System.Collections.Generic;
using System.Threading.Tasks;
using durablefunction.howtotest.OrchestratorTest;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace durablefunction.howtotest.tests.SagaToTestsTest
{
    public class SagaToTestTests
    {

        // V1: The MS Way
        [Fact]
        public async Task CalculatePriceForAmerica()
        {
            // Arrange / Given
            var orchContext = new SagaContext
            {
                Continent = "North America"
            };
            var context = new Mock<IDurableOrchestrationContext>();

            // mock the get input
            context.Setup(m =>
                m.GetInput<SagaContext>()).Returns(orchContext);

            //set-up mocks for activities
            context.Setup(m =>
                    m.CallActivityAsync<bool>("IsContinentSupported", It.IsAny<object>()))
                .ReturnsAsync(true);

            // set-up mocks for activity
            context.Setup(m
                    => m.CallActivityAsync<string>("GetSupplierOrchestratorForContinent", It.IsAny<object>()))
                .ReturnsAsync("CourierA");

            // set-up mocks for suborchstrators
            context.Setup(m =>
                    m.CallSubOrchestratorAsync<decimal>("CourierAOrchestrator", It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(100);

            // ACT / When
            var price = await SagaToTestOrchestrator.RunOrchestrator(context.Object);

            // Assert / Then
            Assert.True(price.Shippable);
            Assert.Equal(100, price.Price);

        }


        // V2: The Flow Way
        [Fact]
        public async Task CalculatePriceForEurope()
        {
            // Arrange / Given
            var orchContext = new SagaContext
            {
                Continent = "Europe"
            };
            var context = new Mock<IDurableOrchestrationContext>();

            // mock the get input
            context.Setup(m =>
                m.GetInput<SagaContext>()).Returns(orchContext);

            //set-up mocks for activities
            context.Setup(m =>
                    m.CallActivityAsync<bool>("IsContinentSupported", It.IsAny<object>()))
                .ReturnsAsync(true);

            // set-up mocks for activity
            context.Setup(m
                    => m.CallActivityAsync<string>("GetSupplierOrchestratorForContinent", It.IsAny<object>()))
                .ReturnsAsync("CourierB");


            // set-up mocks for suborchstrators
            context.Setup(m =>
                    m.CallSubOrchestratorAsync<decimal>("CourierAOrchestrator", It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(100);

            context.Setup(m =>
                    m.CallSubOrchestratorAsync<decimal>("CourierBOrchestrator", It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(120);

            // mock the publish activity
            // at the time of writing, there is no way of mocking CallActivityAsync so we need to use the generic version
            context.Setup(m =>
                m.CallActivityAsync<object>("PublishCalculatedPriceActivity", It.IsAny<object>())
            );


            // ACT / When
            var price = await SagaToTestOrchestrator.RunOrchestrator(context.Object);

            // Assert / Then

            context.Verify(
                m => m.CallActivityAsync<bool>(
                    "IsContinentSupported",
                    It.IsAny<object>()),
                Times.Once);

            context.Verify(
                    m => m.CallActivityAsync<string>(
                        "GetSupplierOrchestratorForContinent", It.IsAny<object>()),
                    Times.Once
                );

            context.Verify(m =>
                    m.CallSubOrchestratorAsync<decimal>("CourierAOrchestrator", It.IsAny<string>(), It.IsAny<object>()),
                Times.Never);

            context.Verify(m =>
                    m.CallSubOrchestratorAsync<decimal>("CourierBOrchestrator", It.IsAny<string>(), It.IsAny<object>()),
                Times.Once);

            context.Verify( m =>
                    m.CallActivityAsync<object>("PublishCalculatedPriceActivity", It.IsAny<object>()),
                Times.Once
            );

        }

        // V3 : Parameterized Flow
        [Theory]
        [MemberData(nameof(DataSourceForTest))]
        public async Task TestUsingTheory(OrchestratorTestParams pTestParams)
        {
           // Arrange / Given
            var orchContext = new SagaContext
            {
                Continent = pTestParams.Continent
            };
            var context = new Mock<IDurableOrchestrationContext>();

            // mock the get input
            context.Setup(m =>
                m.GetInput<SagaContext>()).Returns(orchContext);

            //set-up mocks for activities
            context.Setup(m =>
                    m.CallActivityAsync<bool>("IsContinentSupported", It.IsAny<object>()))
                .ReturnsAsync(pTestParams.IsContinentSupported);

            // set-up mocks for activity
            context.Setup(m
                    => m.CallActivityAsync<string>("GetSupplierOrchestratorForContinent", It.IsAny<object>()))
                .ReturnsAsync(pTestParams.SupplierToBeReturnedFromContinentOrchestrator);


            // set-up mocks for suborchstrators
            context.Setup(m =>
                    m.CallSubOrchestratorAsync<decimal>("CourierAOrchestrator", It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(pTestParams.ValueForCourierA);

            context.Setup(m =>
                    m.CallSubOrchestratorAsync<decimal>("CourierBOrchestrator", It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(pTestParams.ValueForCourierB);

            // mock the publish activity
            // at the time of writing, there is no way of mocking CallActivityAsync so we need to use the generic version
            context.Setup(m =>
                m.CallActivityAsync<object>("PublishCalculatedPriceActivity", It.IsAny<object>())
            );


            // ACT / When
            var price = await SagaToTestOrchestrator.RunOrchestrator(context.Object);

            // Assert / Then

            context.Verify(
                m => m.CallActivityAsync<bool>(
                    "IsContinentSupported",
                    It.IsAny<object>()),
                pTestParams.IsContinentSupportedCalledTimes);

            context.Verify(
                    m => m.CallActivityAsync<string>(
                        "GetSupplierOrchestratorForContinent", It.IsAny<object>()),
                    pTestParams.GetSupplierOrchestratorForContinentCalledTimes
                );

            context.Verify(m =>
                    m.CallSubOrchestratorAsync<decimal>("CourierAOrchestrator", It.IsAny<string>(), It.IsAny<object>()),
                pTestParams.CourierAOrchestratorCalledTimes);

            context.Verify(m =>
                    m.CallSubOrchestratorAsync<decimal>("CourierBOrchestrator", It.IsAny<string>(), It.IsAny<object>()),
                pTestParams.CourierBOrchestratorCalledTimes);

            context.Verify( m =>
                    m.CallActivityAsync<object>("PublishCalculatedPriceActivity", It.IsAny<object>()),
                pTestParams.PublishCalculatedPriceActivityCalledTimes
            );
        }

        public static IEnumerable<object[]> DataSourceForTest =>
            new List<object[]>
            {
                new object[]
                {
                    new OrchestratorTestParams
                    {
                        Continent = "Europe",
                        IsContinentSupported = true,
                        SupplierToBeReturnedFromContinentOrchestrator = "CourierB",
                        ValueForCourierA = 100,
                        ValueForCourierB = 120,
                        IsContinentSupportedCalledTimes = Times.Once(),
                        GetSupplierOrchestratorForContinentCalledTimes = Times.Once(),
                        CourierAOrchestratorCalledTimes = Times.Never(),
                        CourierBOrchestratorCalledTimes = Times.Once(),
                        PublishCalculatedPriceActivityCalledTimes = Times.Once()
                    }
                },
                new object[] {
                    new OrchestratorTestParams
                    {
                        Continent = "North America",
                        IsContinentSupported = true,
                        SupplierToBeReturnedFromContinentOrchestrator = "CourierA",
                        ValueForCourierA = 100,
                        ValueForCourierB = 120,
                        IsContinentSupportedCalledTimes = Times.Once(),
                        GetSupplierOrchestratorForContinentCalledTimes = Times.Once(),
                        CourierAOrchestratorCalledTimes = Times.Once(),
                        CourierBOrchestratorCalledTimes = Times.Never(),
                        PublishCalculatedPriceActivityCalledTimes = Times.Once()
                    }
                },
                new object[] {
                    new OrchestratorTestParams
                    {
                        Continent = "Antartica",
                        IsContinentSupported = false,
                        SupplierToBeReturnedFromContinentOrchestrator = "CourierA",
                        ValueForCourierA = 100,
                        ValueForCourierB = 120,
                        IsContinentSupportedCalledTimes = Times.Once(),
                        GetSupplierOrchestratorForContinentCalledTimes = Times.Never(),
                        CourierAOrchestratorCalledTimes = Times.Never(),
                        CourierBOrchestratorCalledTimes = Times.Never(),
                        PublishCalculatedPriceActivityCalledTimes = Times.Never()
                    }
                }
            };

        public class OrchestratorTestParams
        {
            public string Continent { get; set; }
            public bool IsContinentSupported { get; set; }
            public string SupplierToBeReturnedFromContinentOrchestrator { get; set; }
            public decimal ValueForCourierA { get; set; }
            public decimal ValueForCourierB { get; set; }
            public Times IsContinentSupportedCalledTimes { get; set; }
            public Times GetSupplierOrchestratorForContinentCalledTimes { get; set; }
            public Times CourierAOrchestratorCalledTimes { get; set; }
            public Times CourierBOrchestratorCalledTimes { get; set; }
            public Times PublishCalculatedPriceActivityCalledTimes { get; set; }

        }

    }
}
