using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public string playerName;
    public Vector3 position;
    public Transform playerTransform;
    public bool isJumping;

    public PlayerData(Transform transform, bool jumping)
    {
        playerTransform = transform;
        isJumping = jumping;
    }
}
