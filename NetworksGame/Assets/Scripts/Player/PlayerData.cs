using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public string playerName; // Character
    public Vector3 position;
    public Transform playerTransform;
    public bool isJumping;
    public int userId;

    public PlayerData()
    {
        userId = 5;
    }
}
