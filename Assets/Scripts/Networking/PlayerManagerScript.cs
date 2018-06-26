using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Invector.CharacterController;

public class PlayerManagerScript : NetworkBehaviour
{
    private static Dictionary<string, vThirdPersonController> players = new Dictionary<string, vThirdPersonController>();

    public static void RegisterPlayer(string _netID, vThirdPersonController player)
    {
        string _playerID = "Player " + _netID;
        players.Add(_playerID, player);
        player.gameObject.name = _playerID;
    }

    public static void UnregisterPlayer(string playerID)
    {
        players.Remove(playerID);
    }
}
