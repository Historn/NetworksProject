using HyperStrike;
using System.Collections;
using UnityEngine;

public class Rocket : Projectile
{
    float force = 30f;
    float explosionForce = 800f;
    float radius = 20f;
    //[SerializeField] GameObject explosionFX;
    [SerializeField] Rigidbody body;

    // Start is called before the first frame update
    void Start()
    {
        damage = 20.0f;
        // Spawns from the player that shot
        body = GetComponent<Rigidbody>();

        Move();

        // ADD IENUM TO DESTROY ROCKET AFTER A PERIOD OF TIME
        StartCoroutine(DestroyRocket());
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!enabled) return;

        if (other != null) 
        {
            Explode();
        }
    }

    public override void Move()
    {
        body.AddForce(transform.forward*force, ForceMode.Impulse);
    }

    void Explode()
    {
        //Instantiate(explosionFX, transform.position, transform.rotation);

        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider collider in colliders)
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                //Debug.Log("Body Found: " + rb.name);
                rb.AddExplosionForce(explosionForce, transform.position, radius);
            }
            ApplyDamage(collider.gameObject); // Put it inside Explode()
        }

        Destroy(gameObject);
    }

    public override void ApplyDamage(GameObject collidedGO)
    {
        //Player p = collidedGO.GetComponent<Player>();
        //if (p != null && p.Packet.playerId != playerShooterID) 
        //{
        //    p.playerData.health -= damage;
        //    Debug.Log(p.name + " Health: " + p.playerData.health);
        //}

        // Add VFX
    }

    IEnumerator DestroyRocket()
    {
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
    }
}
