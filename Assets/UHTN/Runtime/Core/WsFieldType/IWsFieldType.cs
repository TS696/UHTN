namespace UHTN
{
    public interface IWsFieldType
    {
        string ToDisplayString(int value);
        bool Validate(int value);
    }
}
