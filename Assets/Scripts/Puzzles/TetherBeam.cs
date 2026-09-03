using UnityEngine;

// Vertical power wire. Player tether crossing it cuts once:
// top stub dangles from the ceiling mount, bottom half flops to the floor.
public class TetherBeam : MonoBehaviour
{
    [SerializeField] Transform top;       // ceiling mount
    [SerializeField] Transform bottom;    // floor mount (unused after cut)
    [SerializeField] PlayerConnection connection;
    [SerializeField] LineRenderer line;
    [SerializeField] float hitRadius = 0.9f;
    [SerializeField] int segments = 10;
    [SerializeField] float flopSpeed = 6f;

    // Legacy aliases so old scenes still wire
    [SerializeField] Transform start;
    [SerializeField] Transform end;

    readonly Color liveColor = new Color(1f, 0.85f, 0.15f);
    readonly Color deadColor = new Color(0.35f, 0.35f, 0.38f);

    bool cutLatched;
    float flopT;
    Vector3 cutPoint;
    Vector3 bottomTip; // animated end of falling half

    public bool IsCut => cutLatched;

    public void Configure(PlayerConnection tether, Transform a, Transform b, LineRenderer lineRenderer, float radius = 0.9f)
    {
        connection = tether;
        top = a;
        bottom = b;
        start = a;
        end = b;
        line = lineRenderer;
        hitRadius = radius;
    }

    void Awake()
    {
        if (top == null) top = start;
        if (bottom == null) bottom = end;
        if (line != null)
        {
            line.positionCount = segments;
            line.useWorldSpace = true;
        }
    }

    void Update()
    {
        if (top == null || bottom == null || line == null)
            return;

        if (!cutLatched)
            TryCut();

        if (cutLatched)
            AnimateCut();
        else
            DrawLiveWire();
    }

    void TryCut()
    {
        if (connection == null || !connection.TryGetTetherSegment(out Vector3 a, out Vector3 b))
            return;
        if (SegmentDistance(a, b, top.position, bottom.position) > hitRadius)
            return;

        cutLatched = true;
        flopT = 0f;
        cutPoint = Vector3.Lerp(top.position, bottom.position, 0.45f);
        bottomTip = cutPoint;
        line.startColor = deadColor;
        line.endColor = deadColor;
        if (line.material != null)
        {
            line.material.color = deadColor;
            if (line.material.HasProperty("_BaseColor"))
                line.material.SetColor("_BaseColor", deadColor);
        }
    }

    void DrawLiveWire()
    {
        Vector3 a = top.position;
        Vector3 b = bottom.position;
        for (int i = 0; i < segments; i++)
        {
            float t = i / (segments - 1f);
            // slight sag so it reads as a cable
            Vector3 p = Vector3.Lerp(a, b, t);
            p.x += Mathf.Sin(t * Mathf.PI) * 0.08f;
            line.SetPosition(i, p);
        }
        line.startColor = liveColor;
        line.endColor = liveColor;
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        if (line.material != null)
        {
            line.material.color = liveColor;
            if (line.material.HasProperty("_BaseColor"))
                line.material.SetColor("_BaseColor", liveColor);
        }
    }

    void AnimateCut()
    {
        flopT = Mathf.MoveTowards(flopT, 1f, flopSpeed * Time.deltaTime);

        // Top stub hangs from mount down to cut
        Vector3 hangEnd = Vector3.Lerp(cutPoint, top.position + Vector3.down * 0.55f, 0.35f);
        hangEnd.x = top.position.x + Mathf.Sin(Time.time * 2.2f) * 0.12f * flopT;

        // Bottom half falls / flops onto the floor near the mount
        Vector3 floorPoint = new Vector3(bottom.position.x + 0.8f, bottom.position.y, bottom.position.z);
        bottomTip = Vector3.Lerp(cutPoint, floorPoint, flopT * flopT);
        // curl the falling tip
        Vector3 midFall = Vector3.Lerp(cutPoint, bottomTip, 0.5f);
        midFall.y = Mathf.Lerp(cutPoint.y, bottom.position.y + 0.05f, flopT);

        int half = segments / 2;
        for (int i = 0; i < half; i++)
        {
            float t = i / (half - 1f);
            line.SetPosition(i, Vector3.Lerp(top.position, hangEnd, t));
        }
        for (int i = half; i < segments; i++)
        {
            float t = (i - half) / (segments - half - 1f);
            Vector3 p = t < 0.5f
                ? Vector3.Lerp(hangEnd, midFall, t * 2f)
                : Vector3.Lerp(midFall, bottomTip, (t - 0.5f) * 2f);
            line.SetPosition(i, p);
        }

        line.startWidth = 0.08f;
        line.endWidth = 0.08f;
    }

    static float SegmentDistance(Vector3 p1, Vector3 q1, Vector3 p2, Vector3 q2)
    {
        Vector3 d1 = q1 - p1;
        Vector3 d2 = q2 - p2;
        Vector3 r = p1 - p2;
        float a = Vector3.Dot(d1, d1);
        float e = Vector3.Dot(d2, d2);
        float f = Vector3.Dot(d2, r);
        const float eps = 0.000001f;
        float s, t;
        if (a <= eps && e <= eps)
            return Vector3.Distance(p1, p2);
        if (a <= eps)
        {
            s = 0f;
            t = Mathf.Clamp01(f / e);
        }
        else
        {
            float c = Vector3.Dot(d1, r);
            if (e <= eps)
            {
                t = 0f;
                s = Mathf.Clamp01(-c / a);
            }
            else
            {
                float b = Vector3.Dot(d1, d2);
                float denom = a * e - b * b;
                s = Mathf.Abs(denom) > eps ? Mathf.Clamp01((b * f - c * e) / denom) : 0f;
                t = (b * s + f) / e;
                if (t < 0f)
                {
                    t = 0f;
                    s = Mathf.Clamp01(-c / a);
                }
                else if (t > 1f)
                {
                    t = 1f;
                    s = Mathf.Clamp01((b - c) / a);
                }
            }
        }
        return Vector3.Distance(p1 + d1 * s, p2 + d2 * t);
    }
}
