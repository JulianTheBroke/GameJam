using UnityEngine;

public class PlayerConnection : MonoBehaviour
{
    [SerializeField] private PlatformerController player1;
    [SerializeField] private PlatformerController player2;
    [SerializeField] private LineRenderer line;
    [SerializeField] private float maxRadius = 8f;
    [SerializeField] private float reconnectDistance = 5f;

    public bool IsLinked { get; private set; } = true;
    public float StretchRatio { get; private set; }

    void Awake()
    {
        if (line == null)
        {
            GameObject lineObj = new GameObject("TetherLine");
            lineObj.transform.SetParent(transform);
            line = lineObj.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.startWidth = 0.08f;
            line.endWidth = 0.08f;
            line.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        }
    }

    void Update()
    {
        if (player1 == null || player2 == null || line == null)
            return;

        Vector3 delta = player2.transform.position - player1.transform.position;
        delta.y = 0f;
        float distance = delta.magnitude;

        UpdateLinkState(distance);
        UpdateLine();

        float speed = IsLinked ? Mathf.Lerp(1f, 0.65f, StretchRatio) : 1f;
        player1.SetSpeedScale(speed);
        player2.SetSpeedScale(speed);
    }

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
        line.SetPosition(0, player1.transform.position + Vector3.up);
        line.SetPosition(1, player2.transform.position + Vector3.up);

        if (!IsLinked)
        {
            line.enabled = false;
            return;
        }

        line.enabled = true;
        Color color = Color.Lerp(new Color(0.2f, 0.9f, 1f), Color.red, StretchRatio);
        line.startColor = color;
        line.endColor = color;
    }
}
