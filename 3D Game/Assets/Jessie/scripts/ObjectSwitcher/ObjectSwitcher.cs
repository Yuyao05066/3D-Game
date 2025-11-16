using UnityEngine;

public class ObjectSwitcher : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;          // 主摄像机
    public GameObject playerRoot;        // 玩家物体（Capsule 或 FPS）
    public MonoBehaviour fpController;   // 玩家第一视角控制脚本

    [Header("Switch Settings")]
    public string switchableTag = "Switchable";   // 可切换物体 Tag
    public float maxSwitchDistance = 4f;         // 最大切换距离
    public float maxAngle = 45f;                 // 摄像机正前方夹角阈值
    public KeyCode switchKey = KeyCode.F;

    [Header("Camera Follow")]
    public float cameraMoveSpeed = 6f;
    public float cameraRotateSpeed = 6f;

    private ControllableObject currentTarget;
    private GameObject currentController; 
    private ControllableObject lastHighlighted = null;
    private bool isPlayerMode = true;
    
    private float switchCooldown = 0.3f;
    private float lastSwitchTime = -999f;

    void Start()
    {
        if (!playerCamera) playerCamera = Camera.main;
        if (!playerRoot) playerRoot = gameObject;
        currentController = playerRoot;
    }

    void Update()
    {
        ControllableObject candidate = FindBestTarget();
        UpdateHighlight(candidate);
        if (Time.time - lastSwitchTime < switchCooldown)
            return; // 还在冷却中
        // 按 F 切换
        if (Input.GetKeyDown(switchKey))
        {
            if (isPlayerMode)
            {
                ControllableObject target = FindBestTarget();
                if (target != null)
                    SwitchToObject(target);
            }
            else
            {
                ControllableObject next = FindBestTarget();
                if (next != null && next != currentTarget)
                    SwitchToObject(next);
                else
                    ReturnToPlayer();
            }
        }

        // 平滑摄像机跟随
        if (!isPlayerMode && currentTarget != null)
            SmoothFollowTarget();
    }

    // 找到摄像机前方，角度与距离都满足的最优物体
    ControllableObject FindBestTarget()
    {
        ControllableObject best = null;
        float bestDist = float.MaxValue;

        // 📌 当前控制者是谁？（player 或 cube）
        Transform origin = currentController.transform;
        Vector3 viewDir = playerCamera.transform.forward;
        Vector3 viewPos = playerCamera.transform.position;

        foreach (var obj in GameObject.FindGameObjectsWithTag(switchableTag))
        {
            // 不找自己
            if (obj == currentController) continue;

            Vector3 dirToObj = obj.transform.position - viewPos;
            float angle = Vector3.Angle(viewDir, dirToObj);
            float dist = Vector3.Distance(origin.position, obj.transform.position);

            if (angle <= maxAngle && dist <= maxSwitchDistance)
            {
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = obj.GetComponent<ControllableObject>();
                }
            }
        }

        return best;
    }

    // void SwitchToObject(ControllableObject target)
    // {
    //     if (target == null) return;
    //
    //     // 停止玩家控制
    //     if (fpController != null) fpController.enabled = false;
    //     isPlayerMode = false;
    //
    //     // 切换控制对象
    //     if (currentTarget != null)
    //         currentTarget.DeactivateControl();
    //
    //     currentTarget = target;
    //     currentTarget.ActivateControl();
    // }
    
    void SwitchToObject(ControllableObject target)
    {
        if (target == null) return;

        if (fpController != null) fpController.enabled = false;
        isPlayerMode = false;

        if (currentTarget != null)
            currentTarget.DeactivateControl();

        currentTarget = target;
        currentTarget.ActivateControl();

        // ⚡ 新增：当前控制者变为这个 cube
        currentController = currentTarget.gameObject;
        // 取消之前高亮（避免切换时残留）
        if (lastHighlighted != null)
        {
            lastHighlighted.SetHighlight(false);
            lastHighlighted = null;
        }
    }

    void ReturnToPlayer()
    {
        if (currentTarget != null)
            currentTarget.DeactivateControl();

        currentTarget = null;
        isPlayerMode = true;

        // 启用玩家第一视角控制
        if (fpController != null)
            fpController.enabled = true;

        // ⚡关键：同步摄像机位置和旋转
        playerCamera.transform.SetParent(playerRoot.transform);
        playerCamera.transform.localPosition = new Vector3(0, 3.0f, -2f); // Player 头顶
        playerCamera.transform.localRotation = Quaternion.identity;
        
        // ⚡ 新增：控制者回归 player
        currentController = playerRoot;
        if (lastHighlighted != null)
        {
            lastHighlighted.SetHighlight(false);
            lastHighlighted = null;
        }
    }

    void SmoothFollowTarget()
    {
        Vector3 camPos = currentTarget.transform.TransformPoint(currentTarget.cameraOffset);
        Vector3 lookPos = currentTarget.transform.TransformPoint(currentTarget.lookAtOffset);

        playerCamera.transform.position = Vector3.Lerp(
            playerCamera.transform.position, camPos, Time.deltaTime * cameraMoveSpeed);

        Quaternion rot = Quaternion.LookRotation(lookPos - camPos);
        playerCamera.transform.rotation = Quaternion.Slerp(
            playerCamera.transform.rotation, rot, Time.deltaTime * cameraRotateSpeed);
    }
    
    void UpdateHighlight(ControllableObject newHighlight)
    {
        // 取消之前的高亮（如果变了）
        if (lastHighlighted != null && lastHighlighted != newHighlight)
        {
            lastHighlighted.SetHighlight(false);
            lastHighlighted = null;
        }

        // 新目标开启高亮（注意不要高亮当前被控制的对象）
        if (newHighlight != null && newHighlight != lastHighlighted && newHighlight != currentTarget)
        {
            newHighlight.SetHighlight(true);
            lastHighlighted = newHighlight;
        }
    }
}
