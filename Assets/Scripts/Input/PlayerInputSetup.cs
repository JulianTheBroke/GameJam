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

        // Only lock devices when two pads need exclusive ownership.
        // Keyboard bindings are already split (WASD vs arrows), so leave them unbound.
        if (Gamepad.all.Count < 2)
            return;

        InputUser user1 = InputUser.CreateUserWithoutPairedDevices();
        InputUser user2 = InputUser.CreateUserWithoutPairedDevices();
        users.Add(user1);
        users.Add(user2);
        user1.AssociateActionsWithUser(player1.InputActions);
        user2.AssociateActionsWithUser(player2.InputActions);
        InputUser.PerformPairingWithDevice(Gamepad.all[0], user1);
        InputUser.PerformPairingWithDevice(Gamepad.all[1], user2);
    }

    void OnDestroy()
    {
        foreach (InputUser user in users)
            user.UnpairDevicesAndRemoveUser();
    }
}
