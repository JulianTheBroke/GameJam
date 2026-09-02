using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] private Color idleColor = new Color(0.35f, 0.35f, 0.4f);
    [SerializeField] private Color activeColor = new Color(0.2f, 0.9f, 0.35f);

    private Renderer plateRenderer;
    private int occupants;

    public bool IsPressed => occupants > 0;

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
    }

    void Update()
    {
        if (plateRenderer == null)
            return;

        Color color = IsPressed ? activeColor : idleColor;
        plateRenderer.material.color = color;
        if (plateRenderer.material.HasProperty("_BaseColor"))
            plateRenderer.material.SetColor("_BaseColor", color);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = IsPressed ? activeColor : idleColor;
        Vector3 center = transform.position + Vector3.up * 0.35f;
        Vector3 size = new Vector3(transform.lossyScale.x * 0.8f, 1f, transform.lossyScale.z * 0.8f);
        Gizmos.DrawWireCube(center, size);
    }
}
