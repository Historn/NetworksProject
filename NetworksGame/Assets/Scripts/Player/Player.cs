using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
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
    public bool updateGO = true;
    public PlayerData playerData;

    // Start is called before the first frame update
    void Start()
    {
        playerData = new PlayerData(this);
        //playerMovement = this.gameObject.GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlayerData();

        if (updateGO)
        {
            SetPlayerData(playerData);
            updateGO = false;
        }
    }

    public void UpdatePlayerData()
    {
        playerData.position[0] = this.gameObject.transform.position.x;
        playerData.position[1] = this.gameObject.transform.position.y;
        playerData.position[2] = this.gameObject.transform.position.z;
    }
    
    public void SetPlayerData(PlayerData data)
    {
        this.gameObject.transform.position = new Vector3(data.position[0], data.position[1], data.position[2]);
    }
}
