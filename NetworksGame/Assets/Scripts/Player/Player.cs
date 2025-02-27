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
        //!NetworkManager.Instance.nm_Player == this
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
                             
        Packet.Rotation[0] = transform.rotation.x;
        Packet.Rotation[1] = transform.rotation.y;
        Packet.Rotation[2] = transform.rotation.z;
        Packet.Rotation[3] = transform.rotation.w;
    }
    
    void UpdateGameObjectData()
    {
        // When not simulating an input the player RigidBody must be kinematic
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.isKinematic = true;

        Vector3 pos = new Vector3(Packet.Position[0], Packet.Position[1], Packet.Position[2]);
        Quaternion rot = new Quaternion(Packet.Rotation[0], Packet.Rotation[1], Packet.Rotation[2], Packet.Rotation[3]);
        
        if (interpolation.IsStateChanged(rb.position, pos)) rb.position = interpolation.Interpolate(rb.position, pos);
        if (interpolation.IsStateChanged(transform.rotation, rot)) transform.rotation = interpolation.Interpolate(transform.rotation, rot);

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.isKinematic = false;
    }

    IEnumerator PlayerDead()
    {
        yield return new WaitForSeconds(5f);
        gameObject.SetActive(false);
        //Destroy(gameObject);
    }
}
