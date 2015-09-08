
### Get git build number
```cmd
git rev-list --count HEAD
```

### Pack nuget package
```cmd
nuget pack ActionQueue.csproj -Prop Configuration=Release
```

### Push nuget package
```cmd
nuget push ActionQueue.nupkg
```