using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int playerId = 0;
    public string playerName = ""; // Character name
    public float health = 100f;
    public float[] position;
    //public float[] rotation;
    //public float[] scale;
    //public bool isGrounded;
    //public int playerState;

    public PlayerData(Player player)
    {
        playerId = player.id;
        playerName = player.name;

        health = 100f;
        position = new float[3];
        position[0] = player.transform.position.x;
        position[1] = player.transform.position.y;
        position[2] = player.transform.position.z;

        //rotation = new float[4];
        //rotation[0] = player.transform.rotation.x;
        //rotation[1] = player.transform.rotation.y;
        //rotation[2] = player.transform.rotation.z;
        //rotation[2] = player.transform.rotation.w;

        //scale = new float[3];
        //scale[0] = player.transform.localScale.x;
        //scale[1] = player.transform.localScale.y;
        //scale[2] = player.transform.localScale.z;

        //isGrounded = true;

        //playerState = (int)player.state;
    }

    public PlayerData()
    {
        playerId = -1;
        playerName = "{PLAYER NAME ERROR}";
        health = 100f;
        position = new float[3];
    }
}
