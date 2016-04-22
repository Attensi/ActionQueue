
### Get git build number
```bash
git rev-list --count HEAD
```

### Pack nuget package
```bash
nuget pack ActionQueue.csproj -Prop Configuration=Release
```

### Push nuget package
```bash
nuget push ActionQueue.nupkg
```

### Usage
```c#
ActionQueue queue = new ActionQueue();
queue.addAction(() => {
  // Code to run
});
```