using System.Collections.Generic;

namespace UHTN
{
    public class WrappedCompoundTask : ICompoundTask
    {
        public TaskAttribute Attribute { get; }
        public string Name { get; }
        public List<IMethod> Methods => _source.Methods;

        private readonly ICompoundTask _source;

        public WrappedCompoundTask(string name, ICompoundTask source) 
        {
            Name = name;
            _source = source;
            Attribute = new TaskAttribute(TaskType.Compound);
        }
    }
}
