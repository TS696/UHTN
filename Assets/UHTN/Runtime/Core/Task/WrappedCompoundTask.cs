using System.Collections.Generic;

namespace UHTN
{
    public class WrappedCompoundTask : ICompoundTask
    {
        public TaskAttribute Attribute { get; }
        public string Name { get; } = "CompoundTask";
        public List<IMethod> Methods => _source.Methods;

        private readonly ICompoundTask _source;

        public WrappedCompoundTask(string name, ICompoundTask source, DecompositionTiming overrideTiming)
        {
            if (!string.IsNullOrEmpty(name))
            {
                Name = name;
            }

            _source = source;
            Attribute = new TaskAttribute(TaskType.Compound, overrideTiming);
        }
    }
}
