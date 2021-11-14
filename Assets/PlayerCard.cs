using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static LobbyStateData;

public class PlayerCard : MonoBehaviour
{
    [Header("Panels")]
    //[SerializeField] private GameObject waitingForPlayerPanel;
    [SerializeField] private GameObject playerDataPanel;

    [Header("Data Display")]
    [SerializeField] private Text playerDisplayNameText;
    [SerializeField] private Toggle isReadyToggle;

    public void UpdateDisplay(LobbyPlayerState lobbyPlayerState)
    {
        playerDisplayNameText.text = lobbyPlayerState.PlayerName;

        isReadyToggle.gameObject.SetActive(true);
        
        isReadyToggle.isOn = lobbyPlayerState.IsReady;

        Debug.Log($"Updating display for {lobbyPlayerState.ClientId}");

        //waitingForPlayerPanel.SetActive(false);
        //playerDataPanel.SetActive(true);
    }

    public void DisableDisplay()
    {
        //waitingForPlayerPanel.SetActive(true);
        //playerDataPanel.SetActive(false);
    }    

    /// <summary>
    /// Wrapping FixedString so that if we want to change player name max size in the future, we only do it once here
    /// </summary>
    public struct FixedPlayerName : INetworkSerializable
    {
        private FixedString32Bytes m_Name;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref m_Name);
        }

        public override string ToString()
        {
            return m_Name.ToString();
        }

        public static implicit operator string(FixedPlayerName s) => s.ToString();
        public static implicit operator FixedPlayerName(string s) => new FixedPlayerName() { m_Name = new FixedString32Bytes(s) };
    }
}

