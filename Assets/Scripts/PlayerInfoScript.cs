using Invector.CharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(vThirdPersonController))]
public class PlayerInfoScript : NetworkBehaviour
{
    private string ID;

    // Use this for initialization
    void Start()
    {
        RegisterPlayer();
    }

    private void RegisterPlayer()
    {
        ID = "Player " + GetComponent<NetworkIdentity>().netId;
        this.gameObject.name = ID;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        string netId = GetComponent<NetworkIdentity>().netId.ToString();
        vThirdPersonController player = GetComponent<vThirdPersonController>();
        PlayerManagerScript.RegisterPlayer(netId, player);
    }
}
