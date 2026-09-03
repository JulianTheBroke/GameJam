using System;
using UnityEngine;

public class CoopDoor : MonoBehaviour
{
    [SerializeField] private PressurePlate[] plates;
    [SerializeField] private TetherBeam[] beams;
    [SerializeField] private PlayerConnection connection;
    [SerializeField] private bool requireLinked;
    [SerializeField] private bool stayOpen = true;
    [SerializeField] private float openHeight = 3.6f;
    [SerializeField] private float moveSpeed = 4f;

    private Vector3 closedPosition;
    private bool lockedOpen;

    public bool IsOpen => lockedOpen || ConditionsMet();
    public event Action Opened;

    public void Configure(PlayerConnection tether, bool mustBeLinked, PressurePlate[] requiredPlates, TetherBeam[] requiredBeams, bool remainOpen)
    {
        connection = tether;
        requireLinked = mustBeLinked;
        plates = requiredPlates ?? Array.Empty<PressurePlate>();
        beams = requiredBeams ?? Array.Empty<TetherBeam>();
        stayOpen = remainOpen;
    }

    void Awake() => closedPosition = transform.position;

    void Update()
    {
        bool met = ConditionsMet();
        if (met && !lockedOpen)
        {
            lockedOpen = stayOpen;
            Opened?.Invoke();
        }

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
