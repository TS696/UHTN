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
            planner.ExecutionType = PlannerExecutionType.RunUntilSuccess;
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

            planner.ExecutionType = PlannerExecutionType.RunUntilSuccess;

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

            var (domain, worldState) = builder.Resolve();
            var planner = new Planner(domain, worldState);
            planner.ExecutionType = PlannerExecutionType.RunUntilSuccess;
            planner.Begin();

            var count = 0;

            while (planner.IsRunning)
            {
                count++;
                if (count >= 3)
                {
                    worldState.SetInt(TestState.A, 1);
                }

                planner.Tick();
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

            var (domain, worldState) = builder.Resolve();
            var planner = new Planner(domain, worldState);
            planner.ExecutionType = PlannerExecutionType.RunUntilSuccess;
            planner.Begin();

            var count = 0;
            while (planner.IsRunning)
            {
                count++;
                if (count >= 4)
                {
                    worldState.SetInt(TestState.A, 2);
                }
                else if (count >= 3)
                {
                    worldState.SetInt(TestState.A, 1);
                }

                planner.Tick();
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
                            builder.Primitive().Operator(() => Debug.Log($"LogA")),
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

            var (domain, worldState) = builder.Resolve();
            var planner = new Planner(domain, worldState);
            planner.ExecutionType = PlannerExecutionType.RunUntilSuccess;
            planner.Begin();

            var count = 0;
            while (planner.IsRunning && count < 100)
            {
                count++;
                if (count >= 5)
                {
                    worldState.SetInt(TestState.A, 1);
                }

                planner.Tick();
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
