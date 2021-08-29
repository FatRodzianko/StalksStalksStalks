using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using System;

public class StalkOwnedListItem : NetworkBehaviour
{
    [SyncVar] public string ownerName;
    [SyncVar] public ulong ownerSteamId;
    [SyncVar] public string stalkName;
    [SyncVar(hook = nameof(UpdateTotalStalksOwned))] public int totalStalksOwned;
    public GamePlayer ownerScript;

    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI StalkNameText;
    [SerializeField] private TextMeshProUGUI NumberOwnedText;
    [SerializeField] private TMP_InputField SellInputField;
    [SerializeField] private Button SellButton;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        GameplayManager.instance.AddNewStalkOwnedListItem(this.gameObject);
        SetStalkOwnerInfo();
        if(isClient)
            FindIfOwnerIsLocalGamePlayer();
    }
    void SetStalkOwnerInfo()
    {
        StalkNameText.text = stalkName;
        NumberOwnedText.text = totalStalksOwned.ToString("N0");
    }
    [Client]
    void FindIfOwnerIsLocalGamePlayer()
    {
        GamePlayer localPlayer = GameObject.FindGameObjectWithTag("LocalGamePlayer").GetComponent<GamePlayer>();
        if (ownerName == localPlayer.PlayerName && ownerSteamId == localPlayer.playerSteamId)
        {
            Debug.Log("FindIfOwnerIsLocalGamePlayer: The local player " + localPlayer.PlayerName + " is the owner of this StalkOwnedListItem object.");
            ownerScript = localPlayer;
        }
        else
            ownerScript = null;
    }
    public void UpdateTotalStalksOwned(int oldValue, int newValue)
    {
        if (isServer)
            totalStalksOwned = newValue;
        if (isClient && this.gameObject.activeInHierarchy)
        {
            SetTotalStalksOwned(newValue);
        }
    }
    void SetTotalStalksOwned(int newValue)
    {
        Debug.Log("SetTotalStalksOwned: setting total stalks to " + newValue.ToString());
        NumberOwnedText.text = newValue.ToString("N0");
    }
    public void SellStalk()
    {
        Debug.Log("SellStalk on StalkOwnedListItem");
        if (ownerScript != null)
        {
            int amountToSell = 0;
            Int32.TryParse(SellInputField.text, out amountToSell);
            Debug.Log("SellStalk: player is trying to Sell " + amountToSell.ToString() + " shares of " + stalkName);
            if (amountToSell > 0)
            {
                ownerScript.PlayerSellStalks(this.stalkName, amountToSell);
            }
        }
    }
}
