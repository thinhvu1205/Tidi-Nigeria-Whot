using UnityEngine;
using System;
using System.Collections.Generic;

public class UnityMainThreadDispatcher : Singleton<UnityMainThreadDispatcher>
{
    private static readonly Queue<Action> _executionQueue = new();

    public void Enqueue(Action action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }

    void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }
}