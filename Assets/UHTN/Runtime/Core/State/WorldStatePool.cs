using System.Collections.Generic;

namespace UHTN
{
    public class WorldStatePool
    {
        private readonly Dictionary<WorldStateDescription, Stack<WorldState>> _pool = new();

        public WorldState Rent(WorldStateDescription description)
        {
            var stack = GetStack(description);
            if (stack.Count > 0)
            {
                return stack.Pop();
            }

            return description.CreateWorldState();
        }

        public void Return(WorldState state)
        {
            var stack = GetStack(state.Description);
            stack.Push(state);
        }

        private Stack<WorldState> GetStack(WorldStateDescription description)
        {
            if (!_pool.TryGetValue(description, out var stack))
            {
                stack = new Stack<WorldState>();
                _pool.Add(description, stack);
            }

            return stack;
        }
    }
}
