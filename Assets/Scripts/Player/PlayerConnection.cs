using UnityEngine;

[DefaultExecutionOrder(-50)]
public class PlayerConnection : MonoBehaviour
{
    [SerializeField] PlatformerController player1;
    [SerializeField] PlatformerController player2;
    [SerializeField] LineRenderer line;
    [SerializeField] float maxRadius = 8f;
    [SerializeField] float reconnectDistance = 6.5f;
    [SerializeField] float tetherDrop = 0.12f;

    public bool IsLinked { get; private set; } = true;
    public float StretchRatio { get; private set; }
    public PlatformerController Player1 => player1;
    public PlatformerController Player2 => player2;

    public PlatformerController GetPartner(PlatformerController self)
    {
        if (self == player1)
            return player2;
        if (self == player2)
            return player1;
        return null;
    }

    public bool TryGetTetherSegment(out Vector3 a, out Vector3 b)
    {
        a = default;
        b = default;
        if (!IsLinked || player1 == null || player2 == null)
            return false;

        a = AntennaPoint(player1.transform);
        b = AntennaPoint(player2.transform);
        return true;
    }

    void Awake()
    {
        if (line != null)
            return;

        GameObject lineObj = new GameObject("TetherLine");
        lineObj.transform.SetParent(transform);
        line = lineObj.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.startWidth = 0.08f;
        line.endWidth = 0.08f;
        line.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
    }

    void Update()
    {
        if (player1 == null || player2 == null || line == null)
            return;

        Vector3 delta = player2.transform.position - player1.transform.position;
        delta.y = 0f;
        float distance = delta.magnitude;

        UpdateLinkState(distance);
        ResolveYank();
        UpdateLine();

        float speed = IsLinked ? Mathf.Lerp(1f, 0.65f, StretchRatio) : 1f;
        player1.SetSpeedScale(speed);
        player2.SetSpeedScale(speed);
    }

    void ResolveYank()
    {
        player1.PollYank();
        player2.PollYank();
        player1.IsBeingReeled = IsLinked && player2.IsYanking;
        player2.IsBeingReeled = IsLinked && player1.IsYanking;
    }

    // snap past max, reconnect when close again
    void UpdateLinkState(float distance)
    {
        float startDistance = 3f;
        StretchRatio = distance <= startDistance
            ? 0f
            : Mathf.Clamp01((distance - startDistance) / (maxRadius - startDistance));

        if (IsLinked && distance >= maxRadius)
            IsLinked = false;
        if (!IsLinked && distance <= reconnectDistance)
            IsLinked = true;
    }

    void UpdateLine()
    {
        line.SetPosition(0, AntennaPoint(player1.transform));
        line.SetPosition(1, AntennaPoint(player2.transform));

        if (!IsLinked)
        {
            line.enabled = false;
            return;
        }

        line.enabled = true;
        bool yanking = player1.IsYanking || player2.IsYanking;
        Color color = Color.Lerp(new Color(0.2f, 0.9f, 1f), Color.red, StretchRatio);
        if (yanking)
            color = new Color(1f, 0.85f, 0.2f);

        line.startColor = color;
        line.endColor = color;
        line.startWidth = yanking ? 0.14f : 0.08f;
        line.endWidth = yanking ? 0.14f : 0.08f;
    }

    // top of the robot mesh = antennas
    Vector3 AntennaPoint(Transform player)
    {
        float top = float.NegativeInfinity;
        Vector3 point = player.position + Vector3.up;

        foreach (Renderer renderer in player.GetComponentsInChildren<Renderer>())
        {
            if (renderer.gameObject == player.gameObject)
                continue;

            Bounds bounds = renderer.bounds;
            if (bounds.max.y > top)
            {
                top = bounds.max.y;
                point = new Vector3(bounds.center.x, bounds.max.y - tetherDrop, bounds.center.z);
            }
        }

        return point;
    }
}
