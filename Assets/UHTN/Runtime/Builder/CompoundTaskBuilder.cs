using System;

namespace UHTN.Builder
{
    public class CompoundTaskBuilder<T> : ITaskBuilder where T : Enum
    {
        private readonly CompoundTask _compoundTask;
        internal CompoundTask CompoundTask => _compoundTask;

        ITask ITaskBuilder.Task => _compoundTask;

        public CompoundTaskBuilder(CompoundTask compoundTask)
        {
            _compoundTask = compoundTask;
        }

        public CompoundTaskBuilder<T> Methods(params MethodBuilder<T>[] methodBuilders)
        {
            foreach (var methodBuilder in methodBuilders)
            {
                _compoundTask.Methods.Add(methodBuilder.Method);
            }

            return this;
        }
    }
}
