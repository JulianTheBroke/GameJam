using UnityEngine;

public class CoopDoor : MonoBehaviour
{
    [SerializeField] private PressurePlate[] plates;
    [SerializeField] private PlayerConnection connection;
    [SerializeField] private bool requireLinked;
    [SerializeField] private float openHeight = 3f;
    [SerializeField] private float moveSpeed = 4f;

    private Vector3 closedPosition;

    void Awake() => closedPosition = transform.position;

    void Update()
    {
        bool platesHeld = AllPlatesPressed();
        bool linkOk = !requireLinked || (connection != null && connection.IsLinked);
        Vector3 target = platesHeld && linkOk ? closedPosition + Vector3.up * openHeight : closedPosition;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }

    bool AllPlatesPressed()
    {
        if (plates == null || plates.Length == 0)
            return false;

        foreach (PressurePlate plate in plates)
        {
            if (plate == null || !plate.IsPressed)
                return false;
        }

        return true;
    }
}
