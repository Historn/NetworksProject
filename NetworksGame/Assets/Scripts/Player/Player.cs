using HyperStrike;
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
        SHOOTING,
        DEAD
    }

    public int id;
    public bool updateGO = false;

    float ultimateCharge = 0f;

    // DATA TO SERIALIZE FROM PLAYER
    public PlayerState state;
    public PlayerData playerData;


    void Start()
    {

    }

    void Update()
    {
        if(playerData.health <= 0.0f)
        {
            state = PlayerState.DEAD;
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None; // When Respawning, freeze rotation again.
            //StartCoroutine("PlayerDead");
        }

        if (updateGO)
        {
            UpdateGameObjectData();
            updateGO = false;
        }
        UpdatePlayerData();
    }

    void UpdatePlayerData()
    {
        playerData.position[0] = this.gameObject.transform.position.x;
        playerData.position[1] = this.gameObject.transform.position.y;
        playerData.position[2] = this.gameObject.transform.position.z;
    }
    
    void UpdateGameObjectData()
    {
        this.gameObject.transform.position = new Vector3(playerData.position[0], playerData.position[1], playerData.position[2]);
    }

    IEnumerator PlayerDead()
    {
        yield return new WaitForSeconds(5f);
        gameObject.SetActive(false);
        //Destroy(gameObject);
    }
}
