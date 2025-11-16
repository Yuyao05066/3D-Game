using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class FirstPersonController : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;

    [Header("Movement")]
    public float walkSpeed = 4.0f;
    public float runSpeed = 6.5f;
    public float gravity = -9.81f;
    public float jumpSpeed = 3.5f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2.0f;
    public float verticalLookLimit = 85f;
    public KeyCode lookHoldKey = KeyCode.Mouse1; // 右键按住才进入视角模式

    private CharacterController cc;
    private float verticalVelocity = 0f;
    private float cameraPitch = 0f;
    private bool isLooking = false;
    
    private static FirstPersonController instance;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        if (playerCamera == null)
        {
            Debug.LogError("PlayerCamera not assigned on FirstPersonController.");
        }

        // 默认鼠标可见
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        HandleLookMode();
        HandleMouseLook();
        HandleMovement();
    }

    void HandleLookMode()
    {
        // 按住右键时启用视角模式
        if (Input.GetKeyDown(lookHoldKey))
        {
            isLooking = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (Input.GetKeyUp(lookHoldKey))
        {
            isLooking = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void HandleMouseLook()
    {
        if (!isLooking) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -verticalLookLimit, verticalLookLimit);
        playerCamera.transform.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
    }

    void HandleMovement()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        Vector3 move = transform.right * inputX + transform.forward * inputZ;
        move = move.normalized * speed;

        if (cc.isGrounded)
        {
            if (Input.GetButtonDown("Jump")) verticalVelocity = jumpSpeed;
            else verticalVelocity = -1f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        move.y = verticalVelocity;
        cc.Move(move * Time.deltaTime);
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
