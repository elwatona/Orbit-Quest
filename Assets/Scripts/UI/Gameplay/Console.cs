using UnityEngine;
using System;

public class Console : IPanel
{
    public GameObject Root {get; private set;}
    public readonly GameObjectPool LogPool;
    public readonly int LogPoolSize;
    public readonly GameObject LogPrefab;
    public readonly Transform LogContainer;
    public Console(ConsoleDependencies dependencies)
    {
        Root = dependencies.Root;
        LogPoolSize = dependencies.LogPoolSize;
        LogPrefab = dependencies.LogPrefab;
        LogContainer = dependencies.LogContainer;
        LogPool = new GameObjectPool(LogPrefab, LogPoolSize, LogContainer);
    }
    public void Log(string message, string stackTrace, LogType type)
    {
        GameObject logGO = LogPool.Get();
        LogComponent log = new LogComponent(logGO.transform);
        log.Log(message, stackTrace, type);
    }

    public void Toggle(bool active)
    {
        Root.SetActive(active);
    }
}
[Serializable]
public class ConsoleDependencies : PanelDependencies
{
    public int LogPoolSize;
    public GameObject LogPrefab;
    public Transform LogContainer;
}