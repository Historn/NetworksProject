using HyperStrike;
using Unity.VisualScripting;
using UnityEngine;

public class BallController : MonoBehaviour
{
    Interpolation interpolation = new Interpolation();
    Prediction prediction = new Prediction();

    [Header("Physics Properties")]
    public float gravityMultiplier = 1.0f;
    public float bounciness = 0.7f;

    private Rigidbody rb;
    private PhysicMaterial ballMaterial;

    public BallPacket Packet;
    public bool updateGO = false;

    private float lastUpdateTime;

    private Vector3 velocity;

    private Vector3 pitchMin = new Vector3(-11.2f, -1.95f, -16.95f); // Example pitch bounds
    private Vector3 pitchMax = new Vector3(11.2f, 4.0f, 16.95f);

    private void Awake()
    {
        Packet = new BallPacket();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //interpolation.SetBaseSpeed(1000f);
        // Create a new Physics Material for runtime changes
        ballMaterial = new PhysicMaterial
        {
            bounciness = bounciness,
            frictionCombine = PhysicMaterialCombine.Multiply,
            bounceCombine = PhysicMaterialCombine.Maximum
        };

        // Apply the material to the collider
        var collider = GetComponent<Collider>();
        if (collider != null)
            collider.material = ballMaterial;

        // Adjust initial gravity
        Physics.gravity = Vector3.down * 9.81f * gravityMultiplier;
    }

    void Update()
    {
        // Dynamically update gravity if the multiplier changes
        Physics.gravity = Vector3.down * 9.81f * gravityMultiplier;

        // Update the bounciness
        if (ballMaterial != null)
            ballMaterial.bounciness = bounciness;

        if (NetworkManager.Instance.nm_IsHost) UpdatePacket();
    }

    void FixedUpdate()
    {
        if (!NetworkManager.Instance.nm_IsHost)
        {
            // Extrapolate if the last update was more than 10 ms ago
            if (Time.time - lastUpdateTime <= 0.01f && updateGO) // Only interpolate if updates are recent
            {
                UpdateGameObjectData();
                updateGO = false;
            }
            else
            {
                // Extrapolate if no updates received in the last 10 ms
                rb.position = prediction.PredictPositionWithCollisions(rb.position, velocity, Time.fixedDeltaTime, pitchMin, pitchMax, ref velocity);
                rb.rotation = prediction.PredictRotation(rb.rotation, rb.angularVelocity, Time.fixedDeltaTime);
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!enabled) return;

        if (other != null)
        {
            Player player  = other.gameObject.GetComponent<Player>();
            if (player != null)
            {
                Packet.LastHitPlayerId = player.Packet.PlayerId;
            }
        }
    }

    void UpdateGameObjectData()
    {
        if (rb == null) return;

        Vector3 pos = new Vector3(Packet.Position[0], Packet.Position[1], Packet.Position[2]);
        Quaternion rot = new Quaternion(Packet.Rotation[0], Packet.Rotation[1], Packet.Rotation[2], Packet.Rotation[3]);

        velocity = rb.velocity;

        lastUpdateTime = Time.time;

        if (interpolation.IsStateChanged(rb.position, pos)) rb.position = interpolation.Interpolate(rb.position, pos);
        if (interpolation.IsStateChanged(rb.rotation, rot)) rb.rotation = interpolation.Interpolate(rb.rotation, rot);
    }

    void UpdatePacket()
    {
        if (rb == null) return;

        Packet.Position[0] = rb.position.x;
        Packet.Position[1] = rb.position.y;
        Packet.Position[2] = rb.position.z;

        Packet.Rotation[0] = rb.rotation.x;
        Packet.Rotation[1] = rb.rotation.y;
        Packet.Rotation[2] = rb.rotation.z;
        Packet.Rotation[3] = rb.rotation.w;
    }
}