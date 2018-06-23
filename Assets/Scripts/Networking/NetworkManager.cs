using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static string gameVersion = "0.0.5";
    public Camera worldCamera;

    private SpawnSpotScript[] spawns;
    // Use this for initialization
    void Start()
    {
        spawns = FindObjectsOfType<SpawnSpotScript>();
        Connect();
    }

    /// <summary>
    /// Connects to the server using the settings set up in the Photon
    /// Server Settings object.
    /// </summary>
    private void Connect()
    {
        PhotonNetwork.ConnectUsingSettings("v" + gameVersion);
    }

    private void OnGUI()
    {
        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
    }

    private void OnJoinedLobby()
    {
        Debug.Log("Attempting to join a room...");
        PhotonNetwork.JoinRandomRoom();
    }

    private void OnPhotonRandomJoinFailed()
    {
        Debug.Log("Failed to join random room!");
        Debug.Log("Creating new room instead...");
        PhotonNetwork.CreateRoom("Arena1");
    }

    private void OnJoinedRoom()
    {
        Debug.Log("Joined the room!");

        SpawnAPlayer();
    }

    /// <summary>
    /// Method to spawn in a player through the network.
    /// </summary>
    public void SpawnAPlayer()
    {
        if(spawns == null)
        {
            Debug.LogError("Spawns array empty.");
            return;
        }
        SpawnSpotScript aSpawn = spawns[Random.Range(0, spawns.Length)];
        //disables world camera
        worldCamera.gameObject.SetActive(false);
        //prefabs must be in a resource folder
        PhotonNetwork.Instantiate("BritishRifleman", aSpawn.transform.position, aSpawn.transform.rotation, (byte)aSpawn.team); //wrong maybe?

    }
}
