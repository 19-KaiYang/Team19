using Cinemachine;
using System.Collections;
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

    [Header("References")]
    private PlayerInput playerInput;
    private CharacterController characterController;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Transform cameraTransform;

    private bool isCursorToggle;
    public float interactRange = 5f;

    public GameObject inventory;

    public Inventory inventorySystem;

    public RaycastWeapon currentWeapon;

    [Header("Animation Alignment")]
    [SerializeField] private GameObject playerSpine;
    public float spineYRotationOffset = 46f;

    [Header("Animator")]
    [SerializeField] private Animator animator;

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
    private CameraMode currentMode = CameraMode.FirstPerson;

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

    public bool usingPistol;
    public bool usingRifle;
    public bool usingNothing;

    public AvatarMask upperBodyMask; // Assign in Inspector

    private float zoominFOV = 50;
    private float zoomoutFOV = 60;
    private float FOVtransitionSpeed = 0.1f;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        characterController = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;

        if (gameObject.tag != "Player")
        {
            gameObject.tag = "Player";
        }
        StartCoroutine(FindAndPositionOnShuttle());
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isCrouching = false;
        targetHeight = normalHeight;
        characterController.height = normalHeight;
        if (currentMode == CameraMode.FirstPerson)
        {
            Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("Playerlayer"));
        }
        else
        {
            Camera.main.cullingMask |= (1 << LayerMask.NameToLayer("Playerlayer"));
        }

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
        StartCoroutine(FindAndPositionOnShuttle());
    }

    IEnumerator FindAndPositionOnShuttle()
    {
        // Wait for grid generation
        yield return new WaitForSeconds(0.5f);

        // Find shuttle platform
        GameObject shuttlePlatform = GameObject.FindGameObjectWithTag("ShuttlePlatform");

        if (shuttlePlatform != null)
        {
            transform.position = shuttlePlatform.transform.position + Vector3.up * 1f;
            transform.rotation = shuttlePlatform.transform.rotation;
        }
        else
        {
            Debug.LogError("Shuttle platform not found! Cannot position player.");
        }
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
            animator.SetBool("IsJumping", true);
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
           
            HandleGuns();
        }
       

        ApplyGravity();
        UpdateHealthUI();

        SetIdleStateType();
        SetIdleAnimation();

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
        Reload();
    }

    private void LateUpdate()
    {
        if (!isCursorToggle)
        {
            SpineRotation();
        }
    }

    private void SetIdleStateType()
    {
        // Assume the player is holding nothing by default
        usingPistol = false;
        usingRifle = false;
        usingNothing = true;

        // Loop through inventory slots to check if an item is equipped
        for (int i = 0; i < inventorySystem.itemSlots.Length; i++)
        {
            if (inventorySystem.itemEquipped[i]) // If an item is equipped
            {
                // Ensure there's a weapon in the holder
                if (inventorySystem.itemHolderPosition.childCount > 0)
                {
                    Transform Weapon = inventorySystem.itemHolderPosition.GetChild(0);
                    RaycastWeapon type = Weapon.GetComponent<RaycastWeapon>();

                    if (type != null)
                    {
                        if (type.weaponData.weaponType == WeaponType.Pistol)
                        {
                            usingPistol = true;
                            usingRifle = false;
                            usingNothing = false;
                        }
                        else if (type.weaponData.weaponType == WeaponType.Rifle)
                        {
                            usingRifle = true;
                            usingPistol = false;
                            usingNothing = false;
                        }
                        else // If the weapon exists but has an unknown type
                        {
                            usingRifle = false;
                            usingPistol = false;
                            usingNothing = true;
                        }
                    }
                }
                else // No weapon in the holder
                {
                    usingRifle = false;
                    usingPistol = false;
                    usingNothing = true;
                }

                // Exit loop early since we found an equipped item
                break;
            }
        }
    }


    private void SetIdleAnimation()
    {
        if (usingRifle)
        {
            animator.SetLayerWeight(2, 0); // Disable Rifle Layer
            animator.SetLayerWeight(1, 1); // Enable Pistol Layer
        }
        else if (usingPistol)
        {
            animator.SetLayerWeight(2, 0); // Disable Rifle Layer
            animator.SetLayerWeight(1, 1); // Enable Pistol Layer
        }
        else if (usingNothing)
        {
            animator.SetLayerWeight(2, 0); // Disable Rifle Layer
            animator.SetLayerWeight(1, 0); // Disable Pistol Layer
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
            if (playerInput.actions["ToggleCursor"].IsPressed()) // Alt key is held down
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0;

                // Disable First-Person camera rotation (both X and Y)
                if (firstPersonCamera != null)
                {
                    CinemachinePOV pov = firstPersonCamera.GetCinemachineComponent<CinemachinePOV>();
                    pov.m_HorizontalAxis.m_MaxSpeed = 0; // Disable left-right rotation
                    pov.m_VerticalAxis.m_MaxSpeed = 0;   // Disable up-down rotation
                }

                // Disable Third-Person camera rotation (both X and Y)
                if (thirdPersonCamera != null)
                {
                    thirdPersonCamera.m_XAxis.m_MaxSpeed = 0; // Disable left-right rotation
                    thirdPersonCamera.m_YAxis.m_MaxSpeed = 0; // Disable up-down rotation
                }
            }
            else // Alt key is released
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1;

                // Re-enable First-Person camera rotation
                if (firstPersonCamera != null)
                {
                    CinemachinePOV pov = firstPersonCamera.GetCinemachineComponent<CinemachinePOV>();
                    pov.m_HorizontalAxis.m_MaxSpeed = lookSensitivity * 700;
                    pov.m_VerticalAxis.m_MaxSpeed = lookSensitivity * 700;
                }

                // Re-enable Third-Person camera rotation
                if (thirdPersonCamera != null)
                {
                    thirdPersonCamera.m_XAxis.m_MaxSpeed = lookSensitivity * 2000;
                    thirdPersonCamera.m_YAxis.m_MaxSpeed = 5; // Adjust to your preferred speed
                }
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

    private float lastMoveY = 0f;

    private void HandleMovement()
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        if (!isSprinting)
        {
            if (currentMode == CameraMode.FirstPerson && firstPersonCamera.m_Lens.FieldOfView < zoomoutFOV)
            {
                firstPersonCamera.m_Lens.FieldOfView += FOVtransitionSpeed;
            }
            if (currentMode == CameraMode.ThirdPersonShiftlock && thirdPersonCamera.m_Lens.FieldOfView < zoomoutFOV)
            {
                thirdPersonCamera.m_Lens.FieldOfView += FOVtransitionSpeed;
            }
        }

        float currentSpeed = walkSpeed;
        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else if (isSprinting && !isCrouching)
        {
            currentSpeed = sprintSpeed;
            if (currentMode == CameraMode.FirstPerson && firstPersonCamera.m_Lens.FieldOfView > zoominFOV)
            {
                firstPersonCamera.m_Lens.FieldOfView -= FOVtransitionSpeed;
            }
            if (currentMode == CameraMode.ThirdPersonShiftlock && thirdPersonCamera.m_Lens.FieldOfView > zoominFOV)
            {
                thirdPersonCamera.m_Lens.FieldOfView -= FOVtransitionSpeed;
            }
        }

        // Calculate movement direction
        Vector3 movement = (forward * moveInput.y + right * moveInput.x) * currentSpeed;

        // Check if player is moving
        bool isMoving = movement.magnitude > 0.01f;

        // Detect if moving backward (Only while "S" is actively pressed)
        if (moveInput.y < 0)
        {
            lastMoveY = moveInput.y; // Store last backward input
        }
        else if (moveInput.y > 0)
        {
            lastMoveY = moveInput.y; // Store last forward input
        }

        bool isMovingBackward = lastMoveY < 0 && moveInput.y < 0; // Ensure backward movement resets correctly

        if (isMoving)
        {
            if (usingPistol)
            {
                if (isMovingBackward)
                {
                    // Add pistol movement behavior when moving backward
                    animator.SetBool("IsWalking", true);
                    animator.SetBool("IsBackwardWalking", true);
                }
                else
                {
                    // Add pistol movement behavior when moving forward
                    animator.SetBool("IsWalking", true);
                    animator.SetBool("IsBackwardWalking", false);
                }
            }
            else if (usingRifle)
            {
                if (isMovingBackward)
                {
                    // Add rifle movement behavior when moving backward
                    animator.SetBool("IsWalking", true);
                    animator.SetBool("IsBackwardWalking", true);
                }
                else
                {
                    // Add rifle movement behavior when moving forward
                    animator.SetBool("IsWalking", true);
                    animator.SetBool("IsBackwardWalking", false);
                }
            }
            else if (usingNothing)
            {
                if (isMovingBackward)
                {
                    animator.SetBool("IsWalking", true);
                    animator.SetBool("IsBackwardWalking", true);
                }
                else
                {
                    animator.SetBool("IsWalking", true);
                    animator.SetBool("IsBackwardWalking", false);
                }
            }
        }
        else // Player is NOT moving
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsBackwardWalking", false);
        }

        // Move the character
        characterController.Move(movement * Time.deltaTime);
    }



    private void HandleCrouch()
    {
        float recenterController;
        float cameraOffsetY;

        var crouchAction = playerInput.actions["Crouch"];

        if (crouchAction.IsPressed())
        {

            upperBodyMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftLeg, false);
            upperBodyMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightLeg, false);
            upperBodyMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Body, false);

            recenterController = -0.2f;
            cameraOffsetY = -2f;

            isCrouching = true;
            targetHeight = crouchHeight;
            animator.SetBool("IsCrouch", true);
        }
        else
        {

            upperBodyMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftLeg, true);
            upperBodyMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightLeg, true);
            upperBodyMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Body, true);

            recenterController = 0;
            cameraOffsetY = 0.698f;

            isCrouching = false;
            targetHeight = normalHeight;
            animator.SetBool("IsCrouch", false);
        }

        characterController.height = targetHeight;
        characterController.center = new Vector3(0, recenterController, 0);

        // Move the camera Y position
        Camera.main.transform.localPosition = new Vector3(
            Camera.main.transform.localPosition.x,
            cameraOffsetY,
            Camera.main.transform.localPosition.z
        );
    }

    private void HandleSprint()
    {
        var sprintAction = playerInput.actions["Sprint"];

        if (sprintAction.IsPressed())
        {
            if (!isSprinting)
            {
                animator.SetBool("IsRunning", true);
                isSprinting = true;
            }
        }
        else
        {
            if (isSprinting)
            {
                animator.SetBool("IsRunning", false);
                isSprinting = false;
            }
        }
    }

    private void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            animator.SetBool("IsJumping", false);
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
        Ray ray;

        if (currentMode == CameraMode.ThirdPersonShiftlock)
        {
            ray = new Ray(cameraLookAt.transform.position, cameraLookAt.transform.forward); // Create ray from object
        }
        else
        {
            ray = new Ray(playerHead.transform.position, playerHead.transform.forward);
        }

        // Variable to store hit information
        RaycastHit hit;
        float maxDistance ;

        // Variable to store hit information
        maxDistance = (currentMode == CameraMode.FirstPerson) ? 3f : 10f;

        // ✅ Draw the ray in the Scene view (Red if it hits, Green if it misses)
        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.green, 1f);

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

                    // Play pick up sound
                    AudioManager audioManager = FindObjectOfType<AudioManager>();
                    if (audioManager != null)
                    {

                        audioManager.PlaySFX("PickUp");

                    }
                    else
                    {
                        Debug.LogWarning("Pickup");
                    }

                    Destroy(hitObject);
                }

                if (hitObject.CompareTag("Money"))
                {
                    inventorySystem.UpdateMoney(20, true);

                    // Play money sound
                    AudioManager audioManager = FindObjectOfType<AudioManager>();
                    if (audioManager != null)
                    {

                        audioManager.PlaySFX("Money");

                    }
                    else
                    {
                        Debug.LogWarning("Money");
                    }

                    Destroy(hitObject);
                }

                if (hitObject.CompareTag("Weapon"))
                {
                    RaycastWeapon weaponItem = hitObject.GetComponent<RaycastWeapon>();
                    inventorySystem.AddItem(weaponItem.weaponData.weaponName, "Weapon", weaponItem.weaponData.cost, weaponItem.weaponData.weight, weaponItem.weaponData.Usable);

                    // Play pick up sound
                    AudioManager audioManager = FindObjectOfType<AudioManager>();
                    if (audioManager != null)
                    {

                        audioManager.PlaySFX("PickUp");

                    }
                    else
                    {
                        Debug.LogWarning("Pickup");
                    }

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
                // if in inventory and a slot selected
                if (inventorySystem.SlotSelected[i] && inventorySystem.InventoryDisplay.activeSelf)
                {
                    // if item not equipped
                    if (!inventorySystem.itemEquipped[i])
                    {
                        ItemManager.Instance.SpawnByItemName(inventorySystem.itemSlots[i].name, DropArea.position);
                    }
                    /// if item equipped
                    if (inventorySystem.itemEquipped[i])
                    {                     

                        Transform equippedItem = inventorySystem.itemHolderPosition.GetChild(0);

                        equippedItem.transform.SetParent(null);

                        equippedItem.transform.position = DropArea.position;

                        inventorySystem.AmmoPanel.SetActive(false);

                        if (equippedItem != null)
                        {
                            equippedItem.AddComponent<Rigidbody>();
                            SetLayerRecursively(equippedItem.gameObject, "Ground");
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
                    // Play drop sound
                    AudioManager audioManager = FindObjectOfType<AudioManager>();
                    if (audioManager != null)
                    {

                        audioManager.PlaySFX("Drop");

                    }
                    else
                    {
                        Debug.LogWarning("Drop");
                    }

                    // put layer
                    inventorySystem.RemoveItem(inventorySystem.itemSlots[i].name);
                    
                    break;
                }
                // if item is equipped and not opened inventory
                else if (inventorySystem.itemEquipped[i] && !inventorySystem.InventoryDisplay.activeSelf)
                {

                    Debug.Log("Object Dropped");

                    // Play drop sound
                    AudioManager audioManager = FindObjectOfType<AudioManager>();
                    if (audioManager != null)
                    {

                        audioManager.PlaySFX("Drop");

                    }
                    else
                    {
                        Debug.LogWarning("Drop");
                    }


                    Transform equippedItem = inventorySystem.itemHolderPosition.GetChild(0);

                    equippedItem.transform.SetParent(null);

                    equippedItem.transform.position = DropArea.position;

                    inventorySystem.AmmoPanel.SetActive(false);

                    // set layer to ground

                    SetLayerRecursively(equippedItem.gameObject, "Ground");

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

    void SetLayerRecursively(GameObject obj, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        obj.layer = layer; // Set parent layer

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layerName); // Recursively set child layers
        }
    }

    public void HandleCursor()
    {
        if (inventory != null)
        {
            if (inventorySystem.InventoryDisplay.activeSelf == false) // Inventory is closed
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1f;

                // Re-enable First-Person camera rotation
                if (firstPersonCamera != null)
                {
                    CinemachinePOV pov = firstPersonCamera.GetCinemachineComponent<CinemachinePOV>();
                    pov.m_HorizontalAxis.m_MaxSpeed = lookSensitivity * 700; // Restore left-right rotation
                    pov.m_VerticalAxis.m_MaxSpeed = lookSensitivity * 700;   // Restore up-down rotation
                }

                // Re-enable Third-Person camera rotation
                if (thirdPersonCamera != null)
                {
                    thirdPersonCamera.m_XAxis.m_MaxSpeed = lookSensitivity * 2000; // Restore left-right rotation
                    thirdPersonCamera.m_YAxis.m_MaxSpeed = 5; // Restore up-down rotation (adjust as needed)
                }
            }
            else // Inventory is open
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0f;

                // Disable First-Person camera rotation
                if (firstPersonCamera != null)
                {
                    CinemachinePOV pov = firstPersonCamera.GetCinemachineComponent<CinemachinePOV>();
                    pov.m_HorizontalAxis.m_MaxSpeed = 0; // Disable left-right rotation
                    pov.m_VerticalAxis.m_MaxSpeed = 0;   // Disable up-down rotation
                }

                // Disable Third-Person camera rotation
                if (thirdPersonCamera != null)
                {
                    thirdPersonCamera.m_XAxis.m_MaxSpeed = 0; // Disable left-right rotation
                    thirdPersonCamera.m_YAxis.m_MaxSpeed = 0; // Disable up-down rotation
                }
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
                Camera.main.cullingMask |= (1 << LayerMask.NameToLayer("Playerlayer"));
            }
            else
            {
                SetCameraMode(CameraMode.FirstPerson);
                CamText.text = "First Person";
                Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("Playerlayer"));
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
            // Sync first-person camera to match player position before switching
            playerModel.transform.rotation = Quaternion.Euler(0, cameraYRotation, 0);
        }
    }

    private void SpineRotation()
    {
        GameObject camera = GameObject.FindWithTag("MainCamera");
        float cameraRotation = camera.transform.eulerAngles.x;

        if (usingNothing)
        {
            playerSpine.transform.localRotation = Quaternion.Euler(cameraRotation, 0, 0);
        }
        else
        {
            playerSpine.transform.localRotation = Quaternion.Euler(cameraRotation, 0, cameraRotation);
        }
    }

    private void UpdateHealthUI()
    {
        float normalizedHealth = playerHealth / playerMaxHealth;
        healthBarFill.sizeDelta = new Vector2(healthBarFill.sizeDelta.x, originalHealthBarHeight * normalizedHealth);
    }

    public void DepletePlayerHealth(float health)
    {
        playerHealth -= health;

        // Play hurt sound
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {

            audioManager.PlaySFX("Hurt");

        }
        else
        {
            Debug.LogWarning("Hurt");
        }

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
                CinemachineBrain.SoloCamera = firstPersonCamera;
                break;

            case CameraMode.ThirdPersonShiftlock:
               

              
                
                firstPersonCamera.Priority = 10;
                thirdPersonCamera.Priority = 20;
                CinemachineBrain.SoloCamera = thirdPersonCamera;
                break;
        }
    }

    public void Reload()
    {
        var ReloadAction = playerInput.actions["Reload"];

        if (ReloadAction.WasPressedThisFrame())
        {
            for (int i = 0; i < inventorySystem.itemSlots.Length; i++)
            {
                // Check if there is a weapon equipped on player
                if (inventorySystem.itemEquipped[i] == true)
                {
                    Transform Weapon = inventorySystem.itemHolderPosition.GetChild(0);
                    RaycastWeapon WeaponInfo = Weapon.GetComponent<RaycastWeapon>();

                    if (WeaponInfo.weaponData.weaponName == "Revolver")
                    {
                        Debug.Log("Reload");
                        if (WeaponInfo.maxAmmoCount >= WeaponInfo.magazineSize)
                        {
                            // The amount of ammo you need to give
                            int GivenAmmo = WeaponInfo.magazineSize - WeaponInfo.ammoCount;
                            // Remove maxAmmo
                            WeaponInfo.maxAmmoCount -= GivenAmmo;
                            // Add Ammo
                            WeaponInfo.ammoCount += GivenAmmo;
                            Debug.Log("Reload sufficient Ammo");
                        }
                        else if (WeaponInfo.maxAmmoCount < WeaponInfo.magazineSize)
                        {
                            // The amount of ammo you need to give
                            int GivenAmmo = WeaponInfo.magazineSize - WeaponInfo.ammoCount;

                            int MaxAmmoleft = WeaponInfo.maxAmmoCount;

                            if (GivenAmmo > MaxAmmoleft)
                            {
                                // Add Ammo
                                WeaponInfo.ammoCount += WeaponInfo.maxAmmoCount;
                                WeaponInfo.maxAmmoCount = 0;
                                Debug.Log("Reload insufficient Ammo");
                            }
                            else if (GivenAmmo < MaxAmmoleft)
                            {
                                WeaponInfo.ammoCount += GivenAmmo;

                                WeaponInfo.maxAmmoCount -= GivenAmmo;
                                Debug.Log("Reload sufficient Ammo");
                            }
                        }
                    }

                    if (WeaponInfo.weaponData.weaponName == "Ak47")
                    {
                        if (WeaponInfo.maxAmmoCount >= WeaponInfo.magazineSize)
                        {
                            // The amount of ammo you need to give
                            int GivenAmmo = WeaponInfo.magazineSize - WeaponInfo.ammoCount;
                            // Remove maxAmmo
                            WeaponInfo.maxAmmoCount -= GivenAmmo;
                            // Add Ammo
                            WeaponInfo.ammoCount += GivenAmmo;
                        }
                        else if (WeaponInfo.maxAmmoCount < WeaponInfo.magazineSize)
                        {
                            // The amount of ammo you need to give
                            int GivenAmmo = WeaponInfo.magazineSize - WeaponInfo.ammoCount;

                            int MaxAmmoleft = WeaponInfo.maxAmmoCount;

                            if (GivenAmmo > MaxAmmoleft)
                            {
                                // Add Ammo
                                WeaponInfo.ammoCount += WeaponInfo.maxAmmoCount;
                                WeaponInfo.maxAmmoCount = 0;
                            }
                            else if (GivenAmmo < MaxAmmoleft)
                            {
                                WeaponInfo.ammoCount += GivenAmmo;

                                WeaponInfo.maxAmmoCount -= GivenAmmo;
                            }
                        }
                    }

                }
            }


        }
    }
}