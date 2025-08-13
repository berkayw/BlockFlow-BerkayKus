using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleInstanceManager : MonoBehaviour
{
    public GameObject obstaclePrefab;
    
    private void Start()
    {
        Instantiate(obstaclePrefab, transform);
    }
}
