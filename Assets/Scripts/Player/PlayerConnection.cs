using UnityEngine;

// Walk far = soft rubberband, then snap. Walk close = reconnect. No extra push on reconnect.
[DefaultExecutionOrder(-50)]
public class PlayerConnection : MonoBehaviour
{
    [SerializeField] PlatformerController player1;
    [SerializeField] PlatformerController player2;
    [SerializeField] LineRenderer line;
    [SerializeField] float softRadius = 7f;      // spring starts here
    [SerializeField] float maxRadius = 9.5f;     // snap / unlink
    [SerializeField] float reconnectDistance = 7f;
    [SerializeField] float springStrength = 4.5f;
    [SerializeField] float snapBounce = 2.5f;    // only on snap-apart
    [SerializeField] float tetherDrop = 0.12f;
    [SerializeField] float healthyRadius = 7f;   // HUD stays full until here
    [SerializeField] float barFalloffPower = 2.4f;

    public bool IsLinked { get; private set; } = true;
    public float StretchRatio { get; private set; }
    public float CurrentDistance { get; private set; }
    public bool JustSnapped { get; private set; }
    public bool JustReconnected { get; private set; }
    public PlatformerController Player1 => player1;
    public PlatformerController Player2 => player2;

    // 0 = far while split, 1 = close enough to almost reconnect
    public float ReconnectProgress
    {
        get
        {
            if (IsLinked)
                return 1f;
            float far = maxRadius + 2f;
            return 1f - Mathf.Clamp01((CurrentDistance - reconnectDistance) / Mathf.Max(0.01f, far - reconnectDistance));
        }
    }

    // Kept for stretch-tinted tether color / misc
    public float ConnectionStrength
    {
        get
        {
            if (CurrentDistance <= healthyRadius)
                return 1f;
            float span = Mathf.Max(0.01f, maxRadius - healthyRadius);
            float t = Mathf.Clamp01((CurrentDistance - healthyRadius) / span);
            return 1f - Mathf.Pow(t, barFalloffPower);
        }
    }

    public PlatformerController GetPartner(PlatformerController self)
    {
        if (self == player1) return player2;
        if (self == player2) return player1;
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
        JustSnapped = false;
        JustReconnected = false;
        if (player1 == null || player2 == null || line == null)
            return;

        Vector3 delta = player2.transform.position - player1.transform.position;
        delta.y = 0f;
        CurrentDistance = delta.magnitude;
        Vector3 dir = CurrentDistance > 0.001f ? delta / CurrentDistance : Vector3.right;

        StretchRatio = CurrentDistance <= softRadius
            ? 0f
            : Mathf.Clamp01((CurrentDistance - softRadius) / (maxRadius - softRadius));

        if (IsLinked && CurrentDistance > softRadius)
        {
            float pull = (CurrentDistance - softRadius) * springStrength * Time.deltaTime;
            player1.AddImpulse(dir * pull);
            player2.AddImpulse(-dir * pull);
        }

        if (IsLinked && CurrentDistance >= maxRadius)
        {
            IsLinked = false;
            JustSnapped = true;
            player1.AddImpulse(-dir * snapBounce);
            player2.AddImpulse(dir * snapBounce);
        }
        else if (!IsLinked && CurrentDistance <= reconnectDistance)
        {
            IsLinked = true;
            JustReconnected = true;
            // Kill leftover impulses so reconnect doesn't yeet anyone
            player1.ClearImpulse();
            player2.ClearImpulse();
        }

        UpdateLine();
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
        Color color = Color.Lerp(new Color(0.2f, 0.9f, 1f), Color.red, StretchRatio);
        if (JustReconnected)
            color = new Color(0.45f, 1f, 0.55f);
        line.startColor = color;
        line.endColor = color;
        line.startWidth = Mathf.Lerp(0.08f, 0.12f, StretchRatio);
        line.endWidth = line.startWidth;
    }

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
