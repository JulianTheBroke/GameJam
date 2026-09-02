using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlatformerController : MonoBehaviour
{
    [Header("Linked (together)")]
    [SerializeField] private float linkedMoveSpeed = 5f;
    [SerializeField] private float linkedAirControl = 0.3f;

    [Header("Split (apart)")]
    [SerializeField] private float splitMoveSpeed = 7f;
    [SerializeField] private float splitAirControl = 0.75f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2.2f;
    [SerializeField] private float gravity = -24f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    private CharacterController controller;
    private InputAction moveAction;
    private InputAction jumpAction;
    private PlayerConnection connection;
    private Transform moveCamera;
    private Vector3 horizontalVelocity;
    private Vector3 velocity;
    private float speedScale = 1f;
    private float coyoteTimer;
    private float jumpBufferTimer;

    public bool IsLinked => connection != null && connection.IsLinked;
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

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        SnapToGround();
    }

    void SnapToGround()
    {
        Vector3 pos = transform.position;
        pos.y = 1f;
        transform.position = pos;

        if (Physics.Raycast(pos + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 10f))
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
