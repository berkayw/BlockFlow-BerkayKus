using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEventSystem : MonoBehaviour
{
    public static GameEventSystem Instance;
    public UnityEvent BlockPlacedEvent;
    public UnityEvent<Vector3, Color> BlockDestroyedEvent;
    public UnityEvent<Vector3> IceBreakEvent;

    private void Awake()
    {
        Instance = this;
    }
    
}
