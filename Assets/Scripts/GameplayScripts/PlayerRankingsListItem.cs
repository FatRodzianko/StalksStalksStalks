using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System;

public class PlayerRankingsListItem : NetworkBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI PlayerNameText;
    [SerializeField] private TextMeshProUGUI PlayerNetworthText;
    [SerializeField] private Image playerReadyStatusImage;
    [SerializeField] private Sprite readyGreenCircle;
    [SerializeField] private Sprite readyRedCircle;

    [Header("Player Info")]
    [SyncVar] public string playerName;
    [SyncVar(hook = nameof(UpdatePlayerNetWorth))] public int playerNetworth;
    [SyncVar(hook = nameof(UpdatePlayerReadyStatus))] public bool playerReadyStatus;
    [SyncVar] public ulong playerSteamId;
    [SyncVar] public uint ownerNetId;
    public GamePlayer myLocalGamePlayerScript;

    // Start is called before the first frame update
    void Start()
    {
        //GameplayManager.instance.AddPlayerToRankingsList(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void OnStartClient()
    {
        Debug.Log("OnStartClient for PlayerRankingsListItem for player " + this.playerName);
        base.OnStartClient();
        GameplayManager.instance.AddPlayerToRankingsList(this.gameObject);
        FindLocalPlayerScript(ownerNetId);
        SetPlayerInfo();
    }
    public void SetPlayerInfo()
    {
        Debug.Log("SetPlayerInfo on PlayerRankingsListItem for player " + this.playerName);
        PlayerNameText.text = playerName;
        PlayerNetworthText.text = playerNetworth.ToString("N0");
        if (playerReadyStatus)
            playerReadyStatusImage.sprite = readyGreenCircle;
        else
            playerReadyStatusImage.sprite = readyRedCircle;
    }
    public void SetNetWorth(int netWorth)
    {
        Debug.Log("SetNetWorth to " + netWorth.ToString());
        PlayerNetworthText.text = playerNetworth.ToString("N0");
        GameplayManager.instance.SortPlayerRankings();
    }
    public void UpdateReadyImage(bool newReadyStatus)
    {
        Debug.Log("UpdateReadyImage: updating ready status to " + newReadyStatus.ToString() + " for player " + playerName);
        playerReadyStatus = newReadyStatus;
        if (newReadyStatus)
        {
            playerReadyStatusImage.sprite = readyGreenCircle;
        }
        else
        {
            playerReadyStatusImage.sprite = readyRedCircle;
        }
    }
    void FindLocalPlayerScript(uint playerId)
    {
        myLocalGamePlayerScript = NetworkIdentity.spawned[playerId].gameObject.GetComponent<GamePlayer>();
        myLocalGamePlayerScript.myPlayerRankingsListItemObject = this.gameObject;
        myLocalGamePlayerScript.myPlayerRankingsListItemScript = this;
    }
    [Command]
    public void CmdUpdateNetWorth(int newNetWorth)
    {
        this.UpdatePlayerNetWorth(this.playerNetworth, newNetWorth);
    }
    public void UpdatePlayerNetWorth(int oldValue, int newValue)
    {
        if (isServer)
        {
            playerNetworth = newValue;
        }
        if (isClient)
        {
            SetNetWorth(newValue);
        }
    }
    [Command]
    public void CmdUpdateReadyStatus(bool newReadyStatus)
    {
        this.UpdatePlayerReadyStatus(this.playerReadyStatus, newReadyStatus);
    }
    public void UpdatePlayerReadyStatus(bool oldValue, bool newValue)
    {
        if (isServer)
            playerReadyStatus = newValue;
        if (isClient)
        {
            UpdateReadyImage(newValue);
        }
    }
    private void OnDestroy()
    {
        try
        {
            GameplayManager.instance.RemovePlayerToRankingsList(this);
        }
        catch (Exception e)
        {
            Debug.Log("PlayerRankingsListItem could not do GameplayManager.instance.RemovePlayerToRankingsList(this) in onDestroy " + e);
        }
    }
}
