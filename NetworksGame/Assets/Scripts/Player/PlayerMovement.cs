using HyperStrike;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Character Data")]
    public Character character; // Set character data, add needed data here to chara data scriptable object

    [Header("Movement")]
    public float movementSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 400f; // Sensitivity of the mouse movement

    private float xRotation = 0f; // Current x-axis rotation

    [Header("KeyBinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode attackKey = KeyCode.Mouse0;
    public KeyCode ability1Key = KeyCode.Mouse1;
    public KeyCode ability2Key = KeyCode.LeftShift;
    public KeyCode ability3Key = KeyCode.E;
    public KeyCode ultimateKey = KeyCode.Q;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask groundMask;
    public bool isGrounded;

    [Header("Wall Check")]
    public float playerWidth;
    public LayerMask wallMask;
    public bool wallrun;

    float horizontalInput;
    float verticalInput;

    [Header("Cinemachine Settings")]
    public CinemachineCamera cinemachineCamera; // Reference to the Cinemachine virtual camera
    private Transform cameraTransform; // The transform of the Cinemachine camera's LookAt target

    public GameObject playerCam;
    Vector3 moveDirection;

    Rigidbody rb;


    [Header("Attacks")]
    public GameObject rocketPrefab;
    float attackCooldown = 0.5f;
    bool attackReady = true;
    public float attackOffset = 5f; // Forward offset for the rocket spawn

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Ensure the Cinemachine camera is set up properly
        if (cinemachineCamera != null)
        {
            cameraTransform = cinemachineCamera.LookAt; // Use the LookAt target for rotation
        }

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
    }

    void Update()
    {
        //Ground Check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundMask);

        MovementInput();
        LimitSpeed();

        //Handle Drag
        if (isGrounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void MovementInput()
    {
        if (cameraTransform != null)
        {
            RotatePlayerWithCamera();
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //Jump
        if (Input.GetKey(jumpKey) && readyToJump && isGrounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);    //Delay for jump to reset
        }
        
        //Attack
        if (Input.GetKey(attackKey) && attackReady)
        {
            attackReady = false;
            Attack();
            Invoke(nameof(ResetAttack), attackCooldown);    //Delay for attack to reset
        }

    }

    void MovePlayer()
    {
        //Calculate player movement direction
        moveDirection = playerCam.transform.forward * verticalInput + playerCam.transform.right * horizontalInput;

        if (!isGrounded) //Air
        {
            rb.AddForce(moveDirection.normalized * movementSpeed * 10f * airMultiplier, ForceMode.Force);
            return;
        }
        rb.AddForce(moveDirection.normalized * movementSpeed * 10f, ForceMode.Force);
    }

    private void RotatePlayerWithCamera()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Adjust xRotation for vertical rotation and clamp it
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Update the Cinemachine camera's rotation
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Apply horizontal rotation to the player
        transform.Rotate(Vector3.up * mouseX);
    }

    void LimitSpeed()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > movementSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * movementSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    void Jump()
    {
        //Reset Y Velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void ResetJump()
    {
        readyToJump = true;
    }

    void Attack()
    {
        GameObject rocketGO = Instantiate(rocketPrefab, transform.position + playerCam.transform.forward * attackOffset, playerCam.transform.rotation);
        Projectile rocket = rocketGO.GetComponent<Projectile>();
        if (rocket != null) 
        {
            rocket.playerShooterID = gameObject.GetComponent<Player>().Packet.PlayerId;
        }
    }

    void ResetAttack()
    {
        attackReady = true;
    }

}
