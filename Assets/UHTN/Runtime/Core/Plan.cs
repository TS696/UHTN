using System.Collections.Generic;
using System.Linq;

namespace UHTN
{
    public class Plan
    {
        public int TargetTaskIndex { get; }

        private readonly int[] _tasks;
        public int[] Tasks => _tasks;

        private readonly int[] _methodTraversalRecord;
        public int[] MethodTraversalRecord => _methodTraversalRecord;

        public readonly List<(int taskIndex, Plan plan)> PartialPlans = new();

        public bool IsEmpty => _tasks.Length == 0;

        public Plan(int[] tasks, int targetTaskIndex, int[] methodTraversalRecord)
        {
            _tasks = tasks;
            TargetTaskIndex = targetTaskIndex;
            _methodTraversalRecord = methodTraversalRecord;
        }

        public override string ToString()
        {
            var tasks = string.Join(",", _tasks.Select(x => x.ToString()));
            var mtr = string.Join(",", _methodTraversalRecord.Select(x => x.ToString()));
            return $"Tasks: {tasks}\nMTR: {mtr}";
        }
    }
}
