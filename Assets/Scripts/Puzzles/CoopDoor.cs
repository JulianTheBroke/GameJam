using UnityEngine;

public class CoopDoor : MonoBehaviour
{
    [SerializeField] PressurePlate[] plates;
    [SerializeField] TetherBeam[] beams;
    [SerializeField] PlayerConnection connection;
    [SerializeField] bool requireLinked;
    [SerializeField] bool stayOpen = true;
    [SerializeField] float openHeight = 3.6f;
    [SerializeField] float moveSpeed = 4f;

    Vector3 closedPosition;
    bool lockedOpen;

    public bool IsOpen => lockedOpen || ConditionsMet();

    void Awake() => closedPosition = transform.position;

    void Update()
    {
        bool met = ConditionsMet();
        if (met && stayOpen)
            lockedOpen = true; // stay up once solved

        bool shouldOpen = stayOpen ? lockedOpen || met : met;
        Vector3 target = shouldOpen ? closedPosition + Vector3.up * openHeight : closedPosition;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }

    bool ConditionsMet()
    {
        return AllPlatesSatisfied() && AllBeamsCut() && LinkOk();
    }

    bool LinkOk()
    {
        if (!requireLinked)
            return true;
        return connection != null && connection.IsLinked;
    }

    bool AllPlatesSatisfied()
    {
        if (plates == null || plates.Length == 0)
            return true;

        foreach (PressurePlate plate in plates)
        {
            if (plate == null || !plate.IsSatisfied)
                return false;
        }

        return true;
    }

    bool AllBeamsCut()
    {
        if (beams == null || beams.Length == 0)
            return true;

        foreach (TetherBeam beam in beams)
        {
            if (beam == null || !beam.IsCut)
                return false;
        }

        return true;
    }
}
