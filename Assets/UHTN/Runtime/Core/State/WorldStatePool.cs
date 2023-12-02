using System.Collections.Generic;

namespace UHTN
{
    public class WorldStatePool
    {
        private readonly Dictionary<int, Stack<WorldState>> _pool = new();

        public WorldState Rent(int stateLength)
        {
            var stack = GetStack(stateLength);
            if (stack.Count > 0)
            {
                return stack.Pop();
            }

            return new WorldState(stateLength);
        }

        public void Return(WorldState state)
        {
            var stack = GetStack(state.StateLength);
            stack.Push(state);
        }

        private Stack<WorldState> GetStack(int stateLength)
        {
            if (!_pool.TryGetValue(stateLength, out var stack))
            {
                stack = new Stack<WorldState>();
                _pool.Add(stateLength, stack);
            }

            return stack;
        }
    }
}
