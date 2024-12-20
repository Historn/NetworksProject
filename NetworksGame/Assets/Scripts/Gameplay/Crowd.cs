using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crowd : MonoBehaviour
{
    [Range(0, 1)] public float defaultSpeed;
    [Range(0, 5)] public float cheeringSpeed;
    [Range(0, 1)] public float randomnessFactor;

    public float maximumHeight;

    [HideInInspector] public float currentSpeedFactor;

    private void Awake()
    {
        currentSpeedFactor = defaultSpeed;
    }

    private void UpdateState(string state)
    {
        switch (state)
        {
            case "idle":
                currentSpeedFactor = defaultSpeed;
                break;
            case "cheer":
                currentSpeedFactor = cheeringSpeed;
                break;
        }
    }
}
