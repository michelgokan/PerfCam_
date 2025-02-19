using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [HideInInspector] public float walkSpeed;
    [HideInInspector] public float sprintSpeed;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;
    [Header("Camera Reference")]
    public Transform playerTransform;

    float horizontalInput;
    float verticalInput;
    int fire1Input;
    int fire2Input;
    int fire3Input;
    int jumpInput;

    Vector3 moveDirection;

    Rigidbody rb;

    [Header("Tilt Settings")]
    public float tiltAngle = 10f; // Angle to tilt per input

    // Define some camera height presets
    private float normalCamHeight = 1.8f;
    // private float crouchCamHeight = 1.0f;
    private float jumpCamHeight = 2.0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        rb.constraints = RigidbodyConstraints.None;

        // Ensure we start at normal camera height
        if (playerTransform != null)
        {
            Vector3 camPos = playerTransform.localPosition;
            camPos.y = normalCamHeight;
            playerTransform.localPosition = camPos;
        }
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        MyInput();
        SpeedControl();
        // Tilt();

        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput() 
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        fire1Input = Input.GetButton("Fire1") ? 1 : 0;
        fire2Input = Input.GetButton("Fire2") ? 1 : 0;
        fire3Input = Input.GetButton("Fire3") ? 1 : 0;
        jumpInput = Input.GetButton("Jump") ? 1 : 0;
        // Debug.Log("horizontal " + horizontalInput);
        // Debug.Log("vertical " + verticalInput);
        // Debug.Log("fire1 " + fire1Input);
        // Debug.Log("fire2 " + fire2Input);
        // Debug.Log("fire3 " + fire3Input);
        // Debug.Log("jump " + jumpInput);

         // Camera height adjustments
        if (playerTransform != null)
        {
            Vector3 camPos = playerTransform.localPosition;

            if (fire3Input == 1)
            {
                // Move camera down (crouch)
                camPos.y -= 0.1f;
            }
            else if (jumpInput == 1)
            {
                // Normal stance
                camPos.y += 0.1f;
            }

            // If jump is pressed (and on the ground), we can move camera up for a moment
            // If you only want the camera up while jump is held, uncomment the if:
            if (jumpInput == 1 && grounded)
            {
                camPos.y = jumpCamHeight;
            }
            playerTransform.localPosition = camPos;
        }

        // when to jump
        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on ground
        if(grounded) {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }

        // in air
        else if(!grounded) {
            Vector3 force = moveDirection.normalized * moveSpeed * 10f * airMultiplier;
            rb.AddForce(force, ForceMode.Force);
        }

        if(horizontalInput == 0 && verticalInput == 0){
            rb.velocity = Vector3.zero;
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
    }

    private void Tilt()
    {   
        Vector3 tiltRight = Vector3.forward * tiltAngle * fire1Input;
        Vector3 tiltLeft = Vector3.forward * -tiltAngle * fire2Input;

        if(fire1Input == 1){
            rb.AddTorque(tiltRight, ForceMode.Force);
            // transform.Rotate(Vector3.forward, tiltAngle * Time.deltaTime);
            Debug.Log("rotating left" + tiltAngle * Time.deltaTime);
        }
        else if (fire2Input == 1) {
            rb.AddTorque(tiltLeft, ForceMode.Force);
            // transform.Rotate(Vector3.forward, -tiltAngle * Time.deltaTime);
            Debug.Log("rotating right" + tiltAngle * Time.deltaTime);
        }
        else {
            rb.angularVelocity = Vector3.zero;
        }

        
        // rb.angularVelocity(tiltLeft);
        // rb.angularVelocity(tiltRight);

        Debug.Log("tilt right: " + tiltRight);
        Debug.Log("tilt left: " + tiltLeft);
    }
}