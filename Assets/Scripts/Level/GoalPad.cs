using UnityEngine;

public class GoalPad : MonoBehaviour
{
    [SerializeField] private PlayerConnection connection;

    public bool Reached { get; private set; }

    public void SetConnection(PlayerConnection tether) => connection = tether;

    void Update()
    {
        if (Reached || connection == null || !connection.IsLinked)
            return;

        Collider[] hits = Physics.OverlapBox(transform.position + Vector3.up * 0.6f, new Vector3(2.6f, 0.8f, 1.4f));
        bool p1 = false;
        bool p2 = false;
        foreach (Collider hit in hits)
        {
            PlatformerController player = hit.GetComponent<PlatformerController>();
            if (player == connection.Player1)
                p1 = true;
            if (player == connection.Player2)
                p2 = true;
        }

        Reached = p1 && p2;
    }
}
