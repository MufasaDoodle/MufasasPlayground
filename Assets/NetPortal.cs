using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ConnectStatus
{
    Undefined,
    Success,                  //client successfully connected. This may also be a successful reconnect.
    ServerFull,               //can't join, server is already at capacity.
    LoggedInAgain,            //logged in on a separate client, causing this one to be kicked out.
    UserRequestedDisconnect,  //Intentional Disconnect triggered by the user.
    GenericDisconnect,        //server disconnected, but no specific reason given.
}

[Serializable]
public class ConnectionPayload
{
    public string clientGUID;
    public int clientScene = -1;
    public string playerName;
}


public class NetPortal : MonoBehaviour
{
    [SerializeField]
    NetworkManager m_NetworkManager;

    public NetworkManager NetManager => m_NetworkManager;

    /// <summary>
    /// the name of the player chosen at game start
    /// </summary>
    public string PlayerName;

    // Instance of GameNetPortal placed in scene. There should only be one at once
    public static NetPortal Instance;
    private ClientNetPortal m_ClientPortal;
    private ServerNetPortal m_ServerPortal;

    private void Awake()
    {
        Debug.Assert(Instance == null);
        Instance = this;
        m_ClientPortal = GetComponent<ClientNetPortal>();
        m_ServerPortal = GetComponent<ServerNetPortal>();
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        //we synthesize a "OnNetworkSpawn" event for the NetworkManager out of existing events. At some point
        //we expect NetworkManager will expose an event like this itself.
        NetManager.OnServerStarted += OnNetworkReady;
        NetManager.OnClientConnectedCallback += ClientNetworkReadyWrapper;
    }

    private void OnSceneEvent(SceneEvent sceneEvent)
    {
        // only processing single player finishing loading events
        if (sceneEvent.SceneEventType != SceneEventType.LoadComplete) return;

        m_ServerPortal.OnClientSceneChanged(sceneEvent.ClientId, SceneManager.GetSceneByName(sceneEvent.SceneName).buildIndex);
    }

    private void OnDestroy()
    {
        if (NetManager != null)
        {
            NetManager.OnServerStarted -= OnNetworkReady;
            NetManager.OnClientConnectedCallback -= ClientNetworkReadyWrapper;
        }

        Instance = null;
    }

    private void ClientNetworkReadyWrapper(ulong clientId)
    {
        if (clientId == NetManager.LocalClientId)
        {
            OnNetworkReady();
            NetManager.SceneManager.OnSceneEvent += OnSceneEvent;
        }
    }

    /// <summary>
    /// This method runs when NetworkManager has started up (following a succesful connect on the client, or directly after StartHost is invoked
    /// on the host). It is named to match NetworkBehaviour.OnNetworkSpawn, and serves the same role, even though GameNetPortal itself isn't a NetworkBehaviour.
    /// </summary>
    private void OnNetworkReady()
    {
        if (NetManager.IsHost)
        {
            //special host code. This is what kicks off the flow that happens on a regular client
            //when it has finished connecting successfully. A dedicated server would remove this.
            m_ClientPortal.OnConnectFinished(ConnectStatus.Success);
        }

        m_ClientPortal.OnNetworkReady();
        m_ServerPortal.OnNetworkReady();
    }

    /// <summary>
    /// Initializes host mode on this client. Call this and then other clients should connect to us!
    /// </summary>
    /// <remarks>
    /// See notes in GNH_Client.StartClient about why this must be static.
    /// </remarks>
    public void StartHost(string ipaddress, int port)
    {
        var chosenTransport = NetworkManager.Singleton.NetworkConfig.NetworkTransport;

        if (!(chosenTransport is UNetTransport))
        {
            Debug.Log("Invalid Net Transport");
        }

        UNetTransport transport = (UNetTransport)chosenTransport;

        transport.ConnectAddress = ipaddress;
        transport.ConnectPort = port;

        NetManager.StartHost();
    }

    /// <summary>
    /// This will disconnect (on the client) or shutdown the server (on the host).
    /// It's a local signal (not from the network), indicating that the user has requested a disconnect.
    /// </summary>
    public void RequestDisconnect()
    {
        m_ClientPortal.OnUserDisconnectRequest();
        m_ServerPortal.OnUserDisconnectRequest();
        NetManager.Shutdown();
    }
}
