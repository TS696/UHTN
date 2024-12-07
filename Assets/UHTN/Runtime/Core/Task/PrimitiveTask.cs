using System;

namespace UHTN
{
    public class PrimitiveTask : IPrimitiveTask
    {
        public TaskAttribute Attribute { get; }
        public string Name { get; } = "PrimitiveTask";
        public StateCondition[] PreConditions { get; }
        public StateEffect[] Effects { get; }
        public IOperator Operator { get; private set; }
        public Action PreExecute { get; set; }
        public Action PostExecute { get; set; }

        public PrimitiveTask(string name, int stateLength)
        {
            if (!string.IsNullOrEmpty(name))
            {
                Name = name;
            }

            Attribute = new TaskAttribute(TaskType.Primitive);

            PreConditions = new StateCondition[stateLength];
            Effects = new StateEffect[stateLength];
        }

        public PrimitiveTask(int stateLength) : this("", stateLength)
        {
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
