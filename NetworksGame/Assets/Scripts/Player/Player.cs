using HyperStrike;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool updateGO = false;

    // DATA TO SERIALIZE FROM PLAYER
    public PlayerDataPacket Packet;

    private void Awake()
    {
        Packet = new PlayerDataPacket();
    }

    void Update()
    {
        if (updateGO)
        {
            UpdateGameObjectData();
            updateGO = false;
        }
        UpdatePlayerData();
    }

    void UpdatePlayerData()
    {
        Packet.Position[0] = this.gameObject.transform.position.x;
        Packet.Position[1] = this.gameObject.transform.position.y;
        Packet.Position[2] = this.gameObject.transform.position.z;
        
        Packet.Rotation[0] = this.gameObject.transform.eulerAngles.x;
        Packet.Rotation[1] = this.gameObject.transform.eulerAngles.y;
        Packet.Rotation[2] = this.gameObject.transform.eulerAngles.z;
    }
    
    void UpdateGameObjectData()
    {
        this.gameObject.transform.position = new Vector3(Packet.Position[0], Packet.Position[1], Packet.Position[2]);
        this.gameObject.transform.eulerAngles = new Vector3(Packet.Rotation[0], Packet.Rotation[1], Packet.Rotation[2]);
    }

    IEnumerator PlayerDead()
    {
        yield return new WaitForSeconds(5f);
        gameObject.SetActive(false);
        //Destroy(gameObject);
    }
}
