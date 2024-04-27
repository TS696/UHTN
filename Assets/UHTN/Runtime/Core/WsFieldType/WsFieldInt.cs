namespace UHTN
{
    public class WsFieldInt : IWsFieldType
    {
        public static WsFieldInt Instance { get; } = new();

        public string ToDisplayString(int value)
        {
            return value.ToString();
        }
    }
}
