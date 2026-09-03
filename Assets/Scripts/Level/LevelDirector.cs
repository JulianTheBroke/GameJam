using UnityEngine;

public class LevelDirector : MonoBehaviour
{
    [SerializeField] private PlatformerController player1;
    [SerializeField] private PlatformerController player2;
    [SerializeField] private PlayerConnection connection;
    [SerializeField] private CoopDoor door1;
    [SerializeField] private CoopDoor door2;
    [SerializeField] private CoopDoor door3;
    [SerializeField] private CoopDoor door4;
    [SerializeField] private GoalPad goal;
    [SerializeField] private Transform[] checkpointsP1;
    [SerializeField] private Transform[] checkpointsP2;
    [SerializeField] private Color playerOneColor = new Color(0.25f, 0.75f, 1f);
    [SerializeField] private Color playerTwoColor = new Color(1f, 0.48f, 0.38f);

    int checkpointIndex;

    void Awake()
    {
        if (player1 != null)
            player1.SetPingColor(playerOneColor);
        if (player2 != null)
            player2.SetPingColor(playerTwoColor);
        if (goal != null)
            goal.SetConnection(connection);
    }

    void Update()
    {
        if (player1 == null || player2 == null)
            return;

        if (door1 != null && door1.IsOpen)
            checkpointIndex = Mathf.Max(checkpointIndex, 1);
        if (door2 != null && door2.IsOpen)
            checkpointIndex = Mathf.Max(checkpointIndex, 2);
        if (door3 != null && door3.IsOpen)
            checkpointIndex = Mathf.Max(checkpointIndex, 3);
        if (door4 != null && door4.IsOpen)
            checkpointIndex = Mathf.Max(checkpointIndex, 4);

        if (player1.transform.position.y < -5f || player2.transform.position.y < -5f)
            Respawn();
    }

    void Respawn()
    {
        int i = Mathf.Clamp(checkpointIndex, 0, Mathf.Min(checkpointsP1.Length, checkpointsP2.Length) - 1);
        if (checkpointsP1 == null || checkpointsP2 == null || i < 0)
            return;
        if (checkpointsP1[i] != null)
            player1.Teleport(checkpointsP1[i].position);
        if (checkpointsP2[i] != null)
            player2.Teleport(checkpointsP2[i].position);
    }
}
