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
        
        Packet.Rotation[0] = this.gameObject.transform.rotation.x;
        Packet.Rotation[1] = this.gameObject.transform.rotation.y;
        Packet.Rotation[2] = this.gameObject.transform.rotation.z;
        Packet.Rotation[3] = this.gameObject.transform.rotation.w;
    }
    
    void UpdateGameObjectData()
    {
        this.gameObject.transform.position = new Vector3(Packet.Position[0], Packet.Position[1], Packet.Position[2]);
        this.gameObject.transform.rotation = new Quaternion(Packet.Rotation[0], Packet.Rotation[1], Packet.Rotation[2], Packet.Rotation[3]);
    }

    IEnumerator PlayerDead()
    {
        yield return new WaitForSeconds(5f);
        gameObject.SetActive(false);
        //Destroy(gameObject);
    }
}
