using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float crouchSpeed = 2f;
    public float rotationSpeed = 5f;
    private bool isSprinting = false;

    [Header("Jump Settings")]
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    private Vector3 velocity;
    private bool isGrounded;

    [Header("Look Settings")]
    public float lookSensitivity = 2f;
    public float maxLookAngle = 85f;

    [Header("Crouch Settings")]
    public float normalHeight = 2f;
    public float crouchHeight = 1f;
    private bool isCrouching = false;
    private float targetHeight;
    private float crouchTransitionSpeed = 5f;

    [Header("References")]
    private PlayerInput playerInput;
    private CharacterController characterController;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Transform cameraTransform;

    private float xRotation = 0f;
    public float interactRange = 5f;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        characterController = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;

        if (gameObject.tag != "Player")
        {
            gameObject.tag = "Player";
        }

        

    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isCrouching = false; 
        targetHeight = normalHeight;
        characterController.height = normalHeight; 
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded) 
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void Update()
    {
        CheckGround();
        HandleMovement();
        HandleLook();
        HandleCrouch();
        HandleSprint();
        ApplyGravity();

        if (playerInput.actions["Interact"].WasPressedThisFrame())
        {
            InteractWithObject();
        }
    }

    private void InteractWithObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)); // Center of screen
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            if (hit.collider.CompareTag("Door")) // Check if object has "Door" tag
            {
                Debug.Log("Interacting with Door: " + hit.collider.name);

                // Try getting an animator component from the door
                Animator doorAnimator = hit.collider.GetComponentInChildren<Animator>();
                DoorScript doorScript = hit.collider.GetComponent<DoorScript>(); 

                if (doorAnimator != null)
                {
                    if (doorScript.GetDoorStatus())
                    {
                        bool isOpen = doorAnimator.GetBool("IsDoorOpen"); // Get current door state
                        doorAnimator.SetBool("IsDoorOpen", !isOpen); // Toggle door state
                    }
                }
            }
            else
            {
                Debug.Log("No door detected. Hit: " + hit.collider.name);
            }
        }
    }

    private void HandleMovement()
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

       
        float currentSpeed = walkSpeed;
        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else if (isSprinting && !isCrouching) 
        {
            currentSpeed = sprintSpeed;
        }

        Vector3 movement = (forward * moveInput.y + right * moveInput.x) * currentSpeed;
        characterController.Move(movement * Time.deltaTime);
    }

    private void HandleLook()
    {
        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        
       
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void HandleCrouch()
    {
        var crouchAction = playerInput.actions["Crouch"];

        if (crouchAction.IsPressed()) 
        {
            if (!isCrouching)
            {
                isCrouching = true;
                targetHeight = crouchHeight;
            }
        }
        else 
        {
            if (isCrouching)
            {
                if (!Physics.Raycast(transform.position, Vector3.up, normalHeight))
                {
                    isCrouching = false;
                    targetHeight = normalHeight;
                }
            }
        }

        characterController.height = Mathf.Lerp(characterController.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
    }

    private void HandleSprint()
    {
        var sprintAction = playerInput.actions["Sprint"];

        if (sprintAction.IsPressed()) 
        {
            if (!isSprinting)
            {
                isSprinting = true;
            }
        }
        else 
        {
            if (isSprinting)
            {
                isSprinting = false;
            }
        }
    }

    private void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;  
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        characterController.Move(new Vector3(0, velocity.y * Time.deltaTime, 0));
    }


    private void CheckGround()
    {
        float rayLength = characterController.height / 2 + 0.1f; 
        isGrounded = Physics.Raycast(transform.position, Vector3.down, rayLength);
    }

    private float GetGroundHeight()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity))
        {
            return hit.point.y + characterController.height / 2;
        }
        return transform.position.y; // Fallback if no ground detected
    }


}
