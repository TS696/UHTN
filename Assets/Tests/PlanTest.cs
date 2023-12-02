using NUnit.Framework;
using UHTN;
using UHTN.Builder;

namespace Tests
{
    public class PlanTest
    {
        private enum TestState
        {
            A,
            B,
            C,
        }

        [Test]
        public void EmptyDecomposeTest()
        {
            var builder = DomainBuilder<TestState>.Create();
            builder.Root
                .Methods(
                    builder.Method()
                );

            var (domain, worldState) = builder.Resolve();
            Assert.IsTrue(PlannerCore.PlanImmediate(domain, worldState, out var _));
            domain.Dispose();
        }

        [Test]
        public void SingleDecomposeTest()
        {
            var builder = DomainBuilder<TestState>.Create();
            builder.Root
                .Methods(
                    builder.Method().Precondition(TestState.A, StateCondition.Equal(0))
                        .SubTasks(
                            builder.Primitive().Effect(TestState.A, StateEffect.Add(1))
                        )
                );

            var (domain, worldState) = builder.Resolve();
            Assert.IsTrue(PlannerCore.PlanImmediate(domain, worldState, out var plan));
            Assert.AreEqual(1, plan.Tasks.Length);
            domain.Dispose();
        }

        [Test]
        public void MultiDecomposeTest()
        {
            var builder = DomainBuilder<TestState>.Create();
            builder.Root
                .Methods(
                    builder.Method().Precondition(TestState.A, StateCondition.Equal(0))
                        .SubTasks(
                            builder.Primitive().Effect(TestState.A, StateEffect.Add(1)),
                            builder.Root
                        ),
                    builder.Method().Precondition(TestState.A, StateCondition.Equal(1))
                        .SubTasks(
                            builder.Primitive().Effect(TestState.A, StateEffect.Add(1)),
                            builder.Primitive().Effect(TestState.B, StateEffect.Add(1)),
                            builder.Root
                        ),
                    builder.Method().Precondition(TestState.B, StateCondition.Equal(1))
                        .SubTasks(
                            builder.Primitive().Effect(TestState.B, StateEffect.Add(1)),
                            builder.Primitive().Effect(TestState.C, StateEffect.Add(1))
                        )
                );

            var (domain, worldState) = builder.Resolve();
            Assert.IsTrue(PlannerCore.PlanImmediate(domain, worldState, out var plan));
            Assert.AreEqual(5, plan.Tasks.Length);
            domain.Dispose();
        }

        [Test]
        public void DecomposeFailTest()
        {
            var builder = DomainBuilder<TestState>.Create();
            builder.Root.Methods(
                builder.Method().Precondition(TestState.A, StateCondition.Equal(1))
                    .SubTasks(
                        builder.Primitive().Effect(TestState.A, StateEffect.Add(1))
                    )
            );
            var (domain, worldState) = builder.Resolve();
            Assert.IsFalse(PlannerCore.PlanImmediate(domain, worldState, out var plan));
            domain.Dispose();
        }

        [Test]
        public void MethodTraversalRecordTest()
        {
            var builder = DomainBuilder<TestState>.Create();
            builder.Root.Methods(
                builder.Method().Precondition(TestState.A, StateCondition.Equal(0))
                    .SubTasks(
                        builder.Primitive().Effect(TestState.A, StateEffect.Add(1)),
                        builder.Root
                    ),
                builder.Method().Precondition(TestState.A, StateCondition.Equal(1))
                    .SubTasks(
                        builder.Primitive().Effect(TestState.A, StateEffect.Add(1)),
                        builder.Primitive().Effect(TestState.B, StateEffect.Add(1)),
                        builder.Root
                    ),
                builder.Method().Precondition(TestState.B, StateCondition.Equal(1))
                    .SubTasks(
                        builder.Primitive().Effect(TestState.B, StateEffect.Add(1)),
                        builder.Primitive().Effect(TestState.C, StateEffect.Add(1))
                    )
            );

            var (domain, worldState) = builder.Resolve();
            PlannerCore.PlanImmediate(domain, worldState, out var plan);
            var mtr = plan.MethodTraversalRecord;
            Assert.AreEqual(0, mtr[0]);
            Assert.AreEqual(1, mtr[1]);
            Assert.AreEqual(2, mtr[2]);

            domain.Dispose();
        }
    }
}
