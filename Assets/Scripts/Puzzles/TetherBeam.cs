using UnityEngine;

public class TetherBeam : MonoBehaviour
{
    [SerializeField] private Transform start;
    [SerializeField] private Transform end;
    [SerializeField] private PlayerConnection connection;
    [SerializeField] private LineRenderer line;
    [SerializeField] private float hitRadius = 0.85f;

    private readonly Color idleColor = new Color(1f, 0.18f, 0.22f);
    private readonly Color cutColor = new Color(0.25f, 1f, 0.45f);

    public bool IsCut { get; private set; }

    public void Configure(Transform beamStart, Transform beamEnd, PlayerConnection tether, LineRenderer renderer)
    {
        start = beamStart;
        end = beamEnd;
        connection = tether;
        line = renderer;
    }

    void Update()
    {
        IsCut = false;
        if (connection != null && connection.TryGetTetherSegment(out Vector3 a, out Vector3 b) && start != null && end != null)
            IsCut = SegmentDistance(a, b, start.position, end.position) <= hitRadius;

        if (line == null)
            return;

        line.SetPosition(0, start.position);
        line.SetPosition(1, end.position);
        Color color = IsCut ? cutColor : idleColor;
        line.startColor = color;
        line.endColor = color;
        line.material.SetColor("_BaseColor", color);
        line.material.color = color;
        line.startWidth = IsCut ? 0.16f : 0.1f;
        line.endWidth = IsCut ? 0.16f : 0.1f;
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

        float s;
        float t;
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
