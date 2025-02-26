using Cinemachine;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;

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

    private bool isCursorToggle;

    private float xRotation = 0f;
    public float interactRange = 5f;

    public GameObject inventory;

    public Inventory inventorySystem;

    public RaycastWeapon currentWeapon;

    [Header("Animation Alignment")]
    [SerializeField] private GameObject playerSpine;
    public float spineYRotationOffset = 46f;

    [Header("Camera References")]
    public CinemachineVirtualCamera firstPersonCamera;
    public CinemachineFreeLook thirdPersonCamera;

    [Header("Camera Settings")]
    [SerializeField] private float maxLookAngle;
    public float lookSensitivity = 1f;
    public float shoulderOffset = 1.2f;
    public Transform playerTransform;
    public Transform playerHead;
    public Transform cameraLookAt;
    [SerializeField] private TMP_Text CamText;

    // Camera states
    private enum CameraMode { FirstPerson, ThirdPersonShiftlock }
    private CameraMode currentMode = CameraMode.ThirdPersonShiftlock;

    // Components
    private CinemachinePOV fpsPOV;

    [Header("Player Health")]
    // Player Health
    public float playerHealth;
    public float playerMaxHealth = 100;
    public RectTransform healthBarFill;
    public Image healthBarImage;
    public float defaultHealthDrain = 1f;
    private float originalHealthBarHeight;
    public GameObject playerPrefab;
    public GameObject playerModel;


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

        currentWeapon = null;

        if (firstPersonCamera != null)
        {
            fpsPOV = firstPersonCamera.GetCinemachineComponent<CinemachinePOV>();
            fpsPOV.m_HorizontalAxis.m_MaxSpeed = lookSensitivity * 700;
            fpsPOV.m_VerticalAxis.m_MaxSpeed = lookSensitivity * 700;
        }

        // Configure third person camera
        if (thirdPersonCamera != null)
        {
            // Set up the free look camera for shiftlock behavior
            thirdPersonCamera.m_XAxis.m_MaxSpeed = lookSensitivity * 2000;
            thirdPersonCamera.m_YAxis.m_MaxSpeed = 5; // Lock vertical orbit in shiftlock mode

            // Set shoulder position
            thirdPersonCamera.GetRig(1).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset =
                new Vector3(shoulderOffset, 1.5f, 0);

            thirdPersonCamera.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
        }

        // Set initial camera mode
        SetCameraMode(currentMode);

        if (currentMode == CameraMode.ThirdPersonShiftlock)
        {
            CamText.text = "Third Person";
        }
        else
        {
            CamText.text = "First Person";
        }

        // Set player health
        playerHealth = playerMaxHealth;

        originalHealthBarHeight = healthBarFill.sizeDelta.y;
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
        HandleCursor();
        ToggleCursor();

        if (!isCursorToggle)
        {
            CheckGround();
            HandleMovement();
            HandleCrouch();
            HandleSprint();
            InteractWithInventory();
            DropItem();
            UseItem();
            HandleGuns();
        }

        ApplyGravity();
        UpdateHealthUI();

        if (playerInput.actions["Interact"].WasPressedThisFrame())
        {
            if (Time.timeScale != 0)
            {
                InteractWithObject();
                PickupItem();
            }
        }

        ToggleCameraMode();
        // Handle player head rotation
        UpdateHeadRotation();
    }

    private void LateUpdate()
    {
        if (!isCursorToggle)
        {
            SpineRotation();
        }
    }

    private void ToggleCursor()
    {
        // Check if inventory is open; if it is, do nothing
        if (inventorySystem != null)
        {
            if (inventorySystem.InventoryDisplay.activeSelf)
                return;

            // If inventory is NOT open, allow Alt key to unlock the cursor
            if (playerInput.actions["ToggleCursor"].IsPressed()) // Alt key is held downs
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                isCursorToggle = true;
            }
            else // Alt key is released
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                isCursorToggle = false;
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

        // Rotate the player body (yaw rotation)
        transform.Rotate(Vector3.up * mouseX);

        // Adjust X rotation for the camera (pitch rotation)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        // Apply the camera's pitch rotation
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Get target rotation from the camera
        Quaternion targetRotation = Quaternion.LookRotation(cameraTransform.forward);

        // Convert to Euler angles for manual adjustments
        Vector3 eulerRotation = targetRotation.eulerAngles;

        // Evenly distribute rotation between X and Z
        float combinedRotation = xRotation;  // Keep the same value for both axes

        eulerRotation.x = combinedRotation;  // Rotate on X (looking up/down)
        eulerRotation.z = combinedRotation;  // Rotate on Z to keep balance

        // Apply Y offset if needed
        eulerRotation.y += spineYRotationOffset;

        // Apply the final rotation to the spine
        playerSpine.transform.rotation = Quaternion.Euler(eulerRotation);
    }


    private void HandleCrouch()
    {
        var crouchAction = playerInput.actions["Crouch"];

        if (crouchAction.IsPressed())
        {
            isCrouching = true;
            targetHeight = crouchHeight;
        }
        else
        {
            isCrouching = false;
            targetHeight = normalHeight;
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
                inventorySystem.UpdateMoneyUI();
                inventorySystem.InventoryDisplay.SetActive(!inventorySystem.InventoryDisplay.activeSelf);
                if (inventorySystem.InventoryDisplay.activeSelf == false)
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
            Debug.Log("Raycast hit: " + hit.collider.gameObject.name); // Debug log for hit object
            // Check if we hit something
            if (hit.collider != null)
            {
                // Get the GameObject that was hit
                GameObject hitObject = hit.collider.gameObject;
                Debug.Log("Hit object tag: " + hitObject.tag); // Check detected tag

                if (hitObject.CompareTag("Item"))
                {
                    ObjectData Item = hitObject.GetComponent<ObjectData>();
                    inventorySystem.AddItem(Item.item.itemName, "Item", Item.item.cost, Item.item.weight, Item.item.usable);
                    Destroy(hitObject);
                }



                if (hitObject.CompareTag("Weapon"))
                {
                    RaycastWeapon weaponItem = hitObject.GetComponent<RaycastWeapon>();
                    inventorySystem.AddItem(weaponItem.weaponData.weaponName, "Weapon", weaponItem.weaponData.cost, weaponItem.weaponData.weight, weaponItem.weaponData.Usable);
                    Destroy(hitObject);
                }

            }
        }
        else
        {
            Debug.Log("Raycast did not hit anything");
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
                    if (!inventorySystem.itemEquipped[i])
                    {
                        ItemManager.Instance.SpawnByItemName(inventorySystem.itemSlots[i].name, DropArea.position);
                    }
                    if (inventorySystem.itemEquipped[i])
                    {
                        Debug.Log("Object Dropped");

                        Transform equippedItem = inventorySystem.itemHolderPosition.GetChild(0);

                        equippedItem.transform.SetParent(null);

                        equippedItem.transform.position = DropArea.position;

                        if (equippedItem != null)
                        {
                            equippedItem.AddComponent<Rigidbody>();
                        }

                        foreach (Transform child in equippedItem)
                        {
                            if (child.CompareTag("pickupPrompt"))
                            {
                                child.gameObject.SetActive(true);
                            }
                        }

                        currentWeapon = null;
                        inventorySystem.itemEquipped[i] = false;
                    }
                    inventorySystem.RemoveItem(inventorySystem.itemSlots[i].name);
                    
                    break;
                }
                else if (inventorySystem.itemEquipped[i] && !inventorySystem.SlotSelected[i])
                {
                    Debug.Log("Object Dropped");

                    Transform equippedItem = inventorySystem.itemHolderPosition.GetChild(0);

                    equippedItem.transform.SetParent(null);

                    equippedItem.transform.position = DropArea.position;

                    inventorySystem.RemoveItem(inventorySystem.itemSlots[i].name);


                    if (equippedItem != null)
                    {
                        equippedItem.AddComponent<Rigidbody>();
                    }

                    foreach (Transform child in equippedItem)
                    {
                        if (child.CompareTag("pickupPrompt"))
                        {
                            child.gameObject.SetActive(true);
                        }
                    }

                    currentWeapon = null;



                    inventorySystem.itemEquipped[i] = false;



                    break; // Stop after dropping the first selected item
                }
            }
        }
    }


    public void UseItem()
    {
        var UseAction = playerInput.actions["UseItem"];

        if (UseAction.WasPressedThisFrame())
        {
            for (int i = 0; i < inventorySystem.itemSlots.Length; i++)
            {
                if (inventorySystem.SlotSelected[i] && inventorySystem.InventoryDisplay.activeSelf)
                {
                    if (inventorySystem.usableItem[i] == true)
                    {
                        inventorySystem.RemoveItem(inventorySystem.itemSlots[i].name);
                    }
                    break;
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

        if (currentWeapon != null)
        {

            if (currentWeapon.weaponState == "Ak47" && currentWeapon.CanShoot)
            {
                if (ShootAction.IsPressed())
                {
                    currentWeapon.Shoot();
                }
            }

            if (currentWeapon.weaponState == "Revolver" && currentWeapon.CanShoot)
            {
                if (ShootAction.WasPressedThisFrame())
                {
                    currentWeapon.Shoot();
                }
            }
        }
    }

    void ToggleCameraMode()
    {
        var ToggleCameraAction = playerInput.actions["ToggleCamera"];


        if (ToggleCameraAction.WasPressedThisFrame())
        {

            if (currentMode == CameraMode.FirstPerson)
            {
                SetCameraMode(CameraMode.ThirdPersonShiftlock);
                CamText.text = "Third Person";
            }
            else
            {
                SetCameraMode(CameraMode.FirstPerson);
                CamText.text = "First Person";
            }
        }
    }

    void UpdateHeadRotation()
    {
        if (playerHead == null) return;

        GameObject camera = GameObject.FindWithTag("MainCamera");

        playerHead.transform.rotation = camera.transform.rotation;
        cameraLookAt.transform.rotation = camera.transform.rotation;

        // Fix: Extract Y rotation properly
        float cameraYRotation = camera.transform.eulerAngles.y;

        if (currentMode == CameraMode.FirstPerson)
        {
            playerModel.transform.rotation = Quaternion.Euler(0, cameraYRotation, 0);
        }
        else
        {
            playerPrefab.transform.rotation = Quaternion.Euler(0, cameraYRotation, 0);
        }
    }

    private void SpineRotation()
    {
        GameObject camera = GameObject.FindWithTag("MainCamera");
        float cameraRotation = camera.transform.eulerAngles.x;

        playerSpine.transform.localRotation = Quaternion.Euler(cameraRotation, 0, cameraRotation);
    }

    private void UpdateHealthUI()
    {
        float normalizedHealth = playerHealth / playerMaxHealth;
        healthBarFill.sizeDelta = new Vector2(healthBarFill.sizeDelta.x, originalHealthBarHeight * normalizedHealth);
    }

    public void DepletePlayerHealth(float health)
    {
        playerHealth -= health * Time.deltaTime;
    }


    public float GetPlayerHealth()
    {
        return playerHealth;
    }

    void SetCameraMode(CameraMode mode)
    {
        currentMode = mode;

        switch (mode)
        {
            case CameraMode.FirstPerson:
                firstPersonCamera.Priority = 20;
                thirdPersonCamera.Priority = 10;
                break;

            case CameraMode.ThirdPersonShiftlock:
                firstPersonCamera.Priority = 10;
                thirdPersonCamera.Priority = 20;
                break;
        }
    }
}