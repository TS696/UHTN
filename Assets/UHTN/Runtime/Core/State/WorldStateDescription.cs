namespace UHTN
{
    public class WorldStateDescription
    {
        public class FieldDesc
        {
            public string Name { get; }
            public IWsFieldType Type { get; }

            public FieldDesc(string name, IWsFieldType type)
            {
                Name = name;
                Type = type;
            }
        }

        private readonly FieldDesc[] _fieldDescList;
        public int StateLength => _fieldDescList.Length;

        public string GetStateName(int index)
        {
            return _fieldDescList[index].Name;
        }

        public IWsFieldType GetStateType(int index)
        {
            return _fieldDescList[index].Type;
        }

        public WorldStateDescription(FieldDesc[] fieldDescList)
        {
            _fieldDescList = fieldDescList;
        }

        public WorldStateDescription(string[] stateNames)
        {
            _fieldDescList = new FieldDesc[stateNames.Length];
            for (var i = 0; i < stateNames.Length; i++)
            {
                _fieldDescList[i] = new FieldDesc(stateNames[i], WsFieldInt.Instance);
            }
        }
    }
}
