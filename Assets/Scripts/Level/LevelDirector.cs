using UnityEngine;

public class LevelDirector : MonoBehaviour
{
    [SerializeField] PlatformerController player1;
    [SerializeField] PlatformerController player2;
    [SerializeField] PlayerConnection connection;
    [SerializeField] CoopDoor door1;
    [SerializeField] CoopDoor door2;
    [SerializeField] CoopDoor door3;
    [SerializeField] CoopDoor door4;
    [SerializeField] GoalPad goal;
    [SerializeField] Transform[] checkpointsP1;
    [SerializeField] Transform[] checkpointsP2;

    int checkpointIndex;

    void Awake()
    {
        if (goal != null)
            goal.SetConnection(connection);
    }

    void Update()
    {
        if (player1 == null || player2 == null)
            return;

        // last opened door is the respawn
        if (door1 != null && door1.IsOpen)
            checkpointIndex = Mathf.Max(checkpointIndex, 1);
        if (door2 != null && door2.IsOpen)
            checkpointIndex = Mathf.Max(checkpointIndex, 2);
        if (door3 != null && door3.IsOpen)
            checkpointIndex = Mathf.Max(checkpointIndex, 3);
        if (door4 != null && door4.IsOpen)
            checkpointIndex = Mathf.Max(checkpointIndex, 4);

        // both fell
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
