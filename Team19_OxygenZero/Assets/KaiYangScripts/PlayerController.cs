using System.Collections.Generic;
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

    public GameObject inventory;

    public Inventory inventorySystem;

    [SerializeField] private RaycastWeapon currentWeapon;

    private bool disableRotation;


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
        disableRotation = false;
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

        if (disableRotation == false)
        {
            HandleLook();
        }
        HandleCrouch();
        HandleSprint();
        ApplyGravity();

        InteractWithInventory();
        DropItem();     
        HandleCursor();
        HandleGuns();

        if (playerInput.actions["Interact"].WasPressedThisFrame())
        {  
            if (Time.timeScale != 0)
            {
                InteractWithObject();
                PickupItem();
            }
        }
    }

    private void InteractWithObject()
    {
        if (inventory != null)
        {
            inventory = GameObject.FindWithTag("Inventory");
            inventorySystem = inventory.GetComponent<Inventory>();
        }

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


            // Camera X rotation
            transform.Rotate(Vector3.up * mouseX);
            //Camera y rotation
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);



            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
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

    public void InteractWithInventory()
    {
        var ToggleInventoryAction = playerInput.actions["ToggleInventory"];

        if (ToggleInventoryAction.WasPressedThisFrame())
        {
            if (inventorySystem != null && inventorySystem.InventoryDisplay != null)
            {
                inventorySystem.InventoryDisplay.SetActive(!inventorySystem.InventoryDisplay.activeSelf);
                disableRotation = inventorySystem.InventoryDisplay.activeSelf;
                if(inventorySystem.InventoryDisplay.activeSelf == false)
                {
                    for (int i = 0; i < inventorySystem.itemSlots.Length; i++)
                    {
                        if (inventorySystem.itemSlots[i] != null)
                        {
                            inventorySystem.Highlight[i].gameObject.SetActive(false);
                            inventorySystem.SlotSelected[i] = false;
                        }
                    }
                }
            }
        }
    
    }

    public void PickupItem()
    {
        // Create a ray from the center of the camera's view
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        // Variable to store hit information
        RaycastHit hit;

        // Maximum distance for the raycast
        float maxDistance = 3f;

        // Cast the ray
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            // Check if we hit something
            if (hit.collider != null)
            {
                // Get the GameObject that was hit
                GameObject hitObject = hit.collider.gameObject;

           
                if (hitObject.CompareTag("Ammo"))
                {
                    inventorySystem.AddItem("Ammo", 0.5f, 0.8f);
                    Destroy(hitObject);
                }

                if (hitObject.CompareTag("Ammo2"))
                {
                    inventorySystem.AddItem("Ammo2", 0.5f, 0.8f);
                    Destroy(hitObject);
                }

                if (hitObject.CompareTag("Revolver"))
                {
                    inventorySystem.AddItem("Revolver", 3.5f, 2.8f);
                    Destroy(hitObject);
                }

                if (hitObject.CompareTag("AK47"))
                {
                    inventorySystem.AddItem("AK47", 5.2f, 3.8f);
                    Destroy(hitObject);
                }
            }
        }       
    }


    public void DropItem()
    {
        var DropAction = playerInput.actions["DropItem"];

        Transform DropArea = inventorySystem.dropArea.transform;

        if (DropAction.WasPressedThisFrame()) // Ensure it's only triggered once per frame
        {
            for (int i = 0; i < inventorySystem.itemSlots.Length; i++)
            {
                if (inventorySystem.SlotSelected[i] && inventorySystem.InventoryDisplay.activeSelf)
                {
                    inventorySystem.SpawnByTag(inventorySystem.itemSlots[i].tag, DropArea.position);
                    inventorySystem.RemoveItem(inventorySystem.itemSlots[i].tag);
                    break;
                }
                else if (inventorySystem.itemEquipped[i])
                {
                    Debug.Log("Object Dropped");

                    Transform equippedItem = inventorySystem.itemHolderPosition.GetChild(0);
                    
                    equippedItem.transform.SetParent(null);

                    equippedItem.transform.position = DropArea.position;
                                       
                    inventorySystem.RemoveItem(inventorySystem.itemSlots[i].tag);

                    foreach (Transform child in equippedItem)
                    {
                        if (child.CompareTag("pickupPrompt"))
                        {
                            child.gameObject.SetActive(true);
                        }
                    }

                    inventorySystem.itemEquipped[i] = false;
                  
                    break; // Stop after dropping the first selected item
                }
            }
        }
    }

    public void HandleCursor()
    {
        if (inventory != null)
        {
            if (inventorySystem.InventoryDisplay.activeSelf == false)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1f;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0f;
            }
        }
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

    private void HandleGuns()
    {
        var ShootAction = playerInput.actions["Shoot"];


        if (RaycastWeapon.weaponName == "Ak47" && currentWeapon.CanShoot)
        {
            if (ShootAction.IsPressed())
            {
                currentWeapon.Shoot();
            }
        }

        if (RaycastWeapon.weaponName == "Revolver" && currentWeapon.CanShoot)
        {
            if (ShootAction.WasPressedThisFrame())
            {
                currentWeapon.Shoot();
            }
        }
    }


}
