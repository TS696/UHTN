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

            var domain = builder.Resolve();
            var planner = new Planner(domain, domain.CreateWorldState());
            planner.ExecutionType = PlannerExecutionType.RunUntilSuccess;
            planner.Begin();

            var tickCount = 0;
            while (planner.Tick())
            {
                tickCount++;
            }

            Assert.AreEqual(3, contextValue);
            Assert.AreEqual(2, tickCount);
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
                            builder.Primitive().Effect(TestState.A, StateEffect.Assign(1))
                                .Operator(() => Debug.Log("Method2")),
                            builder.Primitive().Operator(() => OperatorState.Failed)
                        )
                );

            var domain = builder.Resolve();
            var planner = new Planner(domain, domain.CreateWorldState());

            planner.ExecutionType = PlannerExecutionType.RunUntilSuccess;
            planner.Begin();

            var tickCount = 0;
            while (planner.Tick())
            {
                tickCount++;
            }

            planner.Dispose();

            LogAssert.Expect(LogType.Log, "Method2");
            LogAssert.Expect(LogType.Log, "Method1");
            Assert.AreEqual(2, tickCount);
        }

        [Test]
        public void MtrRePlanTest()
        {
            var builder = DomainBuilder<TestState>.Create();

            builder.Root
                .Methods(
                    builder.Method()
                        .Precondition(TestState.A, StateCondition.Equal(1))
                        .SubTasks(
                            builder.Primitive().Operator(() => Debug.Log("LogA"))
                        ),
                    builder.Method()
                        .SubTasks(
                            builder.Primitive().Operator(() => Debug.Log("LogB")),
                            builder.Primitive().Operator(() => Debug.Log("LogC")),
                            builder.Primitive().Operator(() => Debug.Log("LogD"))
                        )
                );

            var domain = builder.Resolve();
            var worldState = domain.CreateWorldState();
            var planner = new Planner(domain, worldState);
            planner.ExecutionType = PlannerExecutionType.RunUntilSuccess;
            planner.Begin();

            var tickCount = 0;
            while (planner.Tick())
            {
                tickCount++;
                if (tickCount >= 2)
                {
                    worldState.SetValue((int)TestState.A, 1);
                }
            }

            LogAssert.Expect(LogType.Log, "LogB");
            LogAssert.Expect(LogType.Log, "LogC");
            LogAssert.Expect(LogType.Log, "LogA");

            planner.Dispose();
        }

        [Test]
        public void NestedMtrRePlanTest()
        {
            var builder = DomainBuilder<TestState>.Create();

            builder.Root
                .Methods(
                    builder.Method().Precondition(TestState.A, StateCondition.Equal(2))
                        .SubTasks(
                            builder.Primitive().Operator(() => Debug.Log("LogA"))
                        ),
                    builder.Method()
                        .SubTasks(
                            builder.Compound()
                                .Methods(
                                    builder.Method().Precondition(TestState.A, StateCondition.Equal(1))
                                        .SubTasks(
                                            builder.Primitive().Operator(() => Debug.Log("LogB")),
                                            builder.Primitive().Operator(() => Debug.Log("LogC")),
                                            builder.Primitive().Operator(() => Debug.Log("LogD"))
                                        ),
                                    builder.Method()
                                        .SubTasks(
                                            builder.Primitive().Operator(() => Debug.Log("LogE")),
                                            builder.Primitive().Operator(() => Debug.Log("LogF")),
                                            builder.Primitive().Operator(() => Debug.Log("LogG"))
                                        )
                                )
                        )
                );

            var domain = builder.Resolve();
            var worldState = domain.CreateWorldState();
            var planner = new Planner(domain, worldState);
            planner.ExecutionType = PlannerExecutionType.RunUntilSuccess;
            planner.Begin();

            var tickCount = 0;
            while (planner.Tick())
            {
                tickCount++;
                if (tickCount >= 3)
                {
                    worldState.SetValue((int)TestState.A, 2);
                }
                else if (tickCount >= 2)
                {
                    worldState.SetValue((int)TestState.A, 1);
                }
            }

            LogAssert.Expect(LogType.Log, "LogE");
            LogAssert.Expect(LogType.Log, "LogF");
            LogAssert.Expect(LogType.Log, "LogB");
            LogAssert.Expect(LogType.Log, "LogA");

            planner.Dispose();
        }

        [Test]
        public void NestedDelayedPlanFailTest()
        {
            var builder = DomainBuilder<TestState>.Create();

            builder.Root
                .Methods(
                    builder.Method()
                        .SubTasks(
                            builder.Primitive().Operator(() => Debug.Log("LogA")),
                            builder.Compound(DecompositionTiming.Delayed)
                                .Methods(
                                    builder.Method().Precondition(TestState.A, StateCondition.Equal(0))
                                        .SubTasks(
                                            builder.Primitive().Operator(() => Debug.Log("LogB")),
                                            builder.Primitive().Operator(() => Debug.Log("LogC")),
                                            builder.Compound(DecompositionTiming.Delayed)
                                                .Methods(
                                                    builder.Method()
                                                        .SubTasks(
                                                            builder.Primitive().Operator(() => Debug.Log("LogD")),
                                                            builder.Primitive().Operator(() => Debug.Log("LogE")),
                                                            builder.Primitive().Precondition(TestState.A,
                                                                    StateCondition.Equal(0))
                                                                .Operator(() => Debug.Log("LogF"))
                                                        )
                                                )
                                        ),
                                    builder.Method().Precondition(TestState.A, StateCondition.Equal(1))
                                        .SubTasks(
                                            builder.Primitive().Operator(() => Debug.Log("LogG"))
                                        )
                                )
                        )
                );

            var domain = builder.Resolve();
            var worldState = domain.CreateWorldState();
            var planner = new Planner(domain, worldState);
            planner.ExecutionType = PlannerExecutionType.RunUntilSuccess;
            planner.Begin();

            var tickCount = 0;
            while (planner.Tick())
            {
                tickCount++;
                if (tickCount >= 4)
                {
                    worldState.SetValue((int)TestState.A, 1);
                }
            }

            LogAssert.Expect(LogType.Log, "LogA");
            LogAssert.Expect(LogType.Log, "LogB");
            LogAssert.Expect(LogType.Log, "LogC");
            LogAssert.Expect(LogType.Log, "LogD");
            LogAssert.Expect(LogType.Log, "LogG");

            planner.Dispose();
        }
    }
}
