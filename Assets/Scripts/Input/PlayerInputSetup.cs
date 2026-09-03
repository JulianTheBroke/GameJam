using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class PlayerInputSetup : MonoBehaviour
{
    readonly List<InputUser> users = new List<InputUser>();

    public void Initialize(InputActionAsset actions, PlatformerController player1, PlatformerController player2)
    {
        player1.Setup("Player1", actions);
        player2.Setup("Player2", actions);

        InputUser user1 = InputUser.CreateUserWithoutPairedDevices();
        InputUser user2 = InputUser.CreateUserWithoutPairedDevices();
        users.Add(user1);
        users.Add(user2);
        user1.AssociateActionsWithUser(player1.InputActions);
        user2.AssociateActionsWithUser(player2.InputActions);

        // two pads, or pad + keys, or shared keyboard
        Keyboard keyboard = Keyboard.current;
        List<Gamepad> pads = new List<Gamepad>(Gamepad.all);

        if (pads.Count >= 2)
        {
            InputUser.PerformPairingWithDevice(pads[0], user1);
            InputUser.PerformPairingWithDevice(pads[1], user2);
        }
        else if (pads.Count == 1)
        {
            InputUser.PerformPairingWithDevice(pads[0], user1);
            if (keyboard != null)
                InputUser.PerformPairingWithDevice(keyboard, user2);
        }
        else if (keyboard != null)
        {
            InputUser.PerformPairingWithDevice(keyboard, user1);
            InputUser.PerformPairingWithDevice(keyboard, user2);
        }
    }

    void OnDestroy()
    {
        foreach (InputUser user in users)
            user.UnpairDevicesAndRemoveUser();
    }
}
