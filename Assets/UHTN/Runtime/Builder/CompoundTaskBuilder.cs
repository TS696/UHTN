using System;

namespace UHTN.Builder
{
    public class CompoundTaskBuilder<T> : ITaskBuilder where T : Enum
    {
        internal CompoundTask CompoundTask { get; }

        ITask ITaskBuilder.Task => CompoundTask;

        private readonly DecompositionTiming _decompositionTiming;
        DecompositionTiming ITaskBuilder.DecompositionTiming => _decompositionTiming;

        public CompoundTaskBuilder(CompoundTask compoundTask, DecompositionTiming decompositionTiming)
        {
            CompoundTask = compoundTask;
            _decompositionTiming = decompositionTiming;
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
