# UHTN
Hierarchical Task Network library for unity.

This code implementation is inspired by the following literature:
- [Exploring HTN Planners through Example](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter12_Exploring_HTN_Planners_through_Example.pdf)

<br>
This library introduces the JobSystem for task decomposition processing, enabling high-speed execution.

## Installation
Add the following line directly to `Packages/manifest.json`:
```json
"com.ts696.uhtn": "https://github.com/TS696/uhtn.git?path=Assets/UHTN#0.0.10"
```

## Basic Usage
First, define the world state using an enum:
```
private enum WorldState 
{
    [WsFieldHint(typeof(WsFieldBool))]
    HasKey,
    [WsFieldHint(typeof(WsFieldBool))]
    DoorIsOpen,
}
```
Next, create a domain and a world state instance using DomainBuilder:
```
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

var (domain, worldState) = builder.Resolve();
```

Set the initial values for the world state:
```
worldState.SetBool(WorldState.HasKey, false);
worldState.SetBool(WorldState.DoorIsOpen, false);
```

Finally create a Planner and run:
```
var planner = new Planner(domain, worldState.Value);
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

## Partial planning
Partial planning can be achieved by adding `DecompositionTiming.Delayed` to compound task:
```
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

## License
This library is under the MIT License.
