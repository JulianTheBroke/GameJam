using UnityEngine;

// Opens when all beams are cut and/or plates are satisfied.
public class CoopDoor : MonoBehaviour
{
    [SerializeField] PressurePlate[] plates;
    [SerializeField] TetherBeam[] beams;
    [SerializeField] PlayerConnection connection;
    [SerializeField] bool requireLinked;
    [SerializeField] bool stayOpen = true;
    [SerializeField] float openHeight = 28f;
    [SerializeField] float moveSpeed = 12f;
    [SerializeField] bool deactivateWhenOpen = true;

    Vector3 closedPosition;
    bool lockedOpen;
    bool vanished;

    public bool IsOpen => lockedOpen || ConditionsMet();

    public void Configure(PlayerConnection tether, TetherBeam[] cutBeams, PressurePlate[] requiredPlates, float height)
    {
        connection = tether;
        beams = cutBeams;
        plates = requiredPlates;
        openHeight = height;
        stayOpen = true;
    }

    void Awake() => closedPosition = transform.position;

    void Update()
    {
        if (vanished)
            return;

        bool met = ConditionsMet();
        if (met && stayOpen)
            lockedOpen = true;

        Vector3 target = (stayOpen ? lockedOpen || met : met)
            ? closedPosition + Vector3.up * openHeight
            : closedPosition;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if (!deactivateWhenOpen || !(stayOpen ? lockedOpen || met : met))
            return;

        if (Vector3.Distance(transform.position, target) > 0.05f)
            return;

        vanished = true;
        gameObject.SetActive(false);
    }

    bool ConditionsMet() => AllPlatesSatisfied() && AllBeamsCut() && LinkOk();

    bool LinkOk() => !requireLinked || (connection != null && connection.IsLinked);

    bool AllPlatesSatisfied()
    {
        if (plates == null || plates.Length == 0)
            return true;
        foreach (PressurePlate plate in plates)
            if (plate == null || !plate.IsSatisfied)
                return false;
        return true;
    }

    bool AllBeamsCut()
    {
        if (beams == null || beams.Length == 0)
            return true;
        foreach (TetherBeam beam in beams)
            if (beam == null || !beam.IsCut)
                return false;
        return true;
    }
}
