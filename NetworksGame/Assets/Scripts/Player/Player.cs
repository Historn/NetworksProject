using HyperStrike;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool updateGO = false;

    Interpolation interpolation = new Interpolation();

    Rigidbody rb;

    // DATA TO SERIALIZE FROM PLAYER
    public PlayerDataPacket Packet;

    private void Awake()
    {
        Packet = new PlayerDataPacket();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
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
        Packet.Position[0] = rb.position.x;
        Packet.Position[1] = rb.position.y;
        Packet.Position[2] = rb.position.z;
                             
        Packet.Rotation[0] = rb.rotation.x;
        Packet.Rotation[1] = rb.rotation.y;
        Packet.Rotation[2] = rb.rotation.z;
        Packet.Rotation[3] = rb.rotation.w;
    }
    
    void UpdateGameObjectData()
    {
        Vector3 pos = new Vector3(Packet.Position[0], Packet.Position[1], Packet.Position[2]);
        Quaternion rot = new Quaternion(Packet.Rotation[0], Packet.Rotation[1], Packet.Rotation[2], Packet.Rotation[3]);
        
        if (interpolation.IsStateChanged(rb.position, pos)) rb.position = interpolation.Interpolate(rb.position, pos);
        if (interpolation.IsStateChanged(rb.rotation, rot)) rb.rotation = interpolation.Interpolate(rb.rotation, rot);
    }

    IEnumerator PlayerDead()
    {
        yield return new WaitForSeconds(5f);
        gameObject.SetActive(false);
        //Destroy(gameObject);
    }
}
