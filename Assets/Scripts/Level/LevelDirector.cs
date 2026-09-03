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
    bool complete;

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

        if (goal != null && goal.Reached)
            complete = true;

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

    void OnGUI()
    {
        string state = connection != null && connection.IsLinked ? "LINKED" : "SPLIT";
        Color stateColor = connection != null && connection.IsLinked
            ? new Color(0.25f, 0.9f, 1f)
            : new Color(1f, 0.48f, 0.22f);

        GUIStyle box = new GUIStyle(GUI.skin.box);
        box.wordWrap = true;
        box.fontSize = 16;
        box.alignment = TextAnchor.UpperLeft;
        box.normal.textColor = Color.white;
        box.padding = new RectOffset(14, 14, 10, 10);
        GUI.Box(new Rect(16f, 16f, 560f, 108f), $"{state}\n{ObjectiveText()}\nP1  WASD Space  Shift yank  E ping     P2  Arrows Enter  RShift yank  RCtrl ping", box);

        Color old = GUI.color;
        GUI.color = stateColor;
        GUI.DrawTexture(new Rect(16f, 16f, 8f, 108f), Texture2D.whiteTexture);
        GUI.color = old;
    }

    string ObjectiveText()
    {
        if (complete)
            return "CONNECTED. You made it through as a pair.";
        if (door1 == null || !door1.IsOpen)
            return "Both on the plates. Stay close so the tether does not snap. Yank a lagging partner.";
        if (!door2.IsOpen)
            return "Walk far apart until the cameras split. Ping, jump the orange pads, then meet in the cyan circle.";
        if (!door3.IsOpen)
            return "Stand on opposite banks so the tether cuts the red beam. Yank if someone falls in.";
        if (player1.transform.position.z < 63f || player2.transform.position.z < 63f)
            return "One player takes the left catwalk. Reconnect at the far ledge, then hold Yank to reel the other under the ceiling.";
        if (door4 == null || !door4.IsOpen)
            return "Final room: stand on both plates while linked so the tether cuts the beam.";
        return "Step onto the gold pad together.";
    }
}
