using HyperStrike;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public int playerShooterID;
    public int ProjectileId;
    protected float speed = 1f;
    protected float damage = 1f;

    public ProjectilePacket Packet;
    public bool updateGO;

    private void Awake()
    {
        Packet = new ProjectilePacket();
    }

    public abstract void Move();
    public abstract void ApplyDamage(GameObject collidedGO);

}
