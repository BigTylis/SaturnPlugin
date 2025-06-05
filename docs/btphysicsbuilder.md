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
