using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Linq;
using Steamworks;

public class LobbyPlayer : NetworkBehaviour
{
    [Header("GamePlayer Info")]
    [SyncVar(hook = nameof(HandlePlayerNameUpdate))] public string PlayerName;
    [SyncVar] public int ConnectionId;
    [SyncVar] public int playerNumber;
    [SyncVar] public ulong playerSteamId;

    [Header("Game Info")]
    [SyncVar] public bool IsGameLeader = false;
    [SyncVar(hook = nameof(HandlePlayerReadyStatusChange))] public bool isPlayerReady;
    public PlayerListItem myPlayerListItem;

    private NetworkManagerStalks game;
    private NetworkManagerStalks Game
    {
        get
        {
            if (game != null)
            {
                return game;
            }
            return game = NetworkManagerStalks.singleton as NetworkManagerStalks;
        }
    }

    public override void OnStartAuthority()
    {
        CmdSetPlayerName(SteamFriends.GetPersonaName().ToString());
        gameObject.name = "LocalLobbyPlayer";
        LobbyManager.instance.FindLocalLobbyPlayer();
        LobbyManager.instance.UpdateLobbyName();
    }
    [Command]
    private void CmdSetPlayerName(string PlayerNameSubmitted)
    {
        Debug.Log("CmdSetPlayerName: Setting player name to: " + PlayerNameSubmitted);
        string playerNameToSet = "";
        if (PlayerNameSubmitted.Length > 12)
            playerNameToSet = PlayerNameSubmitted.Substring(0, 12);
        else
            playerNameToSet = PlayerNameSubmitted;
        this.HandlePlayerNameUpdate(this.PlayerName, playerNameToSet);
    }
    public void HandlePlayerNameUpdate(string oldValue, string newValue)
    {
        Debug.Log("Player name has been updated for: " + oldValue + " to new value: " + newValue);
        if (isServer)
            this.PlayerName = newValue;
        if (isClient)
        {
            LobbyManager.instance.UpdateUI();
        }

    }
    public override void OnStartClient()
    {
        Game.LobbyPlayers.Add(this);
        LobbyManager.instance.UpdateLobbyName();
        LobbyManager.instance.UpdateUI();
    }
    public void ChangeReadyStatus()
    {
        Debug.Log("Executing ChangeReadyStatus for player: " + this.PlayerName);
        if (hasAuthority)
            CmdChangePlayerReadyStatus();
    }
    [Command]
    void CmdChangePlayerReadyStatus()
    {
        Debug.Log("Executing CmdChangePlayerReadyStatus on the server for player: " + this.PlayerName);
        this.HandlePlayerReadyStatusChange(this.isPlayerReady, !this.isPlayerReady);
    }
    void HandlePlayerReadyStatusChange(bool oldValue, bool newValue)
    {
        if (isServer)
            this.isPlayerReady = newValue;
        if (isClient)
            LobbyManager.instance.UpdateUI();
    }
    public void CanLobbyStartGame()
    {
        if (hasAuthority)
            CmdCanLobbyStartGame();
    }
    [Command]
    void CmdCanLobbyStartGame()
    {
        Game.StartGame();
    }
    public void QuitLobby()
    {
        if (hasAuthority)
        {
            if (IsGameLeader)
            {
                Game.StopHost();
            }
            else
            {
                Game.StopClient();
            }
        }
    }
    private void OnDestroy()
    {
        if (hasAuthority)
        {
            LobbyManager.instance.DestroyPlayerListItems();
            SteamMatchmaking.LeaveLobby((CSteamID)LobbyManager.instance.currentLobbyId);
        }
        Debug.Log("LobbyPlayer destroyed. Returning to main menu.");
    }
    public override void OnStopClient()
    {
        Debug.Log(PlayerName + " is quiting the game.");
        Game.LobbyPlayers.Remove(this);
        Debug.Log("Removed player from the GamePlayer list: " + this.PlayerName);
        LobbyManager.instance.UpdateUI();
    }
}
