using UnityEngine;

// Simple stage watcher for ChaseRoom (already in the scene — edit there).
// Cut wire → gate opens → cross bridge → walls run → linked plates stop walls.
public class ChaseDirector : MonoBehaviour
{
    [SerializeField] PlatformerController player1;
    [SerializeField] PlatformerController player2;
    [SerializeField] CoopDoor startGate;
    [SerializeField] KillWall wallLeft;
    [SerializeField] KillWall wallRight;
    [SerializeField] PressurePlate plateLeft;
    [SerializeField] PressurePlate plateRight;
    [SerializeField] Transform startP1;
    [SerializeField] Transform startP2;
    [SerializeField] Transform chaseP1;
    [SerializeField] Transform chaseP2;
    [SerializeField] float chaseStartZ = 22f;

    int checkpoint;
    bool chaseStarted;
    bool finished;

    void Start() => Teleport(startP1, startP2);

    void Update()
    {
        if (player1 == null || player2 == null)
            return;

        if (player1.transform.position.y < -8f || player2.transform.position.y < -8f)
            Respawn();

        if (finished)
            return;

        if (checkpoint < 1 && startGate != null && startGate.IsOpen)
            checkpoint = 1;

        if (!chaseStarted && checkpoint >= 1
            && player1.transform.position.z > chaseStartZ
            && player2.transform.position.z > chaseStartZ)
        {
            chaseStarted = true;
            checkpoint = 2;
            wallLeft?.Begin();
            wallRight?.Begin();
        }

        if (chaseStarted && plateLeft != null && plateRight != null
            && plateLeft.IsSatisfied && plateRight.IsSatisfied)
        {
            finished = true;
            checkpoint = 3;
            wallLeft?.StopAndReset();
            wallRight?.StopAndReset();
        }
    }

    void Respawn()
    {
        Teleport(checkpoint >= 2 ? chaseP1 : startP1, checkpoint >= 2 ? chaseP2 : startP2);
        if (chaseStarted && !finished)
        {
            wallLeft?.StopAndReset();
            wallRight?.StopAndReset();
            wallLeft?.Begin();
            wallRight?.Begin();
        }
    }

    void Teleport(Transform a, Transform b)
    {
        if (a != null) player1.Teleport(a.position);
        if (b != null) player2.Teleport(b.position);
    }
}
