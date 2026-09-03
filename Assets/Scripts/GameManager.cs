using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-200)]
[RequireComponent(typeof(PlayerInputSetup))]
[RequireComponent(typeof(PlayerConnection))]
public class GameManager : MonoBehaviour
{
    [SerializeField] InputActionAsset inputActions;
    [SerializeField] PlatformerController player1;
    [SerializeField] PlatformerController player2;

    void Awake()
    {
        // Gameplay cams own the view — disable the default Main Camera
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
