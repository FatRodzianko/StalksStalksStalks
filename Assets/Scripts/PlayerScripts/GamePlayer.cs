using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using Steamworks;
using System;


public class GamePlayer : NetworkBehaviour
{
    [Header("Player Info")]
    [SyncVar] public string PlayerName;
    [SyncVar] public int ConnectionId;
    [SyncVar] public int playerNumber;
    [SyncVar] public ulong playerSteamId;
    public ulong currentLobbyId;
    [SyncVar(hook = nameof(HandleDoesPlayerHaveLoan))] public bool doesPlayerHaveLoan;
    [SyncVar(hook = nameof(HandleIsPlayerBankrupt))] public bool isPlayerBankrupt;
    [SyncVar] public bool isLoanPaidOff;

    [Header("Player Cash and Networth")]
    [SyncVar(hook = nameof(UpdateCashOnHandText))] public int cashOnHand;
    [SyncVar(hook = nameof(UpdateNetworthText))] public int netWorth;
    public GameObject[] stalkListItems;

    [Header("Player Stalks")]
    public SyncDictionary<string, int> stalksOwned = new SyncDictionary<string, int>();

    [Header("PlayerRankingsListItem stuff")]
    [SerializeField] private GameObject PlayerRankingsListItemPrefab;
    public GameObject myPlayerRankingsListItemObject;
    public PlayerRankingsListItem myPlayerRankingsListItemScript;

    [Header("Player Statuses")]
    [SyncVar(hook = nameof(UpdatePlayerReadyStatus))] public bool isPlayerReady;

    [Header("Player Loan Info")]
    [SerializeField] private GameObject playerLoanPrefab;
    public GameObject myLoanObject;
    public Loan myLoanScript;
    [SyncVar(hook = nameof(UpdateMaxLoanAmount))] public int maxLoanAmount;
    [SyncVar(hook = nameof(UpdateMinPaymentAmount))] public int minPaymentAmount;

    public IEnumerator errorMessageRoutine;

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
        Debug.Log("OnStartAuthority for: " + this.PlayerName);
        gameObject.name = "LocalGamePlayer";
        gameObject.tag = "LocalGamePlayer";
        Debug.Log("Labeling the local player: " + this.PlayerName + " " + this.name);
        currentLobbyId = SteamLobby.instance.current_lobbyID;
        //CmdGetPlayerStartingCashAndNetworth();
        //CmdSpawnPlayerRankListItem();
    }
    public override void OnStartClient()
    {
        Debug.Log("OnStartClient for: " + this.PlayerName);
        DontDestroyOnLoad(gameObject);
        Game.GamePlayers.Add(this);
        Debug.Log("Added to GamePlayer list: " + this.PlayerName);
        //CmdCheckIfPlayerIsMissingFromRankList();
        if(netWorth > 0)
            GameplayManager.instance.UpdateLocalPlayersNetworth(netWorth);
        if (cashOnHand > 0)
            GameplayManager.instance.UpdateLocalPlayersCashOnHand(cashOnHand);
    }
    public override void OnStopClient()
    {
        Debug.Log(PlayerName + " is quiting the game.");
        Game.GamePlayers.Remove(this);
        Debug.Log("Removed player from the GamePlayer list: " + this.PlayerName);
    }
    private void Start()
    {
        
    }
    [Server]
    public void SetPlayerName(string playerName)
    {
        this.PlayerName = playerName;
    }
    [Server]
    public void SetConnectionId(int connId)
    {
        this.ConnectionId = connId;
    }
    [Server]
    public void SetPlayerNumber(int playerNum)
    {
        this.playerNumber = playerNum;
    }
    [Command]
    public void CmdGetPlayerStartingCashAndNetworth()
    {
        Debug.Log("executing GetPlayerStartingCashAndNetworth on the server for " + this.PlayerName);
        this.UpdateCashOnHandText(this.cashOnHand, 10000);
        this.UpdateNetworthText(this.netWorth, 10000);
    }
    public void UpdateCashOnHandText(int oldValue, int newValue)
    {
        if (isServer)
            cashOnHand = newValue;
        if (isClient)
        {
            if (hasAuthority)
            {
                try
                {
                    GameplayManager.instance.UpdateLocalPlayersCashOnHand(newValue);
                }
                catch (Exception e)
                {
                    Debug.Log("GameplayManager.instance.UpdateLocalPlayersCashOnHand(newValue) failed " + e);
                }
               // try
               // {
                    if (stalkListItems.Length > 0)
                    {
                        Debug.Log("UpdateCashOnHandText: stalkListItems Exist");
                        GetMaxPlayerCanAfford(newValue);
                    }
                    else
                    {
                        Debug.Log("UpdateCashOnHandText: stalkListItemsExist don't exist, creating");
                        stalkListItems = GameObject.FindGameObjectsWithTag("StalkListItem");
                        GetMaxPlayerCanAfford(newValue);
                    }
                //}
                //catch
                //{
                //   Debug.Log("UpdateCashOnHandText: failed to get stalklistitems into array");
                //}
            }
                
        }            
    }
    public void UpdateNetworthText(int oldValue, int newValue)
    {
        if (isServer)
        {
            netWorth = newValue;
        }   
        if (isClient)
        {
            if (hasAuthority)
            {
                if(myPlayerRankingsListItemObject)
                    myPlayerRankingsListItemScript.CmdUpdateNetWorth(newValue);
                GameplayManager.instance.UpdateLocalPlayersNetworth(newValue);
            }
                
        }            
    }
    /*public void CreatePlayerRankListItem()
    {
        Debug.Log("Executing CreatePlayerRankListItem for player: " + this.PlayerName);
        try
        {
            if (myPlayerRankingsListItemObject)
                Destroy(myPlayerRankingsListItemObject);
            GameObject newPlayerRankingsListItem = Instantiate(PlayerRankingsListItemPrefab, transform.position, Quaternion.identity);
            myPlayerRankingsListItemObject = newPlayerRankingsListItem;
            myPlayerRankingsListItemScript = myPlayerRankingsListItemObject.GetComponent<PlayerRankingsListItem>();
            myPlayerRankingsListItemScript.playerName = this.PlayerName;
            myPlayerRankingsListItemScript.playerNetworth = this.netWorth;
            myPlayerRankingsListItemScript.playerReadyStatus = this.isPlayerReady;
            myPlayerRankingsListItemScript.playerSteamId = this.playerSteamId;
            //myPlayerRankingsListItemScript.myLocalGamePlayerScript = this;
            myPlayerRankingsListItemScript.SetPlayerInfo();
        }
        catch
        {
            Debug.Log("CreatePlayerRankListItem: could not instantiate the object");
        }
        
        try
        {
            GameplayManager.instance.AddPlayerToRankingsList(myPlayerRankingsListItemObject);
        }
        catch
        {
            Debug.Log("CreatePlayerRankListItem: could not add to GameplayManager's AddPlayerToRankingsList");
        }
    }*/
    [Client]
    public void ReadyUp()
    {
        Debug.Log("Executing ReadyUp on " + this.PlayerName);
        if (hasAuthority)
            CmdPlayerReadyUp();
    }
    [Command]
    void CmdPlayerReadyUp()
    {
        Debug.Log("Executing CmdPlayerReadyUp for " + this.PlayerName);
        //this.UpdatePlayerReadyStatus(this.isPlayerReady, !this.isPlayerReady);
        NetworkIdentity networkIdentity = connectionToClient.identity;
        GamePlayer requestingPlayer = networkIdentity.GetComponent<GamePlayer>();
        requestingPlayer.isPlayerReady = !requestingPlayer.isPlayerReady;
        CheckIfAllPlayersAreReadyForNextyear();
    }
    [Server]
    void CheckIfAllPlayersAreReadyForNextyear()
    {
        Debug.Log("CheckIfAllPlayersAreReadyForNextyear");
        bool allPlayersReady = false;
        foreach (GamePlayer gamePlayer in Game.GamePlayers)
        {
            if (!gamePlayer.isPlayerReady)
            {
                Debug.Log("CheckIfAllPlayersAreReadyForNextyear: " + gamePlayer.PlayerName + " is not ready!");
                allPlayersReady = false;
                break;
            }
            else
            {
                allPlayersReady = true;
            }
        }
        if (allPlayersReady)
        {
            Debug.Log("CheckIfAllPlayersAreReadyForNextyear: All players are ready!");
            foreach (GamePlayer gamePlayer in Game.GamePlayers)
            {
                gamePlayer.isPlayerReady = false;

            }
            GameplayManager.instance.NewYear();
        }
    }
    public void UpdatePlayerReadyStatus(bool oldValue, bool newValue)
    {
        Debug.Log("UpdatePlayerReadyStatus for player: " + this.PlayerName + " with value: " + newValue.ToString() + " from old value: " + oldValue.ToString());
        /*if (isServer)
        {
            this.isPlayerReady = newValue;
            if (newValue)
            {
                GameplayManager.instance.CheckIfAllPlayersAreReady();
            }
        }            
        if (isClient)
        {
            
            if (hasAuthority)
            {
                GameplayManager.instance.UpdateReadyButtonText(newValue);
                myPlayerRankingsListItemScript.CmdUpdateReadyStatus(newValue);
            }
                
        }*/
        if (hasAuthority)
        {
            GameplayManager.instance.UpdateReadyButtonText(newValue);
            myPlayerRankingsListItemScript.CmdUpdateReadyStatus(newValue);
        }
    }
    [Command]
    public void CmdSpawnPlayerRankListItem()
    {
        Debug.Log("Executing CmdSpawnPlayerRankListItem on the server for player " + this.PlayerName);
        GameObject newPlayerRankingsListItem = Instantiate(PlayerRankingsListItemPrefab, transform.position, Quaternion.identity);
        PlayerRankingsListItem newPlayerRankingsListItemScript = newPlayerRankingsListItem.GetComponent<PlayerRankingsListItem>();
        newPlayerRankingsListItemScript.playerName = this.PlayerName;
        newPlayerRankingsListItemScript.playerNetworth = this.netWorth;
        newPlayerRankingsListItemScript.playerReadyStatus = this.isPlayerReady;
        newPlayerRankingsListItemScript.playerSteamId = this.playerSteamId;
        newPlayerRankingsListItemScript.ownerNetId = this.GetComponent<NetworkIdentity>().netId;
        NetworkServer.Spawn(newPlayerRankingsListItem, connectionToClient);
    }
    public void PlayerBuyStalks(string stalkName, int amountToBuy)
    {
        if (hasAuthority)
            CmdPlayerBuyStalks(stalkName, amountToBuy);
    }
    [Command]
    void CmdPlayerBuyStalks(string stalkName, int amountToBuy)
    {
        Debug.Log("CmdPlayerBuyStalks for " + amountToBuy.ToString() + " shares of " + stalkName + " for player " + this.PlayerName);
        int amountToBuyAbsolute = Mathf.Abs(amountToBuy);
        if (amountToBuyAbsolute > 0)
        {
            if (GameplayManager.instance.currentYear <= GameplayManager.instance.maxNumberOfYears)
            {
                bool canPlayerAffordStalks = false;
                int costOfStalks = StalkManager.instance.GetPriceOfPurchase(stalkName, amountToBuyAbsolute);
                if (costOfStalks > 0)
                {
                    if (this.cashOnHand >= costOfStalks)
                    {
                        Debug.Log("CmdPlayerBuyStalks: cash on hand: " + this.cashOnHand.ToString() + " cost of purchase: " + costOfStalks.ToString() + " " + this.PlayerName + " CAN afford the stalks.");
                        canPlayerAffordStalks = true;
                    }
                    else
                    {
                        Debug.Log("CmdPlayerBuyStalks: cash on hand: " + this.cashOnHand.ToString() + " cost of purchase: " + costOfStalks.ToString() + " " + this.PlayerName + " CANNOT afford the stalks.");
                        canPlayerAffordStalks = false;
                    }
                }

                if (canPlayerAffordStalks)
                {
                    StalkManager.instance.PlayerBuyOrSellStalks(this, stalkName, amountToBuyAbsolute, true);
                    int newCashOnHand = this.cashOnHand;
                    newCashOnHand -= costOfStalks;
                    this.UpdateCashOnHandText(this.cashOnHand, newCashOnHand);
                }
                else
                {
                    Debug.Log("CmdPlayerBuyStalks: Player cannot afford purchase");
                    string errorMessage = "Cannot afford stalk purchase. Cost of stalks is: " + costOfStalks.ToString();
                    TargetErrorMessageReceived(connectionToClient, errorMessage);
                }
            }

        }
        else
        { 

        }
        
    }
    public void PlayerSellStalks(string stalkName, int amountToSell)
    {
        Debug.Log("PlayerSellStalks: " + this.PlayerName + " is trying to sell " + amountToSell.ToString() + " shares of " + stalkName);
        if (hasAuthority)
            CmdPlayerSellStalks(stalkName, amountToSell);
    }
    [Command]
    void CmdPlayerSellStalks(string stalkName, int amountToSell)
    {
        Debug.Log("CmdPlayerSellStalks for " + amountToSell.ToString() + " shares of " + stalkName + " for player " + this.PlayerName);
        int amountToSellAbsolute = Mathf.Abs(amountToSell);
        if (amountToSellAbsolute > 0)
        {
            if(GameplayManager.instance.currentYear <= GameplayManager.instance.maxNumberOfYears)
                StalkManager.instance.PlayerBuyOrSellStalks(this, stalkName, amountToSellAbsolute, false);
        }
    }
    public void QuitGame()
    {
        if (hasAuthority)
        {
            if (isServer)
            {
                Game.StopHost();
                //Game.HostShutDownServer();
            }
            else
            {
                Game.StopClient();
                //Game.HostShutDownServer();
            }
        }
    }
    public void PlayerRequestOrGiveYeehaw(bool requestYeehaw)
    {
        if (hasAuthority)
        {
            if (requestYeehaw)
            {
                CmdRequestYeehaw();
            }
            else
            {
                CmdGiveYeehaw();
            }
        }
    }
    [Command]
    void CmdRequestYeehaw()
    {
        Debug.Log("CmdRequestYeehaw from " + this.PlayerName);
        GameplayManager.instance.requestingYeehawPlayer = this.PlayerName;
        GameplayManager.instance.requestingYeehawPlayerSteamId = this.playerSteamId;
        RpcYeehawRequested(this.PlayerName, this.playerSteamId);
    }
    [ClientRpc]
    void RpcYeehawRequested(string name, ulong steamid)
    {
        GameplayManager.instance.YeehawRequested(name, steamid);
    }
    [Command]
    void CmdGiveYeehaw()
    {
        Debug.Log("CmdGiveYeehaw from " + this.PlayerName);
        if (GameplayManager.instance.requestingYeehawPlayerSteamId == this.playerSteamId)
        {
            Debug.Log("CmdGiveYeehaw: cannot give yeehaw to yourself.");
            return;
        }
        GameplayManager.instance.givingYeehawPlayer = this.PlayerName;
        RpcYeehawGiven(this.PlayerName);
    }
    [ClientRpc]
    void RpcYeehawGiven(string name)
    {
        GameplayManager.instance.YeehawGiven(name);
    }
    public void GetMaxPlayerCanAfford(int cash)
    {
        Debug.Log("GetMaxPlayerCanAfford with cash " + cash.ToString());
        /*foreach (GameObject stalkListItem in stalkListItems)
        {
            StalkListItem itemScript = stalkListItem.GetComponent<StalkListItem>();
            itemScript.MaxPlayerCanAfford(cash);
        }*/
        for (int i = 0; i < stalkListItems.Length; i++)
        {
            //StalkListItem itemScript = stalkListItem.GetComponent<StalkListItem>();
            stalkListItems[i].GetComponent<StalkListItem>().MaxPlayerCanAfford(cash);
        }
    }
    [ClientRpc]
    public void RpcMaxPlayerCanAfford(string stalkName, int stalkPrice, int cashValue)
    {
        if (hasAuthority)
        {
            Debug.Log("");
        }
    }
    public void GetLoanTermsForPlayer()
    {
        if (hasAuthority)
            CmdGetLoanTermsForPlayer();
    }
    [Command]
    void CmdGetLoanTermsForPlayer()
    {
        Debug.Log("CmdGetLoanTermsForPlayer for player: " + this.PlayerName);
        int maxLoanForPlayer = (int)(this.netWorth * 0.25);
        int minPaymentForPlayer = LoanMinimumPayment(maxLoanForPlayer, 10);
        if (minPaymentForPlayer > 0 && maxLoanForPlayer > 0)
        {
            Debug.Log("CmdGetLoanTermsForPlayer:  return to player max loan: " + maxLoanForPlayer.ToString() + " and min payment " + minPaymentForPlayer.ToString());
            this.UpdateMaxLoanAmount(this.maxLoanAmount, maxLoanForPlayer);
            this.UpdateMinPaymentAmount(this.minPaymentAmount, minPaymentForPlayer);
        }
    }
    [Server]
    public int LoanMinimumPayment(int loanAmount, int years)
    {
        int minPayment = 0;

        double firstHalf = Math.Pow(1.06, years);
        firstHalf -= 1;
        double secondHalf = Math.Pow(1.06, years);
        secondHalf = secondHalf * 0.06;

        double denominator = firstHalf / secondHalf;
        Debug.Log("Calculating the minimum payment. Loanbalance: " + loanAmount + " Years remaining: " + years);
        minPayment = (int)(loanAmount / denominator);
        Debug.Log("LoanMinimumPayment: minimum payment is: " + minPayment.ToString());

        return minPayment;
    }
    public void UpdateMaxLoanAmount(int oldValue, int newValue)
    {
        if (isServer)
            maxLoanAmount = newValue;
        if (isClient)
        {
            if (hasAuthority)
            {
                GameplayManager.instance.UpdateMaxLoanAmountText(newValue);
            }
        }
    }
    public void UpdateMinPaymentAmount(int oldValue, int newValue)
    {
        if (isServer)
            minPaymentAmount = newValue;
        if (isClient)
        {
            if (hasAuthority)
            {
                GameplayManager.instance.UpdateMinPaymentAmountText(newValue);
            }
        }
    }
    public void RequestLoan(int requestedLoanAmount)
    {
        if (hasAuthority)
            CmdRequestLoan(requestedLoanAmount);
    }
    [Command]
    void CmdRequestLoan(int requestedLoanAmount)
    {
        Debug.Log("CmdRequestLoan: " + this.PlayerName + " is requesting a loan of " + requestedLoanAmount.ToString());


        if (!this.doesPlayerHaveLoan)
        {
            int maxLoanForPlayer = (int)(this.netWorth * 0.25);
            if (requestedLoanAmount <= maxLoanForPlayer)
            {
                if (requestedLoanAmount > 99)
                {
                    // Create loan object
                    int minPaymentForPlayer = LoanMinimumPayment(requestedLoanAmount, 10);
                    Debug.Log("CmdRequestLoan: requested loan of " + requestedLoanAmount.ToString() + " is less than or equal to the player's max loan amount of " + maxLoanForPlayer.ToString());
                    GameObject newLoan = Instantiate(playerLoanPrefab, transform.position, Quaternion.identity);
                    Loan newLoanScript = newLoan.GetComponent<Loan>();
                    newLoanScript.ownerName = this.PlayerName;
                    newLoanScript.ownerSteamId = this.playerSteamId;
                    newLoanScript.ownerNetId = this.gameObject.GetComponent<NetworkIdentity>().netId;
                    newLoanScript.loanBalance = requestedLoanAmount;
                    //newLoanScript.UpdateLoanBalance(newLoanScript.loanBalance, requestedLoanAmount);
                    newLoanScript.minPayment = minPaymentForPlayer;
                    //newLoanScript.UpdateMinPayment(newLoanScript.minPayment, minPaymentForPlayer);
                    newLoanScript.yearsRemaining = 11;
                    //newLoanScript.UpdateYearsRemaining(newLoanScript.yearsRemaining, 11);
                    newLoanScript.paidThisYear = true;
                    newLoanScript.missedPayments = 0;

                    NetworkServer.Spawn(newLoan, connectionToClient);
                    StalkManager.instance.allLoans.Add(newLoanScript);

                    //Update gameplayer info
                    int newCashOnHand = this.cashOnHand + requestedLoanAmount;
                    this.UpdateCashOnHandText(this.cashOnHand, newCashOnHand);
                    this.HandleDoesPlayerHaveLoan(this.doesPlayerHaveLoan, true);
                }
                else
                {
                    string errorMessage = "Minimum loan amount is $100.";
                    TargetErrorMessageReceived(connectionToClient, errorMessage);
                    return;
                }
                
            }
            else
            {
                Debug.Log("CmdRequestLoan: requested loan of " + requestedLoanAmount.ToString() + " is GREATER THAN the player's max loan amount of " + maxLoanForPlayer.ToString());
                string errorMessage = "Loan request of " + requestedLoanAmount.ToString() + " is more than your max available loan of " + maxLoanForPlayer.ToString();
                TargetErrorMessageReceived(connectionToClient, errorMessage);
                return;
            }
        }
        else
        {
            Debug.Log("CmdRequestLoan: player already has a loan!");
            return;
        }
        
    }
    void HandleDoesPlayerHaveLoan(bool oldValue, bool newValue)
    {
        if (isServer)
            doesPlayerHaveLoan = newValue;
        if (isClient)
        {
            if (hasAuthority)
            {
                SteamAchievementManager.instance.TookOutLoan();
            }
        }
    }
    public void PayLoan(int payment)
    {
        if (hasAuthority)
            CmdPayLoan(payment);

    }
    [Command]
    void CmdPayLoan(int payment)
    {
        Debug.Log("CmdPayLoan: " + this.PlayerName + " is trying to make a loan payment of: " + payment.ToString());
        if (payment > 0)
        {
            if (payment >= this.myLoanScript.minPayment)
            {
                if (payment <= this.cashOnHand)
                {
                    if (payment > this.myLoanScript.loanBalance)
                    {
                        Debug.Log("CmdPayLoan: payment of " + payment.ToString() + " is more than the loan balance of " + this.myLoanScript.loanBalance.ToString() + ". Lowering payment to be equal to loan balance.");
                        payment = this.myLoanScript.loanBalance;
                    }
                    int newLoanBalance = this.myLoanScript.loanBalance - payment;
                    Debug.Log("CmdPayLoan: previous loan balance of " + this.myLoanScript.loanBalance.ToString() + " will be reduced to " + newLoanBalance.ToString());
                    this.myLoanScript.UpdateLoanBalance(this.myLoanScript.loanBalance, newLoanBalance);

                    int newCashOnHand = this.cashOnHand - payment;
                    this.UpdateCashOnHandText(this.cashOnHand, newCashOnHand);

                    if (newLoanBalance > 0)
                    {
                        int newMinPayment = LoanMinimumPayment(newLoanBalance, (this.myLoanScript.yearsRemaining - 1));
                        this.myLoanScript.UpdateMinPayment(this.myLoanScript.minPayment, newMinPayment);
                    }
                    else
                    {
                        this.myLoanScript.loanBalance = 0;
                        this.myLoanScript.minPayment = 0;
                        this.isLoanPaidOff = true;
                        RpcPaidOffLoan();
                    }

                    this.myLoanScript.HandlePaidThisYear(this.myLoanScript.paidThisYear, true);

                    if (newLoanBalance <= 0)
                        StalkManager.instance.LoanPaidOff(this.myLoanScript);
                }
                else
                {
                    Debug.Log("CmdPayLoan: payment of " + payment.ToString() + " is more than " + this.PlayerName + " cash on hand of " + this.cashOnHand.ToString());
                    string errorMessage = "Loan payment of " + payment.ToString() + " is more than you cash of " + this.cashOnHand.ToString();
                    TargetErrorMessageReceived(connectionToClient, errorMessage);
                    return;
                }
            }
            else
            {
                Debug.Log("CmdPayLoan: payment of " + payment.ToString() + " is less than minimum payment of " + this.myLoanScript.minPayment.ToString());
                string errorMessage = "Loan payment of " + payment.ToString() + " is less than the minimum payment of: " + this.myLoanScript.minPayment.ToString();
                TargetErrorMessageReceived(connectionToClient, errorMessage);
                return;
            }
        }        
    }
    public void HandleIsPlayerBankrupt(bool oldValue, bool newValue)
    {
        if (isServer)
            isPlayerBankrupt = newValue;
        if (isClient)
        {
            if (hasAuthority && newValue)
            {
                GameplayManager.instance.HideAllLoanPanels();
                SteamAchievementManager.instance.DefaultedOnLoan();
            }
        }
    }
    [TargetRpc]
    public void TargetErrorMessageReceived(NetworkConnection target, string errorMessage)
    {
        Debug.Log("TargetErrorMessageReceived: Error message received for " + this.PlayerName);
        if (hasAuthority)
        {
            try
            {
                if (GameplayManager.instance.errorMessageRoutineRunning)
                    StopCoroutine(errorMessageRoutine);
                errorMessageRoutine = GameplayManager.instance.ShowErrorMessage(errorMessage);
                StartCoroutine(errorMessageRoutine);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            
        }
    }
    [ClientRpc]
    void RpcPaidOffLoan()
    {
        if (hasAuthority)
        {
            SteamAchievementManager.instance.PaidOffLoan();
        }
    }
}
