using UnityEngine;

// Vertical power cable. Live = straight + glow. Cut once by player tether:
// top half dangles, bottom half falls to the floor, sparks fire at the cut.
public class TetherBeam : MonoBehaviour
{
    [SerializeField] Transform top;
    [SerializeField] Transform bottom;
    [SerializeField] PlayerConnection connection;
    [SerializeField] LineRenderer line;          // live cable / top dangling half
    [SerializeField] LineRenderer fallingLine;   // bottom half after cut (created if missing)
    [SerializeField] GameObject sparkPrefab;
    [SerializeField] float hitRadius = 0.9f;
    [SerializeField] int hangSegments = 6;
    [SerializeField] float flopSpeed = 5f;
    [SerializeField] float glowIntensity = 2.2f;

    // Legacy scene aliases
    [SerializeField] Transform start;
    [SerializeField] Transform end;

    readonly Color liveColor = new Color(1f, 0.9f, 0.25f);
    readonly Color deadColor = new Color(0.4f, 0.4f, 0.42f);

    bool cutLatched;
    float flopT;
    Vector3 cutPoint;
    Light glow;

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
            line.useWorldSpace = true;
            line.positionCount = 2;
        }

        EnsureFallingLine();
        EnsureGlow();
    }

    void EnsureFallingLine()
    {
        if (fallingLine != null || line == null)
            return;

        var go = new GameObject("FallingWire");
        go.transform.SetParent(transform, false);
        fallingLine = go.AddComponent<LineRenderer>();
        fallingLine.sharedMaterial = line.sharedMaterial;
        fallingLine.startWidth = line.startWidth;
        fallingLine.endWidth = line.endWidth;
        fallingLine.useWorldSpace = true;
        fallingLine.positionCount = 2;
        fallingLine.enabled = false;
    }

    void EnsureGlow()
    {
        glow = GetComponent<Light>();
        if (glow == null)
            glow = gameObject.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.color = liveColor;
        glow.range = 4.5f;
        glow.intensity = glowIntensity;
        glow.shadows = LightShadows.None;
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
        cutPoint = Vector3.Lerp(top.position, bottom.position, 0.5f);

        // Top half only on main line
        line.positionCount = hangSegments;
        line.startColor = deadColor;
        line.endColor = deadColor;
        ApplyLineColor(line, deadColor);

        if (fallingLine != null)
        {
            fallingLine.enabled = true;
            fallingLine.positionCount = 2;
            fallingLine.startColor = deadColor;
            fallingLine.endColor = deadColor;
            ApplyLineColor(fallingLine, deadColor);
            fallingLine.SetPosition(0, cutPoint);
            fallingLine.SetPosition(1, bottom.position);
        }

        if (glow != null)
            glow.enabled = false;

        SpawnSparks(cutPoint);
    }

    void SpawnSparks(Vector3 at)
    {
        if (sparkPrefab == null)
            return;

        GameObject fx = Instantiate(sparkPrefab, at, Quaternion.identity);
        foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>())
        {
            var main = ps.main;
            main.playOnAwake = true;
            ps.Play(true);
        }
        Destroy(fx, 3f);
    }

    void DrawLiveWire()
    {
        // Straight energized cable
        line.positionCount = 2;
        line.SetPosition(0, top.position);
        line.SetPosition(1, bottom.position);
        line.startColor = liveColor;
        line.endColor = liveColor;
        line.startWidth = 0.11f;
        line.endWidth = 0.11f;
        ApplyLineColor(line, liveColor);

        if (fallingLine != null)
            fallingLine.enabled = false;

        if (glow != null)
        {
            glow.enabled = true;
            glow.intensity = glowIntensity;
            glow.transform.position = Vector3.Lerp(top.position, bottom.position, 0.5f);
        }
    }

    void AnimateCut()
    {
        flopT = Mathf.MoveTowards(flopT, 1f, flopSpeed * Time.deltaTime);

        // Top half: hangs from ceiling, sways a little
        Vector3 hangTip = top.position + Vector3.down * 0.7f;
        hangTip.x += Mathf.Sin(Time.time * 2.4f) * 0.18f * flopT;
        hangTip.z += Mathf.Cos(Time.time * 1.7f) * 0.08f * flopT;
        for (int i = 0; i < hangSegments; i++)
        {
            float t = i / (hangSegments - 1f);
            // soft curve so it reads as dangling wire
            Vector3 p = Vector3.Lerp(top.position, hangTip, t);
            p.x += Mathf.Sin(t * Mathf.PI) * 0.06f * flopT;
            line.SetPosition(i, p);
        }
        line.startWidth = 0.09f;
        line.endWidth = 0.07f;

        if (fallingLine == null)
            return;

        // Bottom half: drops from cut point and settles on the floor as a short strand
        float len = Vector3.Distance(cutPoint, bottom.position) * 0.85f;
        Vector3 floorA = new Vector3(bottom.position.x, bottom.position.y + 0.04f, bottom.position.z);
        Vector3 floorB = floorA + Vector3.right * Mathf.Max(0.4f, len);

        Vector3 fallStart = Vector3.Lerp(cutPoint, floorA, flopT * flopT);
        Vector3 fallEnd = Vector3.Lerp(bottom.position, floorB, flopT);
        // keep mid sag while falling
        if (flopT < 1f)
        {
            fallStart.y = Mathf.Lerp(cutPoint.y, floorA.y, flopT * flopT);
            fallEnd.y = Mathf.Lerp(bottom.position.y, floorB.y, flopT);
        }

        fallingLine.SetPosition(0, fallStart);
        fallingLine.SetPosition(1, fallEnd);
        fallingLine.startWidth = 0.09f;
        fallingLine.endWidth = 0.09f;
    }

    static void ApplyLineColor(LineRenderer lr, Color color)
    {
        if (lr.material == null)
            return;
        lr.material.color = color;
        if (lr.material.HasProperty("_BaseColor"))
            lr.material.SetColor("_BaseColor", color);
        if (lr.material.HasProperty("_EmissionColor"))
        {
            lr.material.EnableKeyword("_EMISSION");
            lr.material.SetColor("_EmissionColor", color * (color.g > 0.5f ? 1.5f : 0f));
        }
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
