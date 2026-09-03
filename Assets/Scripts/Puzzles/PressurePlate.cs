using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] Color idleColor = new Color(0.35f, 0.35f, 0.4f);
    [SerializeField] Color activeColor = new Color(0.2f, 0.9f, 0.35f);
    [SerializeField] Color blockedColor = new Color(0.7f, 0.25f, 0.15f);
    [SerializeField] bool latch;
    [SerializeField] bool requireLinked;
    [SerializeField] bool requireUnlinked;
    [SerializeField] PlayerConnection connection;

    Renderer plateRenderer;
    readonly HashSet<PlatformerController> triggerOccupants = new();
    int occupants;
    bool latched;

    public bool IsSatisfied
    {
        get
        {
            if (latch && latched)
                return true;
            return occupants > 0 && LinkAllowsPress();
        }
    }

    public void Configure(PlayerConnection tether, bool linkedRequired, bool unlinkedRequired, bool latchOn)
    {
        connection = tether;
        requireLinked = linkedRequired;
        requireUnlinked = unlinkedRequired;
        latch = latchOn;
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
        if (connection != null)
        {
            if (PlayerInside(connection.Player1))
                occupants++;
            if (PlayerInside(connection.Player2))
                occupants++;
        }
        else
        {
            foreach (PlatformerController player in triggerOccupants)
            {
                if (player != null && PlayerInside(player))
                    occupants++;
            }
        }

        if (latch && occupants > 0 && LinkAllowsPress())
            latched = true;
    }

    void OnTriggerEnter(Collider other)
    {
        PlatformerController player = other.GetComponent<PlatformerController>();
        if (player != null)
            triggerOccupants.Add(player);
    }

    void OnTriggerExit(Collider other)
    {
        PlatformerController player = other.GetComponent<PlatformerController>();
        if (player != null)
            triggerOccupants.Remove(player);
    }

    bool PlayerInside(PlatformerController player)
    {
        if (player == null)
            return false;

        Vector3 pos = player.transform.position;
        Vector3 half = PlateHalfExtents();
        if (Mathf.Abs(pos.x - transform.position.x) > half.x)
            return false;
        if (Mathf.Abs(pos.z - transform.position.z) > half.z)
            return false;

        float feetY = pos.y;
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
            feetY = pos.y + cc.center.y - cc.height * 0.5f;

        float plateTop = transform.position.y + transform.lossyScale.y * 0.5f;
        return feetY <= plateTop + 0.35f && feetY >= plateTop - 0.25f;
    }

    Vector3 PlateHalfExtents() =>
        new Vector3(transform.lossyScale.x * 0.45f, 0.5f, transform.lossyScale.z * 0.45f);

    bool LinkAllowsPress()
    {
        if (requireLinked)
            return connection != null && connection.IsLinked;
        if (requireUnlinked)
            return connection != null && !connection.IsLinked;
        return true;
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
        if (!plateRenderer.material.HasProperty("_EmissionColor"))
            return;

        Color emission = IsSatisfied ? color * 0.45f : Color.black;
        plateRenderer.material.SetColor("_EmissionColor", emission);
        if (IsSatisfied)
            plateRenderer.material.EnableKeyword("_EMISSION");
        else
            plateRenderer.material.DisableKeyword("_EMISSION");
    }
}
