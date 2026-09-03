using UnityEngine;

// Moves forward when started. Knocks players toward the pit (funny fail).
public class KillWall : MonoBehaviour
{
    [SerializeField] Vector3 moveDirection = Vector3.forward;
    [SerializeField] float speed = 3.5f;
    [SerializeField] float knockForce = 16f;
    [SerializeField] float upKnock = 5f;
    [SerializeField] float pitSign = 1f;
    [SerializeField] bool autoStart;

    bool running;
    Vector3 startPos;

    public void Configure(Vector3 direction, float moveSpeed, float towardPitSign)
    {
        moveDirection = direction.normalized;
        speed = moveSpeed;
        pitSign = Mathf.Sign(towardPitSign);
    }

    public void Begin() => running = true;

    public void StopAndReset()
    {
        running = false;
        transform.position = startPos;
    }

    void Awake()
    {
        startPos = transform.position;
        if (autoStart)
            running = true;

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    void Update()
    {
        if (!running)
            return;
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other) => Knock(other);
    void OnTriggerStay(Collider other) => Knock(other);

    void Knock(Collider other)
    {
        PlatformerController player = other.GetComponentInParent<PlatformerController>();
        if (player == null)
            return;
        player.AddImpulse(new Vector3(pitSign * knockForce, upKnock, -2f));
    }
}
