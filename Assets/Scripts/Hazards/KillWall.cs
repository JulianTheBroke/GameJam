using System.Collections.Generic;
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
    bool spawnPosCaptured;
    Vector3 startPos;
    readonly Dictionary<PlatformerController, float> knockCooldowns = new();

    public Vector3 SpawnPosition
    {
        get
        {
            CaptureSpawnPosition();
            return startPos;
        }
    }

    void CaptureSpawnPosition()
    {
        if (spawnPosCaptured)
            return;
        startPos = transform.position;
        spawnPosCaptured = true;
    }

    public void Configure(Vector3 direction, float moveSpeed, float towardPitSign)
    {
        moveDirection = direction.normalized;
        speed = moveSpeed;
        pitSign = Mathf.Sign(towardPitSign);
    }

    public void Begin() => running = true;

    public void SetSpawnPosition(Vector3 worldPos)
    {
        startPos = worldPos;
        spawnPosCaptured = true;
        transform.position = worldPos;
        running = false;
    }

    public void StopAndReset()
    {
        running = false;
        transform.position = startPos;
    }

    void Awake()
    {
        CaptureSpawnPosition();
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

    void Knock(Collider other)
    {
        PlatformerController player = other.GetComponentInParent<PlatformerController>();
        if (player == null)
            return;

        float now = Time.time;
        if (knockCooldowns.TryGetValue(player, out float next) && now < next)
            return;

        knockCooldowns[player] = now + 0.4f;
        player.AddImpulse(new Vector3(pitSign * knockForce, upKnock, -2f));
    }
}
