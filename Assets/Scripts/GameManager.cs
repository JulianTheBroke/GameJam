using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputSetup))]
[RequireComponent(typeof(PlayerConnection))]
public class GameManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private PlatformerController player1;
    [SerializeField] private PlatformerController player2;

    void Awake()
    {
        Camera main = Camera.main;
        if (main != null)
        {
            main.enabled = false;
            main.gameObject.SetActive(false);
        }

        PlayerConnection connection = GetComponent<PlayerConnection>();
        player1.SetConnection(connection);
        player2.SetConnection(connection);
        GetComponent<PlayerInputSetup>().Initialize(inputActions, player1, player2);
    }
}
