using NUnit.Framework;
using System.Linq;
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
            Assert.IsTrue(PlannerCore.PlanImmediate(domain, worldState.Value, out _));
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
            Assert.IsTrue(PlannerCore.PlanImmediate(domain, worldState.Value, out var plan));
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
            Assert.IsTrue(PlannerCore.PlanImmediate(domain, worldState.Value, out var plan));
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
            Assert.IsFalse(PlannerCore.PlanImmediate(domain, worldState.Value, out var plan));
            domain.Dispose();
        }

        [TestCase(0, 0, 0, 1, 2)]
        [TestCase(1, 0, 1, 2)]
        [TestCase(2, 1, 2)]
        public void MethodTraversalRecordTest(int initialStateA, int initialStateB, params int[] expectMtr)
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
            worldState.SetInt(TestState.A, initialStateA);
            worldState.SetInt(TestState.B, initialStateB);
            PlannerCore.PlanImmediate(domain, worldState.Value, out var plan);
            var mtr = plan.MethodTraversalRecord;
            Assert.True(mtr.SequenceEqual(expectMtr));

            domain.Dispose();
        }

        [TestCase(0, 0, 0, 1, 2)]
        [TestCase(1, 0, 1, 2)]
        [TestCase(2, 1, 2)]
        public void MethodTraversalRecordTest2(int initialStateA, int initialStateB, params int[] expectMtr)
        {
            var builder = DomainBuilder<TestState>.Create();
            builder.Root.Methods(
                builder.Method()
                    .SubTasks(
                        builder.Primitive()
                            .Precondition(TestState.A, StateCondition.Equal(0))
                            .Effect(TestState.A, StateEffect.Add(1)),
                        builder.Root
                    ),
                builder.Method()
                    .SubTasks(
                        builder.Primitive()
                            .Precondition(TestState.A, StateCondition.Equal(1))
                            .Effect(TestState.A, StateEffect.Add(1)),
                        builder.Primitive().Effect(TestState.B, StateEffect.Add(1)),
                        builder.Root
                    ),
                builder.Method()
                    .SubTasks(
                        builder.Primitive()
                            .Precondition(TestState.B, StateCondition.Equal(1))
                            .Effect(TestState.B, StateEffect.Add(1)),
                        builder.Primitive().Effect(TestState.C, StateEffect.Add(1))
                    )
            );

            var (domain, worldState) = builder.Resolve();
            worldState.SetInt(TestState.A, initialStateA);
            worldState.SetInt(TestState.B, initialStateB);
            PlannerCore.PlanImmediate(domain, worldState.Value, out var plan);
            var mtr = plan.MethodTraversalRecord;
            Assert.True(mtr.SequenceEqual(expectMtr));

            domain.Dispose();
        }
    }
}
