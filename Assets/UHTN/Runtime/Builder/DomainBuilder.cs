using System;

namespace UHTN.Builder
{
    public class DomainBuilder<T> where T : Enum
    {
        private readonly DomainBuilderCore _builder;
        private readonly TaskBuildHelper<T> _helper = new();

        public CompoundTaskBuilder<T> Root => _root;
        private CompoundTaskBuilder<T> _root;

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
            var compoundTask = _helper.CreateCompound();
            AddTask(compoundTask);
            _root = new CompoundTaskBuilder<T>(compoundTask);
        }

        public PrimitiveTaskBuilder<T> Primitive(string taskName = "")
        {
            var primitiveTask = _helper.CreatePrimitive(taskName);
            AddTask(primitiveTask);
            return new PrimitiveTaskBuilder<T>(_helper, primitiveTask);
        }

        public CompoundTaskBuilder<T> Compound(string taskName,
            DecompositionTiming decompositionTiming = DecompositionTiming.Immediate)
        {
            var compoundTask = _helper.CreateCompound(taskName, decompositionTiming);
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
            var compoundTask =
                _helper.CreateCompoundWrapped(taskName, compoundTaskBuilder.CompoundTask, decompositionTiming);
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
            var method = _helper.CreateMethod();
            return new MethodBuilder<T>(_helper, method);
        }

        private void AddTask(ITask task)
        {
            _builder.AddTask(task);
        }

        public (Domain, EnumWorldState<T>) Resolve()
        {
            var domain = _builder.Resolve();
            return (domain, new EnumWorldState<T>(_worldStateDescription));
        }
    }
}
