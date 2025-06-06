# OnBeforePhysicsUpdate

## NON Blocking

    private async Task<TaskCompletionSource<bool>> MyOnBeforePhysicsUpdateEventHook()
    {

      Task.Run(async () =>{
  
        await Task.Delay(5000);
    
        Debugger.Log("Hello World");
    
      });
      
      return null;
    }



## Blocking

    private Task<TaskCompletionSource<bool>> MyOnBeforePhysicsUpdateEventHook()
    {
        TaskCompletionSource<bool> tcs = new();
        Task.Run(async () =>
        {
            await Task.Delay(5000);
            Debugger.Log("Hello World");
            tcs.SetResult(true);
        });
        return Task.FromResult(tcs);
    }

# OnCustomPhysicsOperationTick

## Always Blocks

    private static Task MyCustomOperation()
    {
      Debugger.Log("doing something");
      return Task.CompletedTask;
    }



# Custom operation tips and provided options

## BeforeCommunicationsDeltaTimeSnapshot

This provides you the deltaTime from the current physics tick BEFORE the custom operations ran.

Usage example:

    Velocity += acceleration * PhysicsBuilder.BeforeCommunicationsDeltaTimeSnapshot;

This example utilizes the provided deltaTime to normalize added acceleration.

## Physics.PerTickCustomVectors

PerTickCustomVectors is a special VectorComponent group included in every Physics object. Any vectors added here will be used in the current tick to contribute towards the final force and then be discarded.

Originates from (in Engines.Physics.PB.Physics):

    public readonly List<VectorComponent> PerTickCustomVectors = new();

Usage example:

    physicsObj.PerTickCustomVectors.Add(new(false, Vector3.down, (float)Velocity));

This example uses the PerTickCustomVectors container to add in a simple one tick downward force.

## GetPhysicsObjects()

Gives you a list of all Physics objects stored on the main thread.

## Custom operation hook info

The actual physics thread starts using the 

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]

attribute. So, if you set up hooks using RuntimeInitializeOnLoadMethod, be sure to pick a later load type than the thread such as

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]

Usage example:

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Init()
    {
        PhysicsBuilder.OnCustomPhysicsOperationTick += RunGravityUpdate;
    }

In the example AfterSceneLoad load type is used so that the hook is affectively added and started after the thread is ready. This minimizes the chance of issues and is recommended.
