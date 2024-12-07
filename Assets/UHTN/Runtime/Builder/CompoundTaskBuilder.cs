using System;

namespace UHTN.Builder
{
    public class CompoundTaskBuilder<T> : ITaskBuilder where T : Enum
    {
        internal CompoundTask CompoundTask { get; }

        ITask ITaskBuilder.Task => CompoundTask;

        public CompoundTaskBuilder(CompoundTask compoundTask)
        {
            CompoundTask = compoundTask;
        }

        public CompoundTaskBuilder<T> Methods(params MethodBuilder<T>[] methodBuilders)
        {
            foreach (var methodBuilder in methodBuilders)
            {
                CompoundTask.Methods.Add(methodBuilder.Method);
            }

            return this;
        }
    }
}
