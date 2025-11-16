using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ControllableObject : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 90f;
    public float gravity = -9.81f;
    public float jumpSpeed = 5f; // Space 跳跃速度

    [Header("Camera Offsets")]
    public Vector3 cameraOffset = new Vector3(0f, 4f, -6f);
    public Vector3 lookAtOffset = new Vector3(0f, 1.8f, 0f);

    [HideInInspector] public bool isControlled;

    private CharacterController cc;
    private Renderer cachedRenderer;
    private Color originalColor;
    private bool isHighlighted = false;
    private float verticalVelocity;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        // 缓存 renderer（包含子物体）
        cachedRenderer = GetComponentInChildren<Renderer>();
        if (cachedRenderer != null && cachedRenderer.material != null)
        {
            if (cachedRenderer.material.HasProperty("_Color"))
                originalColor = cachedRenderer.material.color;
            else
                originalColor = Color.white;
        }
    }

    void Update()
    {
        if (!isControlled) return;

        // --- Movement ---
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");
        bool running = Input.GetKey(KeyCode.LeftShift);

        Vector3 move = (transform.forward * inputZ + transform.right * inputX).normalized;
        move *= running ? runSpeed : moveSpeed;

        // --- Jump ---
        if (cc.isGrounded)
        {
            verticalVelocity = -1f;
            if (Input.GetButtonDown("Jump")) // Space 跳跃
                verticalVelocity = jumpSpeed;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        move.y = verticalVelocity;
        cc.Move(move * Time.deltaTime);

        // --- Mouse Hold Rotation ---
        // 按住右键（Mouse1）或左键（Mouse0）旋转
        if (Input.GetMouseButton(0)) // 右键 Hold 可改为 0 左键
        {
            float mouseX = Input.GetAxis("Mouse X");
            transform.Rotate(Vector3.up, mouseX * rotationSpeed * Time.deltaTime);
        }
    }

    // --- Methods called by ObjectSwitcher ---
    public void ActivateControl()
    {
        isControlled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void DeactivateControl()
    {
        isControlled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void SetHighlight(bool on)
    {
        if (cachedRenderer == null) return;
        if (isHighlighted == on) return;
        isHighlighted = on;
        if (cachedRenderer.material == null) return;

        try
        {
            if (cachedRenderer.material.HasProperty("_Color"))
                cachedRenderer.material.color = on ? Color.yellow : originalColor;
        }
        catch
        {
            // 某些共享材质/只读材质可能报错，忽略以保证稳定
        }
    }
}
