using System.Collections.Generic;

namespace UHTN
{
    public class Method : IMethod
    {
        public StateCondition[] PreConditions { get; }

        public List<SubTask> SubTasks { get; } = new();

        public Method(int stateLength)
        {
            PreConditions = new StateCondition[stateLength];
        }
    }
}
