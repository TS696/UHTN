using System.Collections.Generic;

namespace UHTN
{
    public interface ICompoundTask : ITask
    {
        TaskType ITask.Type => TaskType.Compound;
        List<IMethod> Methods { get; }
    }
}
