
### Usage
```cs
ActionQueue queue = new ActionQueue();
queue.addAction(() => {
  // Code to run
});
```

#### File access queue

##### Writing
```cs
ActionQueue fileAccessQueue = new ActionQueue();

fileAccessQueue.AddAction(() => {
  using (StreamWriter w = File.AppendText(filename))
  {
      w.WriteLine(content);
  }
});
```

##### Reading
```cs
string content = "";

Task t = fileAccessQueue.AddAction(() => {
  content = File.ReadAllText(filename);
});

Task.WaitAll(t); // wait for any reading/writing actions in queue

return content;
```