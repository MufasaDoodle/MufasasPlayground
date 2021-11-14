using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class LobbyStateData : MonoBehaviour
{
    public const int k_MaxLobbyPlayers = 8;

    /// <summary>
    /// Describes one of the players in the lobby, and their current character-select status.
    /// </summary>
    public struct LobbyPlayerState : INetworkSerializable, IEquatable<LobbyPlayerState>
    {
        public ulong ClientId;

        private FixedPlayerName m_PlayerName; // I'm sad there's no 256Bytes fixed list :(

        public bool IsReady;


        public LobbyPlayerState(ulong clientId, string name, bool isReady)
        {
            ClientId = clientId;
            IsReady = isReady;
            m_PlayerName = new FixedPlayerName();

            PlayerName = name;
        }

        public string PlayerName
        {
            get => m_PlayerName;
            private set => m_PlayerName = value;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref m_PlayerName);
        }

        public bool Equals(LobbyPlayerState other)
        {
            return ClientId == other.ClientId &&
                   m_PlayerName.Equals(other.m_PlayerName);
        }
    }

    private NetworkList<LobbyPlayerState> m_LobbyPlayers;

    private void Awake()
    {
        m_LobbyPlayers = new NetworkList<LobbyPlayerState>();
    }

    /// <summary>
    /// Current state of all players in the lobby.
    /// </summary>
    public NetworkList<LobbyPlayerState> LobbyPlayers => m_LobbyPlayers;

    /// <summary>
    /// When this becomes true, the lobby is closed and in process of terminating (switching to gameplay).
    /// </summary>
    public NetworkVariable<bool> IsLobbyClosed { get; } = new NetworkVariable<bool>(false);

    /// <summary>
    /// Server notification when a client requests a different lobby-seat, or locks in their seat choice
    /// </summary>
    public event Action<ulong, int, bool> OnClientChangedSeat;

    /// <summary>
    /// RPC to notify the server that a client has chosen a seat.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void ChangeSeatServerRpc(ulong clientId, int seatIdx, bool lockedIn)
    {
        OnClientChangedSeat?.Invoke(clientId, seatIdx, lockedIn);
    }
}
