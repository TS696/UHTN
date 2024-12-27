# UHTN
Hierarchical Task Network library for unity.

This code implementation is inspired by the following literature:
- [Exploring HTN Planners through Example](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter12_Exploring_HTN_Planners_through_Example.pdf)

<br>
This library introduces the JobSystem for task decomposition processing, enabling high-speed execution.

## Installation
Add the following line directly to `Packages/manifest.json`:
```json
"com.ts696.uhtn": "https://github.com/TS696/uhtn.git?path=Assets/UHTN#0.1.1"
```

## Basic Usage
First, define the world state using an enum:
```csharp
private enum WorldState 
{
    [WsFieldHint(typeof(WsFieldBool))]
    HasKey,
    [WsFieldHint(typeof(WsFieldBool))]
    DoorIsOpen,
}
```
Next, create a domain instance using `DomainBuilder`:
```csharp
var builder = DomainBuilder<WorldState>.Create();
builder.Root.Methods(
    builder.Method().Precondition(WorldState.DoorIsOpen, StateCondition.Equal(true))
        .SubTasks(
            builder.Primitive()
                .Operator(() => Debug.Log("Escaped!"))
        ),
    builder.Method().Precondition(WorldState.HasKey, StateCondition.Equal(true))
        .SubTasks(
            builder.Primitive()
                .Effect(WorldState.DoorIsOpen, StateEffect.Assign(true))
                .Operator(() => Debug.Log("Open door")),
            builder.Root
        ),
    builder.Method()
        .SubTasks(
            builder.Primitive()
                .Effect(WorldState.HasKey, StateEffect.Assign(true))
                .Operator(() => Debug.Log("Get key")),
            builder.Root
        )
);

var domain = builder.Resolve();
```

Create and set the initial values for the world state:
```csharp
var worldState = domain.CreateWorldState();
worldState.SetValue((int)WorldState.HasKey, 0);
worldState.SetValue((int)WorldState.DoorIsOpen, 0);
```

Finally create a Planner and run:
```csharp
var planner = new Planner(domain, worldState);
planner.Begin();
while (planner.IsRunning)
{
    planner.Tick();
}

planner.Dispose();
```

When you run this code, the following content should be displayed in the Console:
```
Get key
Open door
Escaped!
```

### Partial planning
Partial planning can be achieved by adding `DecompositionTiming.Delayed` to compound task:
```csharp
builder.Root.Methods(
   builder.Method().SubTasks(
       builder.Primitive().Operator(() => Debug.Log("Operator1")),
       builder.Primitive().Operator(() => Debug.Log("Operator2")),
       builder.Compound(DecompositionTiming.Delayed).Methods(
           builder.Method().SubTasks(
               builder.Primitive().Operator(() => Debug.Log("Operator3"))
           )
       )
   )
);
```
### Agent
By using the `HtnAgent` component, you can execute Domains based on the `MonoBehaviour`'s Update loop. Additionally, the `HtnAgent` includes a sensor module designed to continuously update the `WorldState`.

```csharp
public class HasKeySensor : IBoolSensor
{
    private readonly Key _key;

    public HasKeySensor(Key key)
    {
        _key = key;
    }

    public bool Update(bool current)
    {
        return _key.IsAcquired;
    }

    public SensorUpdateMode UpdateMode => SensorUpdateMode.EveryTick;
}
```

```csharp
public class SampleBehaviour : MonoBehaviour
{
    [SerializeField]
    private HtnAgent _htnAgent;

    void Start()
    {
        var builder = // create domain
        ...

        var domain = builder.Resolve();
        _htnAgent.Prepare(domain);

        _htnAgent.SensorContainer.AddSensor((int)WorldState.HasKey, new HasKeySensor(_key));

        _htnAgent.Run();
    }
}
```

### DomainAsset
You can define a domain in the Inspector using the `DomainAsset`, a `ScriptableObject` generated by navigating to **Assets > Create > UHTN > DomainAsset**.

![image](https://github.com/user-attachments/assets/2dc009f4-912f-4209-bd9e-95cd196a5228)

The above image shows how a `DomainAsset` can be defined directly within the Unity Inspector.

Sensors are defined using the `ISensorCreator`:
```csharp
[SensorCreator("Sample_OpenDoor/HasKey", typeof(WsFieldBool))]
[Serializable]
public class HasKeySensorCreator : ISensorCreator
{
    [SerializeField]
    private Sample_OpenDoor_DomainAsset.KeyType _keyType;

    public ISensor CreateSensor(object userData)
    {
        var context = userData as Sample_OpenDoor_DomainAsset;
        var key = context.GetKey(_keyType);
        return new GameObjectActiveSensor(key, true);
    }
}
```

Tasks are implemented by inheriting from the `TaskAsset` class:
```csharp
[PrimitiveTaskAsset("Sample_OpenDoor/GetKey")]
public class GetKeyTaskAsset : PrimitiveTaskAssetBase
{
    [SerializeField]
    private Sample_OpenDoor_DomainAsset.KeyType _keyType;

    protected override IOperator CreateOperator(object userData)
    {
        var context = userData as Sample_OpenDoor_DomainAsset;
        var key = context.GetKey(_keyType);
        return new GetKeyOperator(context.Agent, key);
    }
    ...
```
![image](https://github.com/user-attachments/assets/03bffb3e-b70f-404d-85d9-04bbbd06cb05)

The Domain defined in the DomainAsset operates through the `DomainAssetAgent`.
 ```csharp
 var userData = // user-defined context
 _domainAssetAgent.Prepare(userData);
 _domainAssetAgent.Run();
``` 
## License
This library is under the MIT License.
