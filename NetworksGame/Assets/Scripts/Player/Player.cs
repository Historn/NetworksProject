using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public enum PlayerState
    {
        IDLE = 0,
        RUNNING,
        JUMPING,
        SHOOTING
    }

    public string characterName; // Change to character name with scriptable obj
    public PlayerState state;
    public PlayerMovement playerMovement;
    public User user;

    // Start is called before the first frame update
    void Start()
    {
        //playerMovement = this.gameObject.GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public PlayerData GetPlayerData()
    {
        return new PlayerData(this);
    }
    
    public void SetPlayerData(PlayerData data)
    {
        this.gameObject.transform.position = new Vector3(data.position[0], data.position[1], data.position[2]);
    }
}
