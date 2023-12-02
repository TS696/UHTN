using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UHTN;
using UHTN.Builder;

namespace Tests
{
    public class PlannerTest
    {
        private enum TestState
        {
            A,
        }

        [Test]
        public void RunUntilSuccessSimpleTest()
        {
            var builder = DomainBuilder<TestState>.Create();
            var contextValue = 0;

            builder.Root
                .Methods(
                    builder.Method()
                        .SubTasks(
                            builder.Primitive().Operator(() => contextValue++),
                            builder.Primitive().Operator(() => contextValue++),
                            builder.Primitive().Operator(() => contextValue++)
                        )
                );

            var (domain, worldState) = builder.Resolve();
            var planner = new Planner(domain, worldState);
            planner.ExecuteType = AgentExecuteType.RunUntilSuccess;
            planner.Begin();

            while (planner.IsRunning)
            {
                planner.Tick();
            }

            Assert.AreEqual(3, contextValue);
            planner.Dispose();
        }

        [Test]
        public void RunUntilSuccessFailOnceTest()
        {
            var builder = DomainBuilder<TestState>.Create();

            builder.Root
                .Methods(
                    builder.Method().Precondition(TestState.A, StateCondition.Equal(1))
                        .SubTasks(builder.Primitive().Operator(() => Debug.Log("Method1"))
                        ),
                    builder.Method()
                        .SubTasks(
                            builder.Primitive().Operator(() => Debug.Log("Method2")),
                            builder.Primitive().Operator(() => OperatorState.Failed)
                        )
                );

            var (domain, worldState) = builder.Resolve();
            var planner = new Planner(domain, worldState);
            var tickCount = 0;

            planner.ExecuteType = AgentExecuteType.RunUntilSuccess;
            
            planner.Begin();

            while (planner.IsRunning)
            {
                planner.Tick();

                tickCount++;
                if (tickCount == 1)
                {
                    worldState.SetValue((int)TestState.A, 1);
                }
            }
            
            planner.Dispose();

            LogAssert.Expect(LogType.Log, "Method2");
            LogAssert.Expect(LogType.Log, "Method1");
        }
    }
}
