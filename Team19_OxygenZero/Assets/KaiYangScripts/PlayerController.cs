using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float crouchSpeed = 2f;
    public float rotationSpeed = 5f;

    [Header("Look Settings")]
    public float lookSensitivity = 2f;
    public float maxLookAngle = 85f;

    [Header("Crouch Settings")]
    public float normalHeight = 2f;
    public float crouchHeight = 1f;
    private bool isCrouching = false;

    [Header("References")]
    private PlayerInput playerInput;
    private CharacterController characterController;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Transform cameraTransform;

    private float xRotation = 0f;

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
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void OnCrouch(InputValue value)
    {
        if (value.isPressed)
        {
            ToggleCrouch();
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleLook();
    }

    private void HandleMovement()
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        float currentSpeed = isCrouching ? crouchSpeed : moveSpeed;
        Vector3 movement = (forward * moveInput.y + right * moveInput.x) * currentSpeed;
        characterController.SimpleMove(movement);
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

    private void ToggleCrouch()
    {
        if (!isCrouching)
        {
          
            characterController.height = crouchHeight;
            isCrouching = true;
        }
        else
        {
           
            if (!Physics.Raycast(transform.position, Vector3.up, normalHeight))
            {
                characterController.height = normalHeight;
                isCrouching = false;
            }
        }
    }
}
