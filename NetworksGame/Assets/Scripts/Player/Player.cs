using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    public Player(Player player)
    {
        this.id = player.id;
        this.playerData = player.playerData;
        this.state = player.state;
    }

    public enum PlayerState
    {
        IDLE = 0,
        RUNNING,
        JUMPING,
        SHOOTING
    }

    public int id;
    public PlayerState state;
    public PlayerMovement playerMovement;
    public bool updateGO = false;
    public PlayerData playerData;

    // Start is called before the first frame update
    void Start()
    {
        //playerData = new PlayerData(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (updateGO)
        {
            UpdateGameObjectData();
            updateGO = false;
        }
        UpdatePlayerData();
    }

    public void UpdatePlayerData()
    {
        playerData.position[0] = this.gameObject.transform.position.x;
        playerData.position[1] = this.gameObject.transform.position.y;
        playerData.position[2] = this.gameObject.transform.position.z;
    }
    
    public void UpdateGameObjectData()
    {
        this.gameObject.transform.position = new Vector3(playerData.position[0], playerData.position[1], playerData.position[2]);
    }
}
