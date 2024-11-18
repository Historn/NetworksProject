using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
public class MainThreadInvoker : MonoBehaviour
{
    private static readonly Queue<Action> actions = new Queue<Action>();

    public static void Invoke(Action action)
    {
        lock (actions)
        {
            actions.Enqueue(action);
        }
    }

    void Update()
    {
        lock (actions)
        {
            while (actions.Count > 0)
            {
                actions.Dequeue()?.Invoke();
            }
        }
    }
}
