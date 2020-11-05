using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Xunit.Abstractions;

namespace durablefunction.howtotest.tests.helper
{
    /// <summary>
    /// Helper class for mocking.
    /// </summary>
    public class McMocker
    {
        private readonly Mock<IDurableOrchestrationContext> context;
        private Action<string> logger;

        private Queue<(string action, object payload, string details)> callStack = new
            Queue<(string action, object payload, string details)>();

        private bool dumpObjects = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="McMocker"/> class.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        /// <param name="dump"></param>
        public McMocker(Mock<IDurableOrchestrationContext> context, Action<string> logger = null, bool dump = false)
        {
            this.context = context;
            var fn = new Action<ITestOutputHelper, string>((helper, message) => { helper.WriteLine(message); });

            if (logger is null)
            {
                this.logger = s => { };
            }
            else
            {
                this.logger = new Action<string>((msg) => { logger(msg); });
            }

            this.dumpObjects = dump;
        }

        /// <summary>
        /// Mocks an activity.
        /// </summary>
        /// <param name="activityName"></param>
        /// <param name="returns"></param>
        /// <param name="details"></param>
        /// <typeparam name="T"></typeparam>
        public void Activity<T>(string activityName, Func<T> returns, string details = "")
        {
            this.context.Setup(m =>
                    m.CallActivityAsync<T>(
                        activityName,
                        It.IsAny<object>()))
                .ReturnsAsync(() =>
                {
                    if (this.dumpObjects)
                    {
                        this.logger($"Calling {activityName}");
                        this.logger($"{ObjectDumper.Dump(returns())}");
                    }

                    return returns();
                }).Callback(() => { this.callStack.Enqueue((activityName, returns(), details)); });
        }

        /// <summary>
        /// Mocks an suborchestrator.
        /// </summary>
        /// <param name="subOrchestratorName"></param>
        /// <param name="returns"></param>
        /// <param name="details"></param>
        /// <typeparam name="T"></typeparam>
        public void SubOrchestrator<T>(string subOrchestratorName, Func<T> returns, string details = "")
        {
            this.context.Setup(m =>
                    m.CallSubOrchestratorAsync<T>(
                        subOrchestratorName,
                        It.IsAny<string>(),
                        It.IsAny<object>()))
                .ReturnsAsync(() =>
                {
                    if (this.dumpObjects)
                    {
                        this.logger($"Calling {subOrchestratorName}");
                        this.logger($"{ObjectDumper.Dump(returns())}");
                    }

                    return returns();
                }).Callback(() => { this.callStack.Enqueue((subOrchestratorName, returns(), details)); });
        }

        /// <summary>
        /// SubOrchestrator.
        /// </summary>
        /// <param name="subOrchestratorName"></param>
        /// <param name="details"></param>
        public void SubOrchestrator(string subOrchestratorName, string details = "")
        {
            this.context.Setup(m =>
                    m.CallSubOrchestratorAsync(
                        subOrchestratorName,
                        It.IsAny<string>(),
                        It.IsAny<object>()))
                .Callback(() => { this.callStack.Enqueue((subOrchestratorName, null, details)); });
        }

        /// <summary>
        /// Register a mock of ActivityWithRetry Type.
        /// </summary>
        /// <param name="activityName"></param>
        /// <param name="returns"></param>
        /// <param name="details"></param>
        /// <typeparam name="T"></typeparam>
        public void ActivityWithRetry<T>(string activityName, Func<T> returns, string details = "")
        {
            this.context.Setup(m =>
                m.CallActivityWithRetryAsync<T>(
                    activityName,
                    It.IsAny<RetryOptions>(),
                    It.IsAny<object>())).ReturnsAsync(() =>
            {
                if (this.dumpObjects)
                {
                    this.logger($"Calling {activityName}");
                    this.logger($"{ObjectDumper.Dump(returns())}");
                }

                return returns();
            }).Callback(() => { this.callStack.Enqueue((activityName, returns(), details)); });
        }

        /// <summary>
        /// Mocks an activity.
        /// </summary>
        /// <param name="activityName"></param>
        /// <param name="details"></param>
        public void Activity(string activityName, string details = "")
        {
            this.context.Setup(m =>
                    m.CallActivityAsync(
                        activityName,
                        It.IsAny<object>()))

                .Callback(() =>
                {
                    if (this.dumpObjects)
                    {
                        this.logger($"Calling {activityName}");
                    }

                    this.callStack.Enqueue((activityName, null, details));
                });
        }

        /// <summary>
        /// Register a mock of ActivityWithRetry Type.
        /// </summary>
        /// <param name="activityName"></param>
        /// <param name="details"></param>
        public void ActivityWithRetry(string activityName, string details = "")
        {
            this.context.Setup(m =>
                    m.CallActivityWithRetryAsync(
                        activityName,
                        It.IsAny<RetryOptions>(),
                        It.IsAny<object>()))
                .Callback(() =>
                {
                    if (this.dumpObjects)
                    {
                        this.logger($"Calling {activityName}");
                    }

                    this.callStack.Enqueue((activityName, null, details));
                });
        }

        /// <summary>
        /// Called helper function.
        /// </summary>
        /// <param name="nameOfActivity"></param>
        /// <param name="times"></param>
        /// <typeparam name="T"></typeparam>
        public void CalledWithRetry<T>(string nameOfActivity, Func<Times> times)
        {
            this.context.Verify(m => m.CallActivityWithRetryAsync<T>(
                nameOfActivity,
                It.IsAny<RetryOptions>(),
                It.IsAny<object>()), times);
        }

        /// <summary>
        /// Called helper function.
        /// </summary>
        /// <param name="nameOfActivity"></param>
        /// <param name="times"></param>
        public void CalledWithRetry(string nameOfActivity, Func<Times> times)
        {
            this.context.Verify(m => m.CallActivityWithRetryAsync(
                nameOfActivity,
                It.IsAny<RetryOptions>(),
                It.IsAny<object>()), times);
        }

        /// <summary>
        /// Called helper function.
        /// </summary>
        /// <param name="nameOfActivity"></param>
        /// <param name="times"></param>
        /// <typeparam name="T"></typeparam>
        public void Called<T>(string nameOfActivity, Func<Times> times)
        {
            this.context.Verify(m => m.CallActivityWithRetryAsync<T>(
                nameOfActivity,
                It.IsAny<RetryOptions>(),
                It.IsAny<object>()), times);
        }

        /// <summary>
        /// Called helper function.
        /// </summary>
        /// <param name="nameOfActivity"></param>
        /// <param name="times"></param>
        public void Called(string nameOfActivity, Func<Times> times)
        {
            this.context.Verify(m => m.CallActivityAsync<object>(
                nameOfActivity,
                It.IsAny<object>()), times);
        }

        /// <summary>
        /// SubOrchestratorCalled.
        /// </summary>
        /// <param name="nameOfSubOrchestrator"></param>
        /// <param name="times"></param>
        public void SubOrchestratorCalled(string nameOfSubOrchestrator, Func<Times> times)
        {
            this.context.Verify(
                m =>
                    m.CallSubOrchestratorAsync(
                        nameOfSubOrchestrator,
                        It.IsAny<string>(),
                        It.IsAny<object>()), times);
        }

        /// <summary>
        /// SubOrchestratorCalled.
        /// </summary>
        /// <param name="nameOfSubOrchestrator"></param>
        /// <param name="times"></param>
        /// <typeparam name="T"></typeparam>
        public void SubOrchestratorCalled<T>(string nameOfSubOrchestrator, Func<Times> times)
        {
            this.context.Verify(
                m =>
                    m.CallSubOrchestratorAsync<T>(
                        nameOfSubOrchestrator,
                        It.IsAny<string>(),
                        It.IsAny<object>()), times);
        }

        /// <summary>
        /// Build plant-uml.
        /// </summary>
        public void BuildDiagram()
        {
            var initial = "(*)";
            while (this.callStack.Count != 0)
            {
                var item = this.callStack.Dequeue();
                var description = string.IsNullOrWhiteSpace(item.details) ? string.Empty : $"[{item.details}]";
                this.logger($"{initial} --> {description} \"{item.action}\"");
                initial = string.Empty;
            }

            this.logger("--> (*)");
        }
    }
}
