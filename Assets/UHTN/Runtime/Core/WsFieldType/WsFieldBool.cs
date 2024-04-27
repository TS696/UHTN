namespace UHTN
{
    public class WsFieldBool : IWsFieldType
    {
        public string ToDisplayString(int value)
        {
            var flag = value >= 1;
            return flag.ToString();
        }
    }
}
