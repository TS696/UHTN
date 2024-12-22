using System;
using System.Reflection;

namespace UHTN.Builder
{
    public class DomainBuilder<T> where T : Enum
    {
        private readonly DomainBuilderCore _builder;

        public CompoundTaskBuilder<T> Root { get; private set; }

        private readonly WorldStateDescription _worldStateDescription;

        private DomainBuilder()
        {
            _worldStateDescription = CreateDescription();
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
            var compoundTask = new CompoundTask();
            AddTask(compoundTask);
            Root = new CompoundTaskBuilder<T>(compoundTask, DecompositionTiming.Immediate);
        }

        public PrimitiveTaskBuilder<T> Primitive(string taskName = "")
        {
            var primitiveTask = new PrimitiveTask(taskName);
            AddTask(primitiveTask);
            return new PrimitiveTaskBuilder<T>(primitiveTask);
        }

        public CompoundTaskBuilder<T> Compound(string taskName,
            DecompositionTiming decompositionTiming = DecompositionTiming.Immediate)
        {
            var compoundTask = new CompoundTask(taskName);
            AddTask(compoundTask);
            return new CompoundTaskBuilder<T>(compoundTask, decompositionTiming);
        }

        public CompoundTaskBuilder<T> Compound(DecompositionTiming decompositionTiming = DecompositionTiming.Immediate)
        {
            return Compound("", decompositionTiming);
        }

        public ITaskBuilder CompoundSlot(CompoundTaskBuilder<T> compoundTaskBuilder,
            DecompositionTiming decompositionTiming)
        {
            return CompoundSlot(compoundTaskBuilder.CompoundTask.Name, compoundTaskBuilder, decompositionTiming);
        }

        public ITaskBuilder CompoundSlot(string name, CompoundTaskBuilder<T> compoundTaskBuilder,
            DecompositionTiming decompositionTiming)
        {
            var wrappedCompoundTask = new WrappedCompoundTask(name, compoundTaskBuilder.CompoundTask);
            AddTask(wrappedCompoundTask);
            return new FixedTaskBuilder(wrappedCompoundTask, decompositionTiming);
        }

        public MethodBuilder<T> Method()
        {
            var method = new Method();
            return new MethodBuilder<T>(method);
        }

        private void AddTask(ITask task)
        {
            _builder.AddTask(task);
        }

        public Domain Resolve()
        {
            return _builder.Resolve();
        }

        public static WorldStateDescription CreateDescription()
        {
            var names = Enum.GetNames(typeof(T));
            var fieldDescList = new WorldStateDescription.FieldDesc[names.Length];

            for (var i = 0; i < names.Length; i++)
            {
                IWsFieldType stateType = WsFieldInt.Instance;

                var fieldInfo = typeof(T).GetField(names[i]);
                var hint = fieldInfo.GetCustomAttribute<WsFieldHintAttribute>();
                if (hint != null)
                {
                    stateType = (IWsFieldType)Activator.CreateInstance(hint.Type);
                }

                fieldDescList[i] = new WorldStateDescription.FieldDesc(names[i], stateType);
            }

            return new WorldStateDescription(fieldDescList);
        }
    }
}
