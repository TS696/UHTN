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

            var domain = builder.Resolve();
            Assert.That(PlannerCore.PlanImmediate(domain, domain.CreateWorldState(), out _), Is.True);
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

            var domain = builder.Resolve();
            PlannerCore.PlanImmediate(domain, domain.CreateWorldState(), out var plan);
            Assert.That(plan.Tasks.Length, Is.EqualTo(1));
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

            var domain = builder.Resolve();
            PlannerCore.PlanImmediate(domain, domain.CreateWorldState(), out var plan);
            Assert.That(plan.Tasks.Length, Is.EqualTo(5));
            domain.Dispose();
        }

        [Test]
        public void MultiDecomposeTest2()
        {
            var builder = DomainBuilder<TestState>.Create();
            builder.Root
                .Methods(
                    builder.Method()
                        .SubTasks(
                            builder.Primitive()
                                .Precondition(TestState.A, StateCondition.Equal(1))
                                .Precondition(TestState.B, StateCondition.Equal(1))
                                .Precondition(TestState.C, StateCondition.Equal(1))
                        ),
                    builder.Method()
                        .SubTasks(
                            builder.Primitive().Effect(TestState.A, StateEffect.Add(1)),
                            builder.Primitive().Effect(TestState.B, StateEffect.Add(1)),
                            builder.Primitive().Effect(TestState.C, StateEffect.Add(1)),
                            builder.Root
                        )
                );

            var domain = builder.Resolve();
            PlannerCore.PlanImmediate(domain, domain.CreateWorldState(), out var plan);
            Assert.That(plan.Tasks.Length, Is.EqualTo(4));
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
            var domain = builder.Resolve();
            Assert.IsFalse(PlannerCore.PlanImmediate(domain, domain.CreateWorldState(), out _));
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

            var domain = builder.Resolve();
            var worldState = domain.CreateWorldState();
            worldState.SetValue((int)TestState.A, initialStateA);
            worldState.SetValue((int)TestState.B, initialStateB);
            PlannerCore.PlanImmediate(domain, worldState, out var plan);
            var mtr = plan.MethodTraversalRecord;
            Assert.That(mtr, Is.EquivalentTo(expectMtr));

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

            var domain = builder.Resolve();
            var worldState = domain.CreateWorldState();
            worldState.SetValue((int)TestState.A, initialStateA);
            worldState.SetValue((int)TestState.B, initialStateB);
            PlannerCore.PlanImmediate(domain, worldState, out var plan);
            var mtr = plan.MethodTraversalRecord;
            Assert.That(mtr, Is.EquivalentTo(expectMtr));

            domain.Dispose();
        }
    }
}
