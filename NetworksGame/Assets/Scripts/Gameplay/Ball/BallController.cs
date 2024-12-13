using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Physics Properties")]
    public float gravityMultiplier = 1.0f;
    public float bounciness = 0.7f;

    private Rigidbody rb;
    private PhysicMaterial ballMaterial;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

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
    }
}