using System.Collections.Generic;
using System.Linq;

namespace UHTN
{
    public class Plan
    {
        public int TargetTaskIndex { get; }

        public int[] Tasks { get; }

        public int[] MethodTraversalRecord { get; }

        public readonly List<(int taskIndex, Plan plan)> PartialPlans = new();

        public bool IsEmpty => Tasks.Length == 0;

        public Plan(int[] tasks, int targetTaskIndex, int[] methodTraversalRecord)
        {
            Tasks = tasks;
            TargetTaskIndex = targetTaskIndex;
            MethodTraversalRecord = methodTraversalRecord;
        }

        public override string ToString()
        {
            var tasks = string.Join(",", Tasks.Select(x => x.ToString()));
            var mtr = string.Join(",", MethodTraversalRecord.Select(x => x.ToString()));
            return $"Tasks: {tasks}\nMTR: {mtr}";
        }
    }
}
