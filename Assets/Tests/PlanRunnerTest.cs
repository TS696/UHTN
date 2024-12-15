using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UHTN;
using UHTN.Builder;

namespace Tests
{
    public class PlanRunnerTest
    {
        private enum TestState
        {
            A,
        }

        [Test]
        public void EmptyRunningTest()
        {
            var builder = DomainBuilder<TestState>.Create();
            builder.Root
                .Methods(
                    builder.Method()
                );

            var domain = builder.Resolve();
            var worldState = domain.CreateWorldState();
            Assert.IsTrue(PlannerCore.PlanImmediate(domain, worldState, out var plan));

            var planRunner = new PlanRunner();
            planRunner.Begin(domain, plan, worldState);

            Assert.AreEqual(PlanRunner.RunnerState.Success, planRunner.State);
            domain.Dispose();
        }

        [Test]
        public void PlanRunnerSuccessTest()
        {
            var builder = DomainBuilder<TestState>.Create();
            var contextValue = 0;

            builder.Root
                .Methods(
                    builder.Method().Precondition(TestState.A, StateCondition.Equal(0))
                        .SubTasks(
                            builder.Primitive().Operator(() => contextValue++),
                            builder.Primitive().Operator(() => contextValue++),
                            builder.Primitive().Operator(() => contextValue++)
                        )
                );

            var domain = builder.Resolve();
            var worldState = domain.CreateWorldState();
            Assert.IsTrue(PlannerCore.PlanImmediate(domain, worldState, out var plan));

            var planRunner = new PlanRunner();
            planRunner.Begin(domain, plan, worldState);

            while (planRunner.State == PlanRunner.RunnerState.Running)
            {
                planRunner.Tick();
            }

            Assert.AreEqual(3, contextValue);
            Assert.AreEqual(PlanRunner.RunnerState.Success, planRunner.State);

            domain.Dispose();
        }

        [Test]
        public void PrePostEventTest()
        {
            var builder = DomainBuilder<TestState>.Create();

            builder.Root
                .Methods(
                    builder.Method()
                        .SubTasks(
                            builder.Primitive()
                                .PreExecute(() => Debug.Log("PreExecute1"))
                                .Operator(() => Debug.Log("Operator1"))
                                .PostExecute(() => Debug.Log("PostExecute1")),
                            builder.Primitive()
                                .PreExecute(() => Debug.Log("PreExecute2"))
                                .Operator(() => Debug.Log("Operator2"))
                                .PostExecute(() => Debug.Log("PostExecute2"))
                        )
                );

            var domain = builder.Resolve();
            var worldState = domain.CreateWorldState();
            Assert.IsTrue(PlannerCore.PlanImmediate(domain, worldState, out var plan));

            var planRunner = new PlanRunner();
            planRunner.Begin(domain, plan, worldState);

            while (planRunner.State == PlanRunner.RunnerState.Running)
            {
                planRunner.Tick();
            }

            LogAssert.Expect(LogType.Log, "PreExecute1");
            LogAssert.Expect(LogType.Log, "Operator1");
            LogAssert.Expect(LogType.Log, "PostExecute1");
            LogAssert.Expect(LogType.Log, "PreExecute2");
            LogAssert.Expect(LogType.Log, "Operator2");
            LogAssert.Expect(LogType.Log, "PostExecute2");

            domain.Dispose();
        }

        [Test]
        public void WorldStateDirtyTest()
        {
            var builder = DomainBuilder<TestState>.Create();
            var contextValue = 0;

            builder.Root
                .Methods(
                    builder.Method().Precondition(TestState.A, StateCondition.Equal(0))
                        .SubTasks(
                            builder.Primitive().Operator(() => contextValue++),
                            builder.Primitive().Operator(() => contextValue++),
                            builder.Primitive().Operator(() => contextValue++)
                        )
                );

            var domain = builder.Resolve();
            var worldState = domain.CreateWorldState();
            Assert.IsTrue(PlannerCore.PlanImmediate(domain, worldState, out var plan));

            var planRunner = new PlanRunner();
            planRunner.Begin(domain, plan, worldState);

            while (planRunner.State == PlanRunner.RunnerState.Running)
            {
                planRunner.Tick();
                worldState.SetValue((int)TestState.A, 1);
                worldState.SetValue((int)TestState.A, 0);
            }

            Assert.AreEqual(contextValue, 3);
            Assert.AreEqual(PlanRunner.RunnerState.Success, planRunner.State);

            domain.Dispose();
        }

        [Test]
        public void PlanRunnerFailTest()
        {
            var builder = DomainBuilder<TestState>.Create();

            builder.Root.Methods(
                builder.Method()
                    .SubTasks(
                        builder.Primitive().Operator(() => OperatorState.Success),
                        builder.Primitive().Operator(() => OperatorState.Failed)
                    )
            );

            var domain = builder.Resolve();
            var worldState = domain.CreateWorldState();
            Assert.IsTrue(PlannerCore.PlanImmediate(domain, worldState, out var plan));
            var planRunner = new PlanRunner();
            planRunner.Begin(domain, plan, worldState);
            while (planRunner.State == PlanRunner.RunnerState.Running)
            {
                planRunner.Tick();
            }

            Assert.AreEqual(PlanRunner.RunnerState.Failed, planRunner.State);
            domain.Dispose();
        }

        [Test]
        public void PlanRunnerFailTest2()
        {
            var builder = DomainBuilder<TestState>.Create();

            builder.Root.Methods(
                builder.Method()
                    .SubTasks(
                        builder.Primitive(),
                        builder.Primitive(),
                        builder.Primitive().Precondition(TestState.A, StateCondition.Equal(0))
                    )
            );

            var domain = builder.Resolve();
            var worldState = domain.CreateWorldState();
            Assert.IsTrue(PlannerCore.PlanImmediate(domain, worldState, out var plan));
            var planRunner = new PlanRunner();
            planRunner.Begin(domain, plan, worldState);
            var count = 0;
            while (planRunner.State == PlanRunner.RunnerState.Running)
            {
                count++;
                planRunner.Tick();

                if (count == 1)
                {
                    worldState.SetValue((int)TestState.A, 1);
                }
            }

            Assert.AreEqual(PlanRunner.RunnerState.Failed, planRunner.State);
            domain.Dispose();
        }

        [Test]
        public void DelayedDecomposeTest()
        {
            var builder = DomainBuilder<TestState>.Create();
            builder.Root.Methods(
                builder.Method().SubTasks(
                    builder.Primitive().Operator(() => Debug.Log("Operator1")),
                    builder.Primitive().Operator(() => Debug.Log("Operator2")),
                    builder.Compound(DecompositionTiming.Delayed).Methods(
                        builder.Method().SubTasks(
                            builder.Primitive(),
                            builder.Primitive().Operator(() => Debug.Log("Operator3"))
                        )
                    )
                )
            );

            var domain = builder.Resolve();
            var worldState = domain.CreateWorldState();
            Assert.IsTrue(PlannerCore.PlanImmediate(domain, worldState, out var plan));
            Assert.AreEqual(3, plan.Tasks.Length);

            var planRunner = new PlanRunner();
            planRunner.Begin(domain, plan, worldState);

            while (planRunner.State == PlanRunner.RunnerState.Running)
            {
                planRunner.Tick();
            }

            LogAssert.Expect(LogType.Log, "Operator1");
            LogAssert.Expect(LogType.Log, "Operator2");
            LogAssert.Expect(LogType.Log, "Operator3");

            domain.Dispose();
        }

        [Test]
        public void FirstTaskDelayedDecomposeTest()
        {
            var builder = DomainBuilder<TestState>.Create();
            builder.Root.Methods(
                builder.Method().SubTasks(
                    builder.Compound(DecompositionTiming.Delayed).Methods(
                        builder.Method().SubTasks(
                            builder.Primitive().Operator(() => Debug.Log("Operator1"))
                        )
                    )
                )
            );

            var domain = builder.Resolve();
            var worldState = domain.CreateWorldState();
            Assert.IsTrue(PlannerCore.PlanImmediate(domain, worldState, out var plan));
            Assert.AreEqual(1, plan.Tasks.Length);

            var planRunner = new PlanRunner();
            planRunner.Begin(domain, plan, worldState);

            while (planRunner.State == PlanRunner.RunnerState.Running)
            {
                planRunner.Tick();
            }

            LogAssert.Expect(LogType.Log, "Operator1");
            domain.Dispose();
        }

        [Test]
        public void NestedDelayedDecomposeTest()
        {
            var builder = DomainBuilder<TestState>.Create();
            builder.Root.Methods(
                builder.Method().SubTasks(
                    builder.Primitive().Operator(() => Debug.Log("Operator1")),
                    builder.Compound(DecompositionTiming.Delayed).Methods(
                        builder.Method().SubTasks(
                            builder.Compound(DecompositionTiming.Delayed).Methods(
                                builder.Method().SubTasks(
                                    builder.Primitive().Operator(() => Debug.Log("Operator2"))
                                )
                            )
                        )
                    )
                )
            );

            var domain = builder.Resolve();
            var worldState = domain.CreateWorldState();
            Assert.IsTrue(PlannerCore.PlanImmediate(domain, worldState, out var plan));
            Assert.AreEqual(2, plan.Tasks.Length);

            var planRunner = new PlanRunner();
            planRunner.Begin(domain, plan, worldState);

            while (planRunner.State == PlanRunner.RunnerState.Running)
            {
                planRunner.Tick();
            }

            LogAssert.Expect(LogType.Log, "Operator1");
            LogAssert.Expect(LogType.Log, "Operator2");

            domain.Dispose();
        }

        [Test]
        public void EmptyCompoundSlotTest()
        {
            var builder = DomainBuilder<TestState>.Create();
            var subBuilder = DomainBuilder<TestState>.Create();

            builder.Root.Methods(
                builder.Method()
                    .SubTasks(
                        builder.CompoundSlot(subBuilder.Root, DecompositionTiming.Delayed)
                    )
            );

            var domain = builder.Resolve();
            var worldState = domain.CreateWorldState();
            Assert.IsTrue(PlannerCore.PlanImmediate(domain, worldState, out var plan));
            Assert.AreEqual(1, plan.Tasks.Length);

            var planRunner = new PlanRunner();
            planRunner.Begin(domain, plan, worldState);
            while (planRunner.State == PlanRunner.RunnerState.Running)
            {
                planRunner.Tick();
            }

            domain.Dispose();
        }

        [Test]
        public void DelayWithCompoundSlotTest()
        {
            var builder = DomainBuilder<TestState>.Create();
            var num = 0;

            builder.Root.Methods(
                builder.Method()
                    .Precondition(TestState.A, StateCondition.LessThan(3))
                    .SubTasks(
                        builder.Primitive().Effect(TestState.A, StateEffect.Add(1))
                            .Operator(() => Debug.Log($"Operator{++num}")),
                        builder.CompoundSlot(builder.Root, DecompositionTiming.Delayed)
                    )
            );

            var domain = builder.Resolve();
            var worldState = domain.CreateWorldState();
            Assert.IsTrue(PlannerCore.PlanImmediate(domain, worldState, out var plan));
            Assert.AreEqual(2, plan.Tasks.Length);

            var planRunner = new PlanRunner();
            planRunner.Begin(domain, plan, worldState);

            while (planRunner.State == PlanRunner.RunnerState.Running)
            {
                planRunner.Tick();
            }

            LogAssert.Expect(LogType.Log, "Operator1");
            LogAssert.Expect(LogType.Log, "Operator2");
            LogAssert.Expect(LogType.Log, "Operator3");

            domain.Dispose();
        }
    }
}
