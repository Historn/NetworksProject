using HyperStrike;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    protected float speed = 1f;
    protected float damage = 1f;

    public ProjectilePacket Packet;
    public bool updateGO = false;

    private void Awake()
    {
        Packet = new ProjectilePacket();
        //NetworkManager.Instance.nm_ProjectilesToSend.Add(Packet.ProjectileId, this);
    }

    public abstract void Move();
    public abstract void ApplyDamage(GameObject collidedGO);

    void UpdateGameObjectData()
    {
        this.transform.position = new Vector3(Packet.Position[0], Packet.Position[1], Packet.Position[2]);
        this.transform.eulerAngles = new Vector3(Packet.Rotation[0], Packet.Rotation[1], Packet.Rotation[2]);
    }

    public void SetPacket(int id, int shooterId,Vector3 pos, Quaternion rot)
    {
        Packet.ProjectileId = id;
        Packet.ShooterId = shooterId;

        Packet.Position[0] = pos.x;
        Packet.Position[1] = pos.y;
        Packet.Position[2] = pos.z;

        Packet.Rotation[0] = rot.x;
        Packet.Rotation[1] = rot.y;
        Packet.Rotation[2] = rot.z;
        Packet.Rotation[3] = rot.w;
    }
}
