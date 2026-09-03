using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlatformerController : MonoBehaviour
{
    [Header("Linked (together)")]
    [SerializeField] float linkedMoveSpeed = 5.5f;
    [SerializeField] float linkedAirControl = 0.35f;
    [SerializeField] float linkedJumpHeight = 2.2f;

    [Header("Split (apart) — half power")]
    [SerializeField] float splitMoveSpeed = 2.75f;
    [SerializeField] float splitAirControl = 0.55f;
    [SerializeField] float splitJumpHeight = 1.1f;

    [Header("Jump")]
    [SerializeField] float gravity = -24f;
    [SerializeField] float coyoteTime = 0.1f;
    [SerializeField] float jumpBufferTime = 0.12f;

    CharacterController controller;
    InputAction moveAction;
    InputAction jumpAction;
    PlayerConnection connection;
    Transform moveCamera;
    Vector3 horizontalVelocity;
    Vector3 velocity;
    Vector3 impulseVelocity;
    float speedScale = 1f;
    float coyoteTimer;
    float jumpBufferTimer;

    public bool IsLinked => connection != null && connection.IsLinked;
    public bool UseSplitStats => connection != null && !connection.IsLinked && connection.ParkourPunishmentActive;
    public float PlanarSpeed => horizontalVelocity.magnitude;
    public Vector2 MoveInput => moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
    public bool JumpPressedThisFrame => jumpAction != null && jumpAction.WasPressedThisFrame();
    public bool JumpHeld => jumpAction != null && jumpAction.IsPressed();
    public InputActionAsset InputActions { get; private set; }

    public void Setup(string mapName, InputActionAsset sourceAsset)
    {
        InputActions = Instantiate(sourceAsset);
        InputActionMap map = InputActions.FindActionMap(mapName, true);
        moveAction = map.FindAction("Move", true);
        jumpAction = map.FindAction("Jump", true);
        map.Enable();
    }

    public void SetConnection(PlayerConnection tether) => connection = tether;
    public void SetMoveCamera(Transform cam) => moveCamera = cam;
    public void SetSpeedScale(float scale) => speedScale = scale;

    public void AddImpulse(Vector3 impulse) => impulseVelocity += impulse;

    public void ClearImpulse() => impulseVelocity = Vector3.zero;

    public void Teleport(Vector3 position)
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        bool wasEnabled = controller.enabled;
        controller.enabled = false;
        transform.position = position;
        controller.enabled = wasEnabled;
        velocity = Vector3.zero;
        horizontalVelocity = Vector3.zero;
        impulseVelocity = Vector3.zero;
    }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        SnapToGround();
    }

    void SnapToGround()
    {
        Vector3 pos = transform.position;
        if (Physics.Raycast(pos + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 12f))
        {
            float bottomOffset = controller.center.y - controller.height * 0.5f;
            pos.y = hit.point.y - bottomOffset + 0.05f;
            transform.position = pos;
        }

        velocity = Vector3.zero;
        horizontalVelocity = Vector3.zero;
        impulseVelocity = Vector3.zero;
    }

    void Update()
    {
        if (moveAction == null)
            return;

        bool grounded = IsGrounded();
        if (grounded)
        {
            coyoteTimer = coyoteTime;
            if (velocity.y < 0f)
                velocity.y = -2f;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (jumpAction.WasPressedThisFrame())
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 wishDir = GetMoveDirection(input);
        float moveSpeed = (UseSplitStats ? splitMoveSpeed : linkedMoveSpeed) * speedScale;
        float airControl = UseSplitStats ? splitAirControl : linkedAirControl;
        float jumpHeight = UseSplitStats ? splitJumpHeight : linkedJumpHeight;

        if (grounded)
            horizontalVelocity = wishDir * moveSpeed;
        else
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, wishDir * moveSpeed, airControl * Time.deltaTime * 10f);

        if (wishDir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(wishDir), 12f * Time.deltaTime);

        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        velocity.y += gravity * Time.deltaTime;
        impulseVelocity = Vector3.Lerp(impulseVelocity, Vector3.zero, 8f * Time.deltaTime);

        Vector3 move = (horizontalVelocity + impulseVelocity) * Time.deltaTime;
        move.y = velocity.y * Time.deltaTime + impulseVelocity.y * Time.deltaTime;
        controller.Move(move);
    }

    bool IsGrounded()
    {
        if (controller.isGrounded)
            return true;

        Vector3 origin = transform.position + controller.center;
        float radius = controller.radius * 0.9f;
        float distance = controller.height * 0.5f + 0.15f;
        return Physics.SphereCast(origin, radius, Vector3.down, out _, distance);
    }

    Vector3 GetMoveDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 0.01f)
            return Vector3.zero;

        Transform cam = moveCamera != null ? moveCamera : Camera.main.transform;
        Vector3 forward = cam.forward;
        Vector3 right = cam.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        return (forward * input.y + right * input.x).normalized;
    }
}
