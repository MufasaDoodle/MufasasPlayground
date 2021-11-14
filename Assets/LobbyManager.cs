using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static LobbyStateData;

public class LobbyManager : NetworkBehaviour
{

    [SerializeField] private PlayerCard[] lobbyPlayerSlots;
    [SerializeField] private Button startGameButton;

    [SerializeField]
    private NetworkList<LobbyPlayerState> lobbyPlayers;

    private void Awake()
    {
        lobbyPlayers = new NetworkList<LobbyPlayerState>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            Debug.Log("Handling connected client");
            lobbyPlayers.OnListChanged += HandleLobbyPlayersStateChanged;
        }

        if (IsServer || IsHost)
        {
            startGameButton.gameObject.SetActive(true);

            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            foreach (NetworkClient networkClient in NetworkManager.Singleton.ConnectedClientsList)
            {
                Debug.Log("Handling connected client");
                HandleClientConnected(networkClient.ClientId);
            }
        }
    }

    private void OnDestroy()
    {
        lobbyPlayers.OnListChanged -= HandleLobbyPlayersStateChanged;


        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    private bool IsEveryoneReady()
    {
        if (lobbyPlayers.Count < 1)
        {
            return false;
        }

        foreach (var lobbyPlayer in lobbyPlayers)
        {
            if (!lobbyPlayer.IsReady)
            {
                return false;
            }
        }

        return true;
    }

    private void HandleClientConnected(ulong clientId)
    {
        PlayerData? playerData = ServerNetPortal.Instance.GetPlayerData(clientId);

        if (!playerData.HasValue)
        {
            return;
        }
        Debug.Log("Client Added");
        lobbyPlayers.Add(new LobbyPlayerState(clientId, playerData.Value.m_PlayerName, false));
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        for (int i = 0; i < lobbyPlayers.Count; i++)
        {
            if (lobbyPlayers[i].ClientId == clientId)
            {
                lobbyPlayers.RemoveAt(i);
                break;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        for (int i = 0; i < lobbyPlayers.Count; i++)
        {
            if (lobbyPlayers[i].ClientId == serverRpcParams.Receive.SenderClientId)
            {
                lobbyPlayers[i] = new LobbyPlayerState(lobbyPlayers[i].ClientId, lobbyPlayers[i].PlayerName, !lobbyPlayers[i].IsReady);
                ToggleReadyClientRpc(i);
            }
        }
    }

    [ClientRpc]
    private void ToggleReadyClientRpc(int i)
    {
        lobbyPlayerSlots[i].UpdateDisplay(lobbyPlayers[i]);
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (serverRpcParams.Receive.SenderClientId != NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        if (!IsEveryoneReady())
        {
            //return;
            //don't do anything here until we figure out how to make it work
        }

        //how do we start the game?
        NetworkManager.SceneManager.LoadScene("MinigamesMain", UnityEngine.SceneManagement.LoadSceneMode.Single);
        //NetPortal.Instance.StartGame();
    }

    public void OnLeaveClicked()
    {
        NetPortal.Instance.RequestDisconnect();
    }

    public void OnReadyClicked()
    {
        ToggleReadyServerRpc();


    }

    public void OnStartGameClicked()
    {
        StartGameServerRpc();
    }

    private void HandleLobbyPlayersStateChanged(NetworkListEvent<LobbyPlayerState> lobbyState)
    {
        for (int i = 0; i < lobbyPlayerSlots.Length; i++)
        {
            if (lobbyPlayers.Count > i)
            {
                lobbyPlayerSlots[i].UpdateDisplay(lobbyPlayers[i]);
            }
            else
            {
                lobbyPlayerSlots[i].DisableDisplay();
            }
        }

        if (IsHost)
        {
            //startGameButton.interactable = IsEveryoneReady();
        }
    }
}
