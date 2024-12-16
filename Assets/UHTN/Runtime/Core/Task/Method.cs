using System.Collections.Generic;

namespace UHTN
{
    public class Method : IMethod
    {
        public List<ConditionToDecompose> Preconditions { get; } = new();

        public List<SubTask> SubTasks { get; } = new();
    }
}
