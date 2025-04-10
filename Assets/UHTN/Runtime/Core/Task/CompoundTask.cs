using System.Collections.Generic;

namespace UHTN
{
    public class CompoundTask : ICompoundTask
    {
        public string Name { get; } = "CompoundTask";

        public List<IMethod> Methods { get; } = new();

        public CompoundTask(string name = "")
        {
            if (!string.IsNullOrEmpty(name))
            {
                Name = name;
            }
        }
    }
}
