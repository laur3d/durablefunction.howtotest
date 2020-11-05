using System;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;

namespace durablefunction.howtotest.tests.helper
{
  /// <summary>
    /// The fluent McMocker.
    /// </summary>
    public class FluentMcMocker : McMocker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMcMocker"/> class.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        public FluentMcMocker(Mock<IDurableOrchestrationContext> context, Action<string> logger = null)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Random Helper.
        /// </summary>
        /// <returns></returns>
        public FluentMcMocker With()
        {
            return this;
        }

        /// <summary>
        /// Syntax Sugar.
        /// </summary>
        /// <returns></returns>
        public FluentMcMocker And()
        {
            return this;
        }

        /// <summary>
        /// Syntax sugar.
        /// </summary>
        /// <returns></returns>
        public FluentMcMocker CheckThat()
        {
            return this;
        }

        /// <summary>
        /// Activity.
        /// </summary>
        /// <param name="activityName"></param>
        /// <param name="returns"></param>
        /// <param name="details"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public new FluentMcMocker Activity<T>(string activityName, Func<T> returns, string details = "")
        {
            base.Activity<T>(activityName, returns, details);
            return this;
        }

        /// <summary>
        /// Activity.
        /// </summary>
        /// <param name="activityName"></param>
        /// <param name="details"></param>
        /// <returns></returns>
        public new FluentMcMocker Activity(string activityName, string details = "")
        {
            base.Activity(activityName, details);
            return this;
        }

        /// <summary>
        /// ActivityWithRetry.
        /// </summary>
        /// <param name="activityName"></param>
        /// <param name="returns"></param>
        /// <param name="details"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public new FluentMcMocker ActivityWithRetry<T>(string activityName, Func<T> returns, string details = "")
        {
            base.ActivityWithRetry<T>(activityName, returns, details);
            return this;
        }

        /// <summary>
        /// ActivityWithRetry.
        /// </summary>
        /// <param name="activityName"></param>
        /// <param name="details"></param>
        /// <returns></returns>
        public new FluentMcMocker ActivityWithRetry(string activityName, string details = "")
        {
            base.ActivityWithRetry(activityName, details);
            return this;
        }

        /// <summary>
        /// Run.
        /// </summary>
        /// <param name="runnable"></param>
        /// <returns></returns>
        public FluentMcMocker Run(Action runnable)
        {
            runnable();
            return this;
        }

        /// <summary>
        /// WasCalled.
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="times"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public FluentMcMocker WasCalled<T>(string actionName, Func<Times> times)
        {
            this.Called<T>(actionName, times);
            return this;
        }

        /// <summary>
        /// WasCalled.
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        public FluentMcMocker WasCalled(string actionName, Func<Times> times)
        {
            this.Called(actionName, times);
            return this;
        }

        /// <summary>
        /// WasCalledWithRetry.
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        public FluentMcMocker WasCalledWithRetry(string actionName, Func<Times> times)
        {
            this.CalledWithRetry(actionName, times);
            return this;
        }

        /// <summary>
        /// WasCalledWithRetry.
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="times"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public FluentMcMocker WasCalledWithRetry<T>(string actionName, Func<Times> times)
        {
            this.CalledWithRetry<T>(actionName, times);
            return this;
        }

        /// <summary>
        /// Mock a suborchestrator.
        /// </summary>
        /// <param name="orchestratorName"></param>
        /// <param name="details"></param>
        /// <returns></returns>
        public new FluentMcMocker SubOrchestrator(string orchestratorName, string details = "")
        {
            base.SubOrchestrator(orchestratorName, details);
            return this;
        }

        /// <summary>
        /// Mock a SubOrchestrator.
        /// </summary>
        /// <param name="orchestratorName"></param>
        /// <param name="returns"></param>
        /// <param name="details"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public new FluentMcMocker SubOrchestrator<T>(string orchestratorName, Func<T> returns, string details = "")
        {
            base.SubOrchestrator<T>(orchestratorName, returns, details);
            return this;
        }

        /// <summary>
        /// SubOrchestratorCalled.
        /// </summary>
        /// <param name="orchestratorName"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        public new FluentMcMocker SubOrchestratorCalled(string orchestratorName, Func<Times> times)
        {
            base.SubOrchestratorCalled(orchestratorName, times);
            return this;
        }

        /// <summary>
        /// SubOrchestratorCalled.
        /// </summary>
        /// <param name="orchestratorName"></param>
        /// <param name="times"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public new FluentMcMocker SubOrchestratorCalled<T>(string orchestratorName, Func<Times> times)
        {
            base.SubOrchestratorCalled<T>(orchestratorName, times);
            return this;
        }
    }
}
