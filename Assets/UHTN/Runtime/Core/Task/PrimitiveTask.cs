using System;
using System.Collections.Generic;

namespace UHTN
{
    public class PrimitiveTask : IPrimitiveTask
    {
        public string Name { get; } = "PrimitiveTask";
        public List<ConditionToDecompose> Preconditions { get; } = new();
        public List<EffectToDecompose> Effects { get; } = new();
        public IOperator Operator { get; private set; }
        public Action PreExecute { get; set; }
        public Action PostExecute { get; set; }

        public PrimitiveTask(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                Name = name;
            }
        }

        void IPrimitiveTask.OnPreExecute()
        {
            PreExecute?.Invoke();
        }

        void IPrimitiveTask.OnPostExecute()
        {
            PostExecute?.Invoke();
        }

        public void SetOperator(IOperator op)
        {
            Operator = op;
        }
    }
}
