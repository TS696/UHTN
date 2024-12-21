using UHTN.Agent;

namespace UHTN.DomainAssets
{
    public interface ISensorCreator
    {
        ISensor CreateSensor(object userData);
    }
}
