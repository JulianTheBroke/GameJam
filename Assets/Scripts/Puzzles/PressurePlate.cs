using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] private Color idleColor = new Color(0.35f, 0.35f, 0.4f);
    [SerializeField] private Color activeColor = new Color(0.2f, 0.9f, 0.35f);
    [SerializeField] private Color blockedColor = new Color(0.7f, 0.25f, 0.15f);
    [SerializeField] private bool latch;
    [SerializeField] private bool requireUnlinked;
    [SerializeField] private PlayerConnection connection;

    private Renderer plateRenderer;
    private int occupants;
    private bool latched;

    public bool IsSatisfied
    {
        get
        {
            if (latch && latched)
                return true;
            return occupants > 0 && LinkAllowsPress();
        }
    }

    public void Configure(bool latchOnPress, bool unlinkedOnly, PlayerConnection tether, Color active)
    {
        latch = latchOnPress;
        requireUnlinked = unlinkedOnly;
        connection = tether;
        activeColor = active;
    }

    void Awake()
    {
        plateRenderer = GetComponent<Renderer>();
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    void FixedUpdate()
    {
        occupants = 0;
        Vector3 center = transform.position + Vector3.up * 0.35f;
        Vector3 halfExtents = new Vector3(transform.lossyScale.x * 0.4f, 0.5f, transform.lossyScale.z * 0.4f);
        Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity);

        foreach (Collider hit in hits)
        {
            if (hit.GetComponent<PlatformerController>() != null)
                occupants++;
        }

        if (latch && occupants > 0 && LinkAllowsPress())
            latched = true;
    }

    bool LinkAllowsPress()
    {
        if (!requireUnlinked)
            return true;
        return connection != null && !connection.IsLinked;
    }

    void Update()
    {
        if (plateRenderer == null)
            return;

        Color color;
        if (IsSatisfied)
            color = activeColor;
        else if (occupants > 0 && !LinkAllowsPress())
            color = blockedColor;
        else
            color = idleColor;

        ApplyColor(color);
    }

    void ApplyColor(Color color)
    {
        plateRenderer.material.color = color;
        if (plateRenderer.material.HasProperty("_BaseColor"))
            plateRenderer.material.SetColor("_BaseColor", color);
        if (plateRenderer.material.HasProperty("_EmissionColor"))
        {
            Color emission = IsSatisfied ? color * 0.45f : Color.black;
            plateRenderer.material.SetColor("_EmissionColor", emission);
            if (IsSatisfied)
                plateRenderer.material.EnableKeyword("_EMISSION");
            else
                plateRenderer.material.DisableKeyword("_EMISSION");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = IsSatisfied ? activeColor : idleColor;
        Vector3 center = transform.position + Vector3.up * 0.35f;
        Vector3 size = new Vector3(transform.lossyScale.x * 0.8f, 1f, transform.lossyScale.z * 0.8f);
        Gizmos.DrawWireCube(center, size);
    }
}
