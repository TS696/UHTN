using System.Collections.Generic;

namespace UHTN
{
    public interface ICompoundTask : ITask
    {
        List<IMethod> Methods { get; }
    }
}
