using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlatformerController : MonoBehaviour
{
    [Header("Linked (together)")]
    [SerializeField] float linkedMoveSpeed = 5f;
    [SerializeField] float linkedAirControl = 0.3f;
    [SerializeField] float linkedJumpHeight = 2.1f;

    [Header("Split (apart)")]
    [SerializeField] float splitMoveSpeed = 7f;
    [SerializeField] float splitAirControl = 0.75f;
    [SerializeField] float splitJumpHeight = 3.35f;

    [Header("Jump")]
    [SerializeField] float gravity = -24f;
    [SerializeField] float coyoteTime = 0.1f;
    [SerializeField] float jumpBufferTime = 0.12f;

    [Header("Yank")]
    [SerializeField] float yankReelSpeed = 16f;

    CharacterController controller;
    InputAction moveAction;
    InputAction jumpAction;
    InputAction yankAction;
    PlayerConnection connection;
    Transform moveCamera;
    Vector3 horizontalVelocity;
    Vector3 velocity;
    float speedScale = 1f;
    float coyoteTimer;
    float jumpBufferTimer;

    public bool IsLinked => connection != null && connection.IsLinked;
    public bool IsYanking { get; private set; }
    public bool IsBeingReeled { get; set; }
    public float PlanarSpeed => horizontalVelocity.magnitude;
    public InputActionAsset InputActions { get; private set; }

    public void Setup(string mapName, InputActionAsset sourceAsset)
    {
        InputActions = Instantiate(sourceAsset);
        InputActionMap map = InputActions.FindActionMap(mapName, true);
        moveAction = map.FindAction("Move", true);
        jumpAction = map.FindAction("Jump", true);
        yankAction = map.FindAction("Yank");
        map.Enable();
    }

    public void SetConnection(PlayerConnection tether) => connection = tether;
    public void SetMoveCamera(Transform cam) => moveCamera = cam;
    public void SetSpeedScale(float scale) => speedScale = scale;

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
        IsBeingReeled = false;
        IsYanking = false;
    }

    public void PollYank()
    {
        IsYanking = yankAction != null && yankAction.IsPressed() && IsLinked;
    }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        SnapToGround();
    }

    // sit on the floor at start / respawn
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
    }

    void Update()
    {
        if (moveAction == null)
            return;

        // partner is pulling us in
        if (IsBeingReeled)
        {
            ApplyReelMove();
            return;
        }

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
        float moveSpeed = (IsLinked ? linkedMoveSpeed : splitMoveSpeed) * speedScale;
        float airControl = IsLinked ? linkedAirControl : splitAirControl;
        float jumpHeight = IsLinked ? linkedJumpHeight : splitJumpHeight;

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
        Vector3 move = horizontalVelocity * Time.deltaTime;
        move.y = velocity.y * Time.deltaTime;
        controller.Move(move);
    }

    // slide toward the yanking partner
    void ApplyReelMove()
    {
        PlatformerController partner = connection != null ? connection.GetPartner(this) : null;
        if (partner == null || !partner.IsYanking || !IsLinked)
        {
            IsBeingReeled = false;
            return;
        }

        Vector3 delta = partner.transform.position - transform.position;
        if (delta.magnitude < 0.9f)
        {
            velocity.y = 0f;
            horizontalVelocity = Vector3.zero;
            return;
        }

        controller.Move(Vector3.ClampMagnitude(delta, yankReelSpeed * Time.deltaTime));
        velocity.y = 0f;
        horizontalVelocity = Vector3.zero;
        if (delta.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(delta.normalized), 16f * Time.deltaTime);
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

    // camera-relative stick
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
