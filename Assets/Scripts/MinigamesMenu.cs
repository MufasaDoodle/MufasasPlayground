using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using System.Text;
using static Player;
using static LobbyStateData;

public class MinigamesMenu : MonoBehaviour
{
    public InputField ipInputField;
    public InputField portInputField;
    public InputField nameInputfield;

    public GameObject lobbyPrefab;
    public GameObject NetPortalGO;


    private NetPortal netPortal;
    private ClientNetPortal clientNetPortal;

    const string playerNamePrefKey = "PlayerName";

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        string defaultName = "defaultName";
        netPortal = NetPortalGO.GetComponent<NetPortal>();
        clientNetPortal = NetPortalGO.GetComponent<ClientNetPortal>();

        clientNetPortal.NetworkTimedOut += OnNetworkTimeout;
        clientNetPortal.ConnectFinished += OnConnectFinished;

        //any disconnect reason set? Show it to the user here.
        ConnectStatusToMessage(clientNetPortal.DisconnectReason.Reason, false);
        clientNetPortal.DisconnectReason.Clear();

        if (PlayerPrefs.HasKey(playerNamePrefKey))
        {
            defaultName = PlayerPrefs.GetString(playerNamePrefKey);
            nameInputfield.text = defaultName;
        }
    }

    public void HostServer()
    {
        PlayerPrefs.SetString(playerNamePrefKey, nameInputfield.text);
        NetPortal.Instance.PlayerName = nameInputfield.text;

        if (portInputField.text == "")
        {
            portInputField.text = "7777";
        }

        int port;

        if (!int.TryParse(portInputField.text, out port))
        {
            throw new System.Exception("Invalid port number");
        }


        NetPortal.Instance.StartHost(ipInputField.text, port);
        NetworkManager.Singleton.SceneManager.LoadScene("MinigamesLobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void JoinGame()
    {
        PlayerPrefs.SetString(playerNamePrefKey, nameInputfield.text);
        NetPortal.Instance.PlayerName = nameInputfield.text;

        if (ipInputField.text == "")
        {
            ipInputField.text = "127.0.0.1";
        }

        if (portInputField.text == "")
        {
            portInputField.text = "7777";
        }

        int port;

        if (!int.TryParse(portInputField.text, out port))
        {
            throw new System.Exception("Invalid port number");
        }


        ClientNetPortal.StartClient(netPortal, ipInputField.text, port);
    }

    /// <summary>
    /// Callback when the server sends us back a connection finished event.
    /// </summary>
    /// <param name="status"></param>
    private void OnConnectFinished(ConnectStatus status)
    {
        ConnectStatusToMessage(status, true);
                
    }

    /// <summary>
    /// Takes a ConnectStatus and shows an appropriate message to the user. This can be called on: (1) successful connect,
    /// (2) failed connect, (3) disconnect.
    /// </summary>
    /// <param name="connecting">pass true if this is being called in response to a connect finishing.</param>
    private void ConnectStatusToMessage(ConnectStatus status, bool connecting)
    {
        string toPrint = "";

        switch (status)
        {
            case ConnectStatus.Undefined:
            case ConnectStatus.UserRequestedDisconnect:
                break;
            case ConnectStatus.ServerFull:
                toPrint = "Connection failed. Lobby is full";
                break;
            case ConnectStatus.Success:
                if (connecting) toPrint = "Connection successful, joining.";
                break;
            case ConnectStatus.LoggedInAgain:
                toPrint = "Connection failed. Duplicate login detected.";
                break;
            case ConnectStatus.GenericDisconnect:
                toPrint = connecting ? "Connection Failed" : "Disconnected From Host";
                toPrint += connecting ? "Something went wrong" : "The connection to the host was lost";
                break;
            default:
                Debug.LogWarning($"New ConnectStatus {status} has been added, but no connect message defined for it.");
                break;
        }

        if(toPrint != "") Debug.Log(toPrint);
    }

    /// <summary>
    /// Invoked when the client sent a connection request to the server and didn't hear back at all.
    /// This should create a UI letting the player know that something went wrong and to try again
    /// </summary>
    private void OnNetworkTimeout()
    {
        Debug.LogError("Connection failed, unable to reach host/server");
    }
}
