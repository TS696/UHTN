using System.Collections.Generic;

namespace UHTN
{
    public class CompoundTask : ICompoundTask
    {
        public TaskAttribute Attribute { get; }
        public string Name { get; } = "CompoundTask";

        public List<IMethod> Methods { get; }

        public CompoundTask(string name, DecompositionTiming decompositionTiming)
        {
            if (!string.IsNullOrEmpty(name))
            {
                Name = name;
            }

            Attribute = new TaskAttribute(TaskType.Compound, decompositionTiming);
            Methods = new List<IMethod>();
        }

        public CompoundTask(DecompositionTiming decompositionTiming) : this("", decompositionTiming)
        {
        }
    }
}
