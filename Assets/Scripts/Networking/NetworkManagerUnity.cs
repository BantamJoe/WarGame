using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using Invector.CharacterController;

public class NetworkManagerUnity : NetworkManager
{
    public static NetworkManagerUnity instance;

    public GameObject worldCamera;
    public MatchSettings matchSettings;

    private static SpawnSpotScript[] spawns;
    private static Dictionary<string, vThirdPersonController> players = new Dictionary<string, vThirdPersonController>();

    private void Awake()
    {
        if(instance != null)
        {
            Debug.LogError("More than one NetworkManagerUnity in scene");
        }
        else
        {
            instance = this;
        }
    }

    // Use this for initialization
    private void Start()
    {
        //some unexposed network settings
        /*customConfig = true;
        connectionConfig.MaxCombinedReliableMessageCount = 40;
        connectionConfig.MaxCombinedReliableMessageSize = 800;
        connectionConfig.MaxSentMessageQueueSize = 2048;
        connectionConfig.IsAcksLong = true;
        globalConfig.ThreadAwakeTimeout = 1;*/
        spawns = FindObjectsOfType<SpawnSpotScript>(); //might need to change this so when the scene loads for the map, the spawns are grabbed
    }


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

    public Transform GetASpawnPoint()
    {
        return spawns[Random.Range(0, spawns.Length)].transform;
    }

    #region Server Callbacks
    public override void OnServerConnect(NetworkConnection conn)
    {
        Debug.Log("A client connected to the server: " + conn);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        NetworkServer.DestroyPlayersForConnection(conn);
        if (conn.lastError != NetworkError.Ok)
        {
            if (LogFilter.logError) { Debug.LogError("ServerDisconnected due to error: " + conn.lastError); }
        }
        Debug.Log("A client disconnected from the server: " + conn);
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        NetworkServer.SetClientReady(conn);
        Debug.Log("Client is set to the ready state (ready to receive state updates): " + conn);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        SpawnSpotScript spawn = spawns[Random.Range(0, spawns.Length)];
        GameObject myPlayer = Instantiate(playerPrefab, spawn.transform.position, spawn.transform.rotation);

        NetworkServer.AddPlayerForConnection(conn, myPlayer, playerControllerId);
        Debug.Log("Client has requested to get his player added to the game");
    }

    //this should be overriddeable but is definitely not for some reason?
    public void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
    {
        if (player.gameObject != null)
        {
            NetworkServer.Destroy(player.gameObject);
        }
    }

    public override void OnServerError(NetworkConnection conn, int errorCode)
    {
        Debug.Log("Server network error occurred: " + (NetworkError)errorCode);
    }

    public override void OnStartHost()
    {
        Debug.Log("Host has started");
    }

    public override void OnStartServer()
    {
        Debug.Log("Server has started");
    }

    public override void OnStopServer()
    {
        Debug.Log("Server has stopped");
    }

    public override void OnStopHost()
    {
        Debug.Log("Host has stopped");
    }
    #endregion

    #region Client Callbacks
    // Client callbacks
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.Log("Connected successfully to server, now to set up other stuff for the client...");
        worldCamera.SetActive(false);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        StopClient();
        if (conn.lastError != NetworkError.Ok)
        {
            if (LogFilter.logError)
            {
                Debug.LogError("ClientDisconnected due to error: " + conn.lastError);
            }
        }
        Debug.Log("Client disconnected from server: " + conn);
        //NetworkManagerUnity.UnregisterPlayer()
    }

    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        Debug.Log("Client network error occurred: " + (NetworkError)errorCode);
    }

    public override void OnClientNotReady(NetworkConnection conn)
    {
        Debug.Log("Server has set client to be not-ready (stop getting state updates)");
    }

    public override void OnStartClient(NetworkClient client)
    {
        Debug.Log("Client has started");
    }

    public override void OnStopClient()
    {
        Debug.Log("Client has stopped");
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        base.OnClientSceneChanged(conn);
        Debug.Log("Server triggered scene change and we've done the same, do any extra work here for the client...");
    }
    #endregion
}
