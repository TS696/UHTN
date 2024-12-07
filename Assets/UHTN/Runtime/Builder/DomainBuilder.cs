using System;

namespace UHTN.Builder
{
    public class DomainBuilder<T> where T : Enum
    {
        private readonly DomainBuilderCore _builder;

        public CompoundTaskBuilder<T> Root { get; private set; }

        private readonly WorldStateDescription _worldStateDescription;

        private DomainBuilder()
        {
            _worldStateDescription = EnumWorldState<T>.CreateDescription();
            _builder = new DomainBuilderCore(_worldStateDescription);
        }

        public static DomainBuilder<T> Create()
        {
            var builder = new DomainBuilder<T>();
            builder.Initialize();
            return builder;
        }

        private void Initialize()
        {
            var compoundTask = new CompoundTask(DecompositionTiming.Immediate);
            AddTask(compoundTask);
            Root = new CompoundTaskBuilder<T>(compoundTask);
        }

        public PrimitiveTaskBuilder<T> Primitive(string taskName = "")
        {
            var primitiveTask = new PrimitiveTask(taskName, _worldStateDescription.StateLength);
            AddTask(primitiveTask);
            return new PrimitiveTaskBuilder<T>(primitiveTask);
        }

        public CompoundTaskBuilder<T> Compound(string taskName,
            DecompositionTiming decompositionTiming = DecompositionTiming.Immediate)
        {
            var compoundTask = new CompoundTask(taskName, decompositionTiming);
            AddTask(compoundTask);
            return new CompoundTaskBuilder<T>(compoundTask);
        }

        public CompoundTaskBuilder<T> Compound(DecompositionTiming decompositionTiming = DecompositionTiming.Immediate)
        {
            return Compound("", decompositionTiming);
        }

        public ITaskBuilder CompoundSlot(string taskName, CompoundTaskBuilder<T> compoundTaskBuilder,
            DecompositionTiming decompositionTiming)
        {
            var compoundTask = new WrappedCompoundTask(taskName, compoundTaskBuilder.CompoundTask, decompositionTiming);
            AddTask(compoundTask);
            return new FixedTaskBuilder(compoundTask);
        }

        public ITaskBuilder CompoundSlot(CompoundTaskBuilder<T> compoundTaskBuilder,
            DecompositionTiming decompositionTiming)
        {
            return CompoundSlot("", compoundTaskBuilder, decompositionTiming);
        }


        public MethodBuilder<T> Method()
        {
            var method = new Method(_worldStateDescription.StateLength);
            return new MethodBuilder<T>(method);
        }

        private void AddTask(ITask task)
        {
            _builder.AddTask(task);
        }

        public (Domain, EnumWorldState<T>) Resolve()
        {
            var domain = _builder.Resolve();
            return (domain, new EnumWorldState<T>(domain.CreateWorldState()));
        }
    }
}
