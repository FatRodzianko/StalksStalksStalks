using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class StalkManager : NetworkBehaviour
{
    public static StalkManager instance;

    [SerializeField] private GameObject[] stalkPrefabs;
    [SyncVar(hook = nameof(ActivateButtons))] public bool activateButtons;

    [Header("GamePlayers")]
    public List<GamePlayer> gamePlayers = new List<GamePlayer>();

    [Header("Stalks")]
    public SyncList<Stalk> stalks = new SyncList<Stalk>();

    [Header("Loans")]
    public SyncList<Loan> allLoans = new SyncList<Loan>();

    [Header("Stalks Owned UI")]
    [SerializeField] private GameObject StalksOwnedListItemPrefab;
    public List<StalkOwnedListItem> stalksOwnedListItems = new List<StalkOwnedListItem>();

    /*struct SpecialEvent
    {
        public string stalkName;
        public string description;
        public int priceChange;
    }*/
    public class SpecialEvent
    {

        public string StalkName { get; set; }
        public string Description { get; set; }
        public int PriceChange { get; set; }

        public SpecialEvent(string stalkname, string description, int pricechange)
        {
            StalkName = stalkname;
            Description = description;
            PriceChange = pricechange;
        }
    }
    [System.Serializable]
    public class StalksOwned
    {
        public string PlayerName { get; set; }
        public ulong PlayerSteamId { get; set; }
        public uint PlayerNetId { get; set; }
        public string StalkName { get; set; }
        public Stalk StalkScript { get; set; }
        public int TotalStalks { get; set; }


        public StalksOwned(string playername, ulong playerSteamId, uint playerNetId, string stalkname, Stalk stalkScript, int totalstalks)
        {
            PlayerName = playername;
            PlayerSteamId = playerSteamId;
            PlayerNetId = playerNetId;
            StalkName = stalkname;
            StalkScript = stalkScript;
            TotalStalks = totalstalks;
        }
    }
    private List<SpecialEvent> BearEvents = new List<SpecialEvent>();
    private List<SpecialEvent> BullEvents = new List<SpecialEvent>();
    public List<StalksOwned> stalksOwned = new List<StalksOwned>();
    // Start is called before the first frame update
    void Awake()
    {
        MakeInstance();
    }
    private void Start()
    {
        CreateSpecialEvents();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    void MakeInstance()
    {
        Debug.Log("StalkManager MakeInstance.");
        if (instance == null)
            instance = this;
    }
    [Server]
    public void CreateNewStalks()
    {
        Debug.Log("Executing CreateNewStalks on the server");
        foreach (GameObject stalkPrefabToSpawn in stalkPrefabs)
        { 
            GameObject newStalk = Instantiate(stalkPrefabToSpawn, transform.position, Quaternion.identity);
            NetworkServer.Spawn(newStalk);
            stalks.Add(newStalk.GetComponent<Stalk>());
        }

    }
    public void ActivateBuyButtons()
    {
        Debug.Log("ActivateBuyButtons");
        foreach (Stalk stalk in stalks)
        {
            stalk.myStalkListItemScript.ActivateBuyButtons();
        }
    }
    public void ActivateButtons(bool oldValue, bool newValue)
    {
        if (newValue)
            ActivateBuyButtons();

    }
    [Server]
    public void AdjustStalksForNewYear(string bearOrBull)
    {
        Debug.Log("AdjustStalksForNewYear for :" + bearOrBull);
        var rng = new System.Random();
        List<Stalk> bankruptStalks = new List<Stalk>();
        //List<Stalk> stalksToSplit = new List<Stalk>();
        if (bearOrBull.ToLower().Contains("bear"))
        {
            foreach (Stalk stalk in stalks)
            {
                int newPricePerShare = stalk.pricePerShare;
                int newYearlyChange = 0;
                // Random chance of stalk price INCREASING during a bear year
                int randomChance = rng.Next(0, 10);
                if (stalk.stalkName != "BERRY BONDS FROM DIE HARD")
                {
                    if (randomChance > 8)
                    {
                        newYearlyChange = rng.Next(0, 6);
                    }
                    else
                    {
                        newYearlyChange = rng.Next((stalk.maxChangeBear * -1), (stalk.minChangeBear * -1));
                        if (GameplayManager.instance.maxNumberOfYears > 49)
                        {
                            Debug.Log("AdjustStalksForNewYear: More than 50 year game. Softening the yearly change. Old yearly change: " + newYearlyChange.ToString());
                            newYearlyChange = (int)(newYearlyChange * 0.45);
                            Debug.Log("AdjustStalksForNewYear: More than 50 year game. Softening the yearly change. NEW yearly change: " + newYearlyChange.ToString());
                        }
                        else if (GameplayManager.instance.maxNumberOfYears > 19)
                        {
                            Debug.Log("AdjustStalksForNewYear: More than 20 year game. Softening the yearly change. Old yearly change: " + newYearlyChange.ToString());
                            newYearlyChange = (int)(newYearlyChange * 0.7);
                            Debug.Log("AdjustStalksForNewYear: More than 20 year game. Softening the yearly change. NEW yearly change: " + newYearlyChange.ToString());
                        }
                    }
                    stalk.UpdateYearlyChange(stalk.YearlyChange, newYearlyChange);
                    newPricePerShare += newYearlyChange;
                    if (newPricePerShare <= 0)
                    {
                        Debug.Log("AdjustStalksForNewYear: stalk price of " + stalk.stalkName + " is now " + newPricePerShare.ToString() + " and is now bankrupt!!!");
                        bankruptStalks.Add(stalk);
                    }
                    /*if (newPricePerShare > 150)
                    {
                        Debug.Log("AdjustStalksForNewYear: stalk price of " + stalk.stalkName + " is now " + newPricePerShare.ToString() + " and will now split!!!");
                        stalksToSplit.Add(stalk);
                    }
                    else
                        stalk.HandleDidStalkSplit(stalk.didStalkSplit, false);*/
                    stalk.UpdatePricePerShare(stalk.pricePerShare, newPricePerShare);
                }                
            }
        }
        else
        {
            foreach (Stalk stalk in stalks)
            {
                int newPricePerShare = stalk.pricePerShare;
                int newYearlyChange = 0;
                // Random chance of stalk price DECREASING during a bull year
                int randomChance = rng.Next(0, 10);
                if (stalk.stalkName != "BERRY BONDS FROM DIE HARD")
                {
                    if (randomChance > 8)
                    {
                        newYearlyChange = rng.Next(-10, 1);
                    }
                    else
                    {
                        newYearlyChange = rng.Next((stalk.minChangeBull), (stalk.maxChangeBull));
                        if (GameplayManager.instance.maxNumberOfYears > 49)
                        {
                            Debug.Log("AdjustStalksForNewYear: More than 50 year game. Softening the yearly change. Old yearly change: " + newYearlyChange.ToString());
                            newYearlyChange = (int)(newYearlyChange * 0.5);
                            Debug.Log("AdjustStalksForNewYear: More than 50 year game. Softening the yearly change. NEW yearly change: " + newYearlyChange.ToString());
                        }
                        else if (GameplayManager.instance.maxNumberOfYears > 19)
                        {
                            Debug.Log("AdjustStalksForNewYear: More than 20 year game. Softening the yearly change. Old yearly change: " + newYearlyChange.ToString());
                            newYearlyChange = (int)(newYearlyChange * 0.8);
                            Debug.Log("AdjustStalksForNewYear: More than 20 year game. Softening the yearly change. NEW yearly change: " + newYearlyChange.ToString());
                        }
                    }
                    //int newYearlyChange = rng.Next((stalk.minChangeBull), (stalk.maxChangeBull));
                    stalk.UpdateYearlyChange(stalk.YearlyChange, newYearlyChange);
                    newPricePerShare += newYearlyChange;
                    if (newPricePerShare <= 0)
                    {
                        Debug.Log("AdjustStalksForNewYear: stalk price of " + stalk.stalkName + " is now " + newPricePerShare.ToString() + " and is now bankrupt!!!");
                        bankruptStalks.Add(stalk);
                    }
                    /*if (newPricePerShare > 150)
                    {
                        Debug.Log("AdjustStalksForNewYear: stalk price of " + stalk.stalkName + " is now " + newPricePerShare.ToString() + " and will now split!!!");
                        stalksToSplit.Add(stalk);
                    }
                    else
                        stalk.HandleDidStalkSplit(stalk.didStalkSplit, false);*/
                    stalk.UpdatePricePerShare(stalk.pricePerShare, newPricePerShare);
                }
                
            }
        }
        if (bankruptStalks.Count > 0)
        {
            RemoveBankruptStalks(bankruptStalks);
            GameplayManager.instance.RpcPlayBearOrBullSounds("bankrupt");
        }
        /*if (stalksToSplit.Count > 0)
        {
            SplitStalks(stalksToSplit);
        }*/
        GetYearEvent(bearOrBull);
    }
    [Server]
    void GetYearEvent(string bearOrBull)
    {
        Debug.Log("GetYearEvent for " + bearOrBull + " year");
        var rng = new System.Random();
        List<Stalk> bankruptStalks = new List<Stalk>();
        //List<Stalk> stalksToSplit = new List<Stalk>();
        if (bearOrBull.ToLower().Contains("bear") && BearEvents.Count > 0)
        {
            SpecialEvent bearEvent = BearEvents[rng.Next(BearEvents.Count - 1)];
            Debug.Log("GetYearEvent: the bear event is for stalk " + bearEvent.StalkName + " will change by: " + bearEvent.PriceChange.ToString());
            GameplayManager.instance.SpecialEventForNewYear(bearEvent.StalkName, bearEvent.Description, bearEvent.PriceChange);
            foreach (Stalk stalk in stalks)
            {
                if (stalk.stalkName == bearEvent.StalkName)
                {
                    int newPrice = stalk.pricePerShare;
                    newPrice += bearEvent.PriceChange;
                    if (newPrice <= 0)
                        bankruptStalks.Add(stalk);
                    /*if (newPrice > 150)
                    {
                        Debug.Log("AdjustStalksForNewYear: stalk price of " + stalk.stalkName + " is now " + newPrice.ToString() + " and will now split!!!");
                        stalksToSplit.Add(stalk);
                    }
                    else
                        stalk.HandleDidStalkSplit(stalk.didStalkSplit, false);*/
                    stalk.UpdatePricePerShare(stalk.pricePerShare, newPrice);
                }
            }
        }
        else if (bearOrBull.ToLower().Contains("bull") && BullEvents.Count > 0)
        {
            SpecialEvent bullEvent = BullEvents[rng.Next(BullEvents.Count - 1)];
            Debug.Log("GetYearEvent: the bear event is for stalk " + bullEvent.StalkName + " will change by: " + bullEvent.PriceChange.ToString());
            GameplayManager.instance.SpecialEventForNewYear(bullEvent.StalkName, bullEvent.Description, bullEvent.PriceChange);
            foreach (Stalk stalk in stalks)
            {
                if (stalk.stalkName == bullEvent.StalkName)
                {
                    int newPrice = stalk.pricePerShare;
                    newPrice += bullEvent.PriceChange;
                    if (newPrice <= 0)
                        bankruptStalks.Add(stalk);
                    /*if (newPrice > 150)
                    {
                        Debug.Log("AdjustStalksForNewYear: stalk price of " + stalk.stalkName + " is now " + newPrice.ToString() + " and will now split!!!");
                        stalksToSplit.Add(stalk);
                    }
                    else
                        stalk.HandleDidStalkSplit(stalk.didStalkSplit, false);*/
                    stalk.UpdatePricePerShare(stalk.pricePerShare, newPrice);
                }
            }
        }
        else
        {
            Debug.Log("GetYearEvent: No more yearly events exist!");
            GameplayManager.instance.SpecialEventForNewYear("", "", 0);
        }
        if (bankruptStalks.Count > 0)
        {
            RemoveBankruptStalks(bankruptStalks);
            GameplayManager.instance.RpcPlayBearOrBullSounds("bankrupt");
        }
        /*if (stalksToSplit.Count > 0)
        {
            SplitStalks(stalksToSplit);
        }*/
        //CheckForBankruptStalks(bearOrBull);
        //PayDividendsToPlayers(bearOrBull);
        CheckForSplitStalks(bearOrBull);
    }
    [Server]
    void SplitStalks(List<Stalk> stalksToSplit)
    {
        Debug.Log("CheckForStalkSplits for " + stalksToSplit.Count.ToString() + " stalks.");
        foreach (Stalk splitStalk in stalksToSplit)
        {
            int priceOfStalkToSplit = splitStalk.pricePerShare;
            int newPriceAfterSplit = (priceOfStalkToSplit / 2);
            Debug.Log("CheckForStalkSplits: " + splitStalk.stalkName + " split. Previous price: " + priceOfStalkToSplit.ToString() + " new price after split = " + newPriceAfterSplit.ToString());
            splitStalk.UpdatePricePerShare(splitStalk.pricePerShare, newPriceAfterSplit);
            foreach (StalksOwned owned in stalksOwned)
            {
                if (owned.StalkName == splitStalk.stalkName && owned.TotalStalks > 0)
                {
                    owned.TotalStalks *= 2;
                    //Find the owner of the stalk's Gameplayer object to get the split stalk achievement?
                    GamePlayer playerOwner = NetworkIdentity.spawned[owned.PlayerNetId].gameObject.GetComponent<GamePlayer>();
                    Debug.Log("StalkManager.cs: SplitStalks: player owns a stalk that split. " + playerOwner.PlayerName + " owned player name: " + owned.PlayerName + owned.PlayerNetId.ToString());
                    playerOwner.RpcStalksSplit();
                }
            }
            foreach (StalkOwnedListItem stalkOwnedListItem in stalksOwnedListItems)
            {
                if (stalkOwnedListItem.stalkName == splitStalk.stalkName && stalkOwnedListItem.totalStalksOwned > 0)
                {
                    int newStalksOwnedNumber = (stalkOwnedListItem.totalStalksOwned * 2);
                    stalkOwnedListItem.UpdateTotalStalksOwned(stalkOwnedListItem.totalStalksOwned, newStalksOwnedNumber);
                }
            }
            splitStalk.HandleDidStalkSplit(splitStalk.didStalkSplit, true);

        }
        
    }
    [Server]
    void CheckForSplitStalks(string bearOrBull)
    {
        Debug.Log("CheckForSplitStalks");

        foreach (Stalk stalk in stalks)
        {
            if (stalk.pricePerShare > 150)
            {
                Debug.Log("CheckForSplitStalks: " + stalk.stalkName + " split!!!");
                int priceOfStalkToSplit = stalk.pricePerShare;
                int newPriceAfterSplit = (priceOfStalkToSplit / 2);
                if (priceOfStalkToSplit % 2 != 0)
                    newPriceAfterSplit++;
                Debug.Log("CheckForStalkSplits: " + stalk.stalkName + " split. Previous price: " + priceOfStalkToSplit.ToString() + " new price after split = " + newPriceAfterSplit.ToString());
                stalk.UpdatePricePerShare(stalk.pricePerShare, newPriceAfterSplit);
                foreach (StalksOwned owned in stalksOwned)
                {
                    if (owned.StalkName == stalk.stalkName && owned.TotalStalks > 0)
                    {
                        owned.TotalStalks *= 2;
                        GamePlayer playerOwner = NetworkIdentity.spawned[owned.PlayerNetId].gameObject.GetComponent<GamePlayer>();
                        Debug.Log("StalkManager.cs: CheckForSplitStalks: player owns a stalk that split. " + playerOwner.PlayerName + " owned player name: " + owned.PlayerName + owned.PlayerNetId.ToString());
                        playerOwner.RpcStalksSplit();
                    }
                }
                foreach (StalkOwnedListItem stalkOwnedListItem in stalksOwnedListItems)
                {
                    if (stalkOwnedListItem.stalkName == stalk.stalkName && stalkOwnedListItem.totalStalksOwned > 0)
                    {
                        int newStalksOwnedNumber = (stalkOwnedListItem.totalStalksOwned * 2);
                        stalkOwnedListItem.UpdateTotalStalksOwned(stalkOwnedListItem.totalStalksOwned, newStalksOwnedNumber);
                    }
                }
                stalk.HandleDidStalkSplit(stalk.didStalkSplit, true);
            }
            else
            {
                stalk.HandleDidStalkSplit(stalk.didStalkSplit, false);
            }
        }

        PayDividendsToPlayers(bearOrBull);
    }
    [Server]
    void PayDividendsToPlayers(string bearOrBull)
    {
        Debug.Log("PayDividendsToPlayers: for " + bearOrBull + " year");

        foreach (GamePlayer player in gamePlayers)
        {
            foreach (StalksOwned owned in stalksOwned)
            {
                if (owned.PlayerSteamId == player.playerSteamId && owned.TotalStalks > 0)
                {
                    int dividendAmount = 0;
                    if (bearOrBull.ToLower().Contains("bear"))
                    {
                        dividendAmount = owned.StalkScript.bearDividend;
                    }
                    else
                    {
                        dividendAmount = owned.StalkScript.bullDividend;
                    }
                    /*foreach (Stalk stalk in stalks)
                    {
                        if (stalk.stalkName == owned.StalkName)
                        {
                            if (bearOrBull.ToLower().Contains("bear"))
                            {
                                dividendAmount = stalk.bearDividend;
                            }
                            else
                            {
                                dividendAmount = stalk.bullDividend;
                            }
                        }
                    }*/
                    int dividendPayment = (owned.TotalStalks * dividendAmount);
                    int newCashOnHand = player.cashOnHand + dividendPayment;
                    Debug.Log("PayDividendsToPlayers: paying " + player.PlayerName + " a dividend of " + dividendPayment.ToString() + " for " + owned.TotalStalks + " shares in " + owned.StalkName);
                    player.UpdateCashOnHandText(player.cashOnHand, newCashOnHand);
                }
            }
        }
        //CalculateNetWorth();
        AdjustLoans();
    }
    [Server]
    void AdjustLoans()
    {
        Debug.Log("Adjust loans: " + GameplayManager.instance.areLoansEnabled.ToString());
        if (GameplayManager.instance.areLoansEnabled)
        {
            foreach (Loan loan in allLoans)
            {
                int newYearsRemaining = (loan.yearsRemaining - 1);
                loan.UpdateYearsRemaining(loan.yearsRemaining, newYearsRemaining);

                if (!loan.paidThisYear)
                {
                    Debug.Log("AdjustLoans: " + loan.ownerName + " did NOT pay their loan last year!!!");
                    loan.missedPayments++;

                    if (loan.missedPayments == 1)
                    {
                        Debug.Log("AdjustLoans: first missed payment for " + loan.ownerName + " balance before penalty is " + loan.loanBalance.ToString());
                        int newLoanBalance = (int)(loan.loanBalance * 1.10);
                        Debug.Log("AdjustLoans: first missed payment for " + loan.ownerName + " balance AFTER penalty is " + newLoanBalance.ToString());
                        loan.UpdateLoanBalance(loan.loanBalance, newLoanBalance);

                        GamePlayer loanOwner = NetworkIdentity.spawned[loan.ownerNetId].gameObject.GetComponent<GamePlayer>();
                        int newMinPayment = 0;
                        newMinPayment = loanOwner.LoanMinimumPayment(newLoanBalance, newYearsRemaining);
                        loan.UpdateMinPayment(loan.minPayment, newMinPayment);
                    }
                    else if (loan.missedPayments == 2)
                    {
                        Debug.Log("AdjustLoans: second missed payment for " + loan.ownerName + " balance before penalty is " + loan.loanBalance.ToString());
                        int newLoanBalance = (int)(loan.loanBalance * 1.15);
                        Debug.Log("AdjustLoans: second missed payment for " + loan.ownerName + " balance AFTER penalty is " + newLoanBalance.ToString());
                        loan.UpdateLoanBalance(loan.loanBalance, newLoanBalance);

                        GamePlayer loanOwner = NetworkIdentity.spawned[loan.ownerNetId].gameObject.GetComponent<GamePlayer>();
                        int newMinPayment = 0;
                        newMinPayment = loanOwner.LoanMinimumPayment(newLoanBalance, newYearsRemaining);
                        loan.UpdateMinPayment(loan.minPayment, newMinPayment);
                    }
                    else if (loan.missedPayments == 3)
                    {
                        Debug.Log("AdjustLoans: THIRD missed payment for " + loan.ownerName + ". Player is now bankrupt!!!");
                        GamePlayer bankruptPlayer = NetworkIdentity.spawned[loan.ownerNetId].gameObject.GetComponent<GamePlayer>();
                        bankruptPlayer.UpdateCashOnHandText(bankruptPlayer.cashOnHand, 0);
                        bankruptPlayer.UpdateNetworthText(bankruptPlayer.netWorth, 0);
                        stalksOwned.RemoveAll(x => x.PlayerSteamId == bankruptPlayer.playerSteamId);

                        List<StalkOwnedListItem> stalksOwnedListItemsToRemove = new List<StalkOwnedListItem>();
                        foreach (StalkOwnedListItem stalkOwnedListItem in stalksOwnedListItems)
                        {
                            if (stalkOwnedListItem.ownerName == bankruptPlayer.PlayerName && stalkOwnedListItem.ownerSteamId == bankruptPlayer.playerSteamId)
                            {
                                stalksOwnedListItemsToRemove.Add(stalkOwnedListItem);
                            }
                        }
                        if (stalksOwnedListItemsToRemove.Count > 0)
                        {
                            foreach (StalkOwnedListItem item in stalksOwnedListItemsToRemove)
                            {
                                stalksOwnedListItems.Remove(item);
                                NetworkServer.Destroy(item.gameObject);
                            }
                        }

                        //bankruptPlayer.doesPlayerHaveLoan = false;
                        bankruptPlayer.isPlayerBankrupt = true;
                        loan.yearsRemaining = 0;
                        loan.loanBalance = 0;
                        loan.minPayment = 0;
                        allLoans.Remove(loan);
                        NetworkServer.Destroy(loan.gameObject);

                    }
                }
                else
                    loan.HandlePaidThisYear(loan.paidThisYear, false);

                Debug.Log("AdjustLoans: " + loan.ownerName + "'s balance before interest is: " + loan.loanBalance.ToString());
                int newBalance = (int)(loan.loanBalance * 1.06);
                Debug.Log("AdjustLoans: " + loan.ownerName + "'s balance AFTER interest is: " + newBalance.ToString());
                loan.UpdateLoanBalance(loan.loanBalance, newBalance);
            }
        }
        CalculateNetWorth();
    }
    [Server]
    void CalculateNetWorth()
    {
        Debug.Log("CalculateNetWorth: Calculating player net worth for new year.");
        foreach (GamePlayer player in gamePlayers)
        {
            int totalStalkValue = 0;
            int netWorthValue = 0;
            int totalUniqueStalksOwned = 0;
            foreach (StalksOwned owned in stalksOwned)
            {
                if (owned.PlayerSteamId == player.playerSteamId && owned.TotalStalks > 0)
                {
                    //int priceOfStalk = GetStalkPrice(owned.StalkName);
                    int priceOfStalk = owned.StalkScript.pricePerShare;
                    if (priceOfStalk > 0)
                    {
                        totalStalkValue = (priceOfStalk * owned.TotalStalks);
                        netWorthValue += totalStalkValue;
                        totalUniqueStalksOwned++;
                    }
                    
                }
            }

            if (totalUniqueStalksOwned == 10)
            {
                Debug.Log("Player owns one of each stalk. Checking if they own enough of each to get the diversified achivement");
                bool doesPlayerOwnTen = false;
                foreach (StalksOwned owned in stalksOwned)
                {
                    if (owned.PlayerSteamId == player.playerSteamId)
                    {
                        if (owned.TotalStalks >= 10)
                            doesPlayerOwnTen = true;
                        else
                        {
                            doesPlayerOwnTen = false;
                            break;
                        }
                    }
                }
                if (doesPlayerOwnTen)
                {
                    Debug.Log("Player owns ten shares of all ten stalks!");
                    player.RpcDiversified();
                }
            }

            netWorthValue += player.cashOnHand;

            if (GameplayManager.instance.areLoansEnabled)
            {
                if (player.doesPlayerHaveLoan && !player.isPlayerBankrupt && !player.isLoanPaidOff)
                {
                    Debug.Log("CalculateNetWorth: " + player.PlayerName + "'s networth value before loan is " + netWorthValue.ToString());
                    int loanAmount = player.myLoanScript.loanBalance;
                    netWorthValue -= loanAmount;
                    Debug.Log("CalculateNetWorth: " + player.PlayerName + " has a loan with a balance of " + loanAmount + " lowering the networth value to " + netWorthValue.ToString()); ;
                }
            }
            Debug.Log("CalculateNetWorth: net worth of " + player.PlayerName + " should now be : " + netWorthValue.ToString());
            player.UpdateNetworthText(player.netWorth, netWorthValue);
        }
        if (GameplayManager.instance.currentYear > GameplayManager.instance.maxNumberOfYears)
        {
            Debug.Log("CalculateNetWorth: The current year is greater than the max years. The game should end now and a winner determined.");
            DetermineWhoWonGame();
        }
    }
    [Server]
    void DetermineWhoWonGame()
    {
        Debug.Log("DetermineWhoWonGame");
        int highestNetWorth = gamePlayers.Max(x => x.netWorth);
        Debug.Log("DetermineWhoWonGame: the highest net worth value is " + highestNetWorth.ToString());
        List<GamePlayer> winningPlayers = gamePlayers.FindAll(x => x.netWorth == highestNetWorth);
        Debug.Log("DetermineWhoWonGame: " + winningPlayers.Count.ToString() + " players match the highest net worth");
        if (winningPlayers.Count > 1)
        {
            Debug.Log("DetermineWhoWonGame: tie on net worth between "+ winningPlayers.Count.ToString() + " players. Determining if cash on hand will break tie.");
            int highestCashOnHand = winningPlayers.Max(x => x.cashOnHand);
            List<GamePlayer> newWinningPlayers = winningPlayers.FindAll(x => x.cashOnHand == highestCashOnHand);
            if (newWinningPlayers.Count > 1)
            {
                Debug.Log("DetermineWhoWonGame: after cash on hand tie breaker, still a tie between " + newWinningPlayers.Count.ToString() + " players.");
                GameplayManager.instance.RpcGameWinnerName(newWinningPlayers, false, true);
            }
            else if (newWinningPlayers.Count == 1)
            {
                Debug.Log("DetermineWhoWonGame: tie broken by cash on hand!");
                GameplayManager.instance.RpcGameWinnerName(newWinningPlayers, true, false);
            }
        }
        else
        {
            Debug.Log("DetermineWhoWonGame: Only one player with highest net worth. No tie breakers needed!");
            GameplayManager.instance.RpcGameWinnerName(winningPlayers, false, false);
        }
    }
    [Server]
    int GetStalkPrice(string stalkNameToFind)
    {
        int stalkPrice = 0;
        foreach (Stalk stalk in stalks)
        {
            if (stalk.stalkName == stalkNameToFind)
            {
                stalkPrice = stalk.pricePerShare;
            }
        }
        return stalkPrice;
    }
    [Server]
    public void CreateStalksOwnedList(List<GamePlayer> newGamePlayers)
    {
        Debug.Log("CreateStalksOwnedList on the server");
        gamePlayers = newGamePlayers;
        foreach (GamePlayer player in gamePlayers)
        {
            foreach (Stalk stalk in stalks)
            {
                stalksOwned.Add(new StalksOwned(player.PlayerName, player.playerSteamId, player.gameObject.GetComponent<NetworkIdentity>().netId, stalk.stalkName, stalk, 0));
            }
        }
    }
    [Server]
    public int GetPriceOfPurchase(string stalkToBuy, int amountToBuy)
    {
        int price = 0;
        foreach (Stalk stalk in stalks)
        {
            if (stalk.stalkName == stalkToBuy)
            {
                price = stalk.pricePerShare * amountToBuy;
            }
        }
        return price;
    }
    [Server]
    public void PlayerBuyOrSellStalks(GamePlayer buyingPlayer, string stalkName, int amountToBuy, bool isPlayerBuying)
    {
        Debug.Log("PlayerBuyOrSellStalks for player " + buyingPlayer.PlayerName + " and they are buying?: " + isPlayerBuying.ToString());
        foreach (StalksOwned owned in stalksOwned)
        {
            if (owned.StalkName == stalkName && owned.PlayerName == buyingPlayer.PlayerName && owned.PlayerSteamId == buyingPlayer.playerSteamId)
            {
                if (isPlayerBuying)
                {
                    Debug.Log("PlayerBuyOrSellStalks: " + buyingPlayer.PlayerName + " will buy " + amountToBuy.ToString() + " shares of " + stalkName + " They already own " + owned.TotalStalks.ToString() + " shares.");
                    if (owned.TotalStalks == 0)
                    {
                        Debug.Log("PlayerBuyOrSellStalks " + buyingPlayer.PlayerName + " did not previously own any shares of " + stalkName + " creating new StalkOwnedListItem");
                        GameObject newStalkOwnedListItem = Instantiate(StalksOwnedListItemPrefab, transform.position, Quaternion.identity);
                        StalkOwnedListItem newStalkOwnedListItemScript = newStalkOwnedListItem.GetComponent<StalkOwnedListItem>();
                        newStalkOwnedListItemScript.ownerName = buyingPlayer.PlayerName;
                        newStalkOwnedListItemScript.ownerSteamId = buyingPlayer.playerSteamId;
                        newStalkOwnedListItemScript.stalkName = stalkName;
                        newStalkOwnedListItemScript.totalStalksOwned = amountToBuy;
                        NetworkServer.Spawn(newStalkOwnedListItem);
                        stalksOwnedListItems.Add(newStalkOwnedListItemScript);
                    }
                    owned.TotalStalks += amountToBuy;
                    foreach (StalkOwnedListItem stalkOwnedListItem in stalksOwnedListItems)
                    {
                        if (stalkOwnedListItem.ownerName == buyingPlayer.PlayerName && stalkOwnedListItem.ownerSteamId == buyingPlayer.playerSteamId && stalkOwnedListItem.stalkName == stalkName)
                        {
                            stalkOwnedListItem.UpdateTotalStalksOwned(stalkOwnedListItem.totalStalksOwned, owned.TotalStalks);
                        }
                    }
                }
                else
                {
                    Debug.Log("PlayerBuyOrSellStalks: " + buyingPlayer.PlayerName + " will SELL " + amountToBuy.ToString() + " shares of " + stalkName + " They already own " + owned.TotalStalks.ToString() + " shares.");
                    if (owned.TotalStalks > 0)
                    {
                        if (owned.TotalStalks >= amountToBuy)
                        {
                            
                            int cashFromSale = GetPriceOfPurchase(owned.StalkName, amountToBuy);
                            if (cashFromSale > 0)
                            {
                                int newCashOnHand = buyingPlayer.cashOnHand + cashFromSale;
                                Debug.Log("PlayerBuyOrSellStalks: Cash from sale of stalks for player " + buyingPlayer.PlayerName + " will be " + cashFromSale.ToString() + ". Previous cash on hand: " + buyingPlayer.cashOnHand.ToString() + " new cash on hand " + newCashOnHand.ToString());
                                buyingPlayer.UpdateCashOnHandText(buyingPlayer.cashOnHand, newCashOnHand);
                                owned.TotalStalks -= amountToBuy;
                                List<StalkOwnedListItem> stalksOwnedListItemsToRemove = new List<StalkOwnedListItem>();
                                foreach (StalkOwnedListItem stalkOwnedListItem in stalksOwnedListItems)
                                {
                                    if (stalkOwnedListItem.ownerName == buyingPlayer.PlayerName && stalkOwnedListItem.ownerSteamId == buyingPlayer.playerSteamId && stalkOwnedListItem.stalkName == stalkName)
                                    {
                                        if (owned.TotalStalks == 0)
                                        {
                                            Debug.Log("PlayerBuyOrSellStalks: " + buyingPlayer.PlayerName + " no longer owns any shares of " + stalkName + " destroying the StalkOwnedListItem.");
                                            stalksOwnedListItemsToRemove.Add(stalkOwnedListItem);
                                            //NetworkServer.Destroy(stalkOwnedListItem.gameObject);
                                        }
                                        else
                                        {
                                            stalkOwnedListItem.UpdateTotalStalksOwned(stalkOwnedListItem.totalStalksOwned, owned.TotalStalks);
                                        }

                                    }
                                }
                                if (stalksOwnedListItemsToRemove.Count > 0)
                                {
                                    foreach (StalkOwnedListItem item in stalksOwnedListItemsToRemove)
                                    {
                                        stalksOwnedListItems.Remove(item);
                                        NetworkServer.Destroy(item.gameObject);
                                    }
                                }
                            }
                            else 
                            {
                                Debug.Log("PlayerBuyOrSellStalks: somehow cash from sale was less than 0. Aborting sale.");
                            }
                            
                        }
                        else
                        {
                            Debug.Log("PlayerBuyOrSellStalks: player tried to sell more stalks than they own");
                            string errorMessage = "You are trying to sell more stalks than you own.";
                            buyingPlayer.TargetErrorMessageReceived(buyingPlayer.connectionToClient, errorMessage);
                        }
                        
                    }
                    
                }
            }
        }
    }
    [Server]
    void RemoveBankruptStalks(List<Stalk> bankruptStalks)
    {
        Debug.Log("RemoveBankruptStalks: Removing this many stalks as bankrupt: " + bankruptStalks.Count.ToString());
        foreach (Stalk bankrupt in bankruptStalks)
        {
            Debug.Log("RemoveBankruptStalks: bankrupting " + bankrupt.stalkName);
            bankrupt.HandleIsBankrupt(bankrupt.isBankrupt, true);
            stalks.Remove(bankrupt);
            stalksOwned.RemoveAll(x => x.StalkName == bankrupt.stalkName);
            BearEvents.RemoveAll(x => x.StalkName == bankrupt.stalkName);
            BullEvents.RemoveAll(x => x.StalkName == bankrupt.stalkName);
            List<StalkOwnedListItem> stalksOwnedListItemsToRemove = new List<StalkOwnedListItem>();
            foreach (StalkOwnedListItem stalkOwnedListItem in stalksOwnedListItems)
            {
                if (stalkOwnedListItem.stalkName == bankrupt.stalkName)
                {
                    Debug.Log("RemoveBankruptStalks: Destroying StalkOwnedListItem of " + stalkOwnedListItem.stalkName + " for player: " + stalkOwnedListItem.ownerName);
                    //stalksOwnedListItems.Remove
                    //NetworkServer.Destroy(stalkOwnedListItem.gameObject);
                    stalksOwnedListItemsToRemove.Add(stalkOwnedListItem);
                    stalkOwnedListItem.BankruptAchievement();
                }
            }
            //stalksOwnedListItems.RemoveAll(x => x.stalkName == bankrupt.stalkName);
            if (stalksOwnedListItemsToRemove.Count > 0)
            {
                foreach (StalkOwnedListItem item in stalksOwnedListItemsToRemove)
                {
                    stalksOwnedListItems.Remove(item);
                    NetworkServer.Destroy(item.gameObject);
                }
            }
        }
    }
    [Server]
    public void LoanPaidOff(Loan paidOffLoan)
    {
        Debug.Log("LoanPaidOff for : " + paidOffLoan.ownerName);
        allLoans.Remove(paidOffLoan);
        NetworkServer.Destroy(paidOffLoan.gameObject);
    }
    void CreateSpecialEvents()
    {
        //add the yearly events
        //Bull events
        BullEvents.Add(new SpecialEvent("strYker!!!", "The company has discovered a way to tap into a rival's oil reserves while they are none the wiser. Time to slurp it up.", 15));
        BullEvents.Add(new SpecialEvent("strYker!!!", "Papa John appointed as Administrator of the EPA.", 10));
        BullEvents.Add(new SpecialEvent("strYker!!!", "New oil deposists found beneath sacred Indian site. ", 5));
        BullEvents.Add(new SpecialEvent("strYker!!!", "Regulations on poisoning the water supply relaxed.", 10));
        BullEvents.Add(new SpecialEvent("strYker!!!", "Coup in Venezuela successful. Chicago Boys on first plane to Caracas to take over the finance ministry.", 20));

        BullEvents.Add(new SpecialEvent("Mafia HUD Scam", "This Old House has a lot of copper pipes. Bada-bing!", 10));
        BullEvents.Add(new SpecialEvent("Mafia HUD Scam", "Great meeting in the Russian bathhouse. You weren't even the fattest one there!", 5));
        BullEvents.Add(new SpecialEvent("Mafia HUD Scam", "City Council member shows up at your underground casino. You spike their drinks and loan them $50k. They lose it all.", 10));
        BullEvents.Add(new SpecialEvent("Mafia HUD Scam", "After purchasing a block of dilapidated homes, Police Commissioner on the dole agrees to clear out the 'riffraff' living in the neighborhood.", 5));
        BullEvents.Add(new SpecialEvent("Mafia HUD Scam", "Head of the Urban League receives bill for daughter's college tuition. They agree to sign off on your scam.", 15));

        BullEvents.Add(new SpecialEvent("Teamsters Pension Fund", "Ah Marone another hotel in Las Vegas needs our help. We're getting in on the ground floor!", 10));
        BullEvents.Add(new SpecialEvent("Teamsters Pension Fund", "Bullets left on the doorsteps of union member dissidents day before leadership elections.", 5));
        BullEvents.Add(new SpecialEvent("Teamsters Pension Fund", "Construction of casino complete. No one will ever find what's beneath the foundation. Vegas, baby!", 10));
        BullEvents.Add(new SpecialEvent("Teamsters Pension Fund", "New union contract signed with FedEx. Nation wide strike ends!", 5));
        BullEvents.Add(new SpecialEvent("Teamsters Pension Fund", "An epidemic of goods falling off the back of trucks and right into the hands of our 'friends.'", 15));

        BullEvents.Add(new SpecialEvent("Springfield Nuclear Power", "Mr CEO Man, time for your sponge bath!", 10));
        BullEvents.Add(new SpecialEvent("Springfield Nuclear Power", "New union contract specifies donuts to be provided each morning to every employee. Productivity increases 10%", 15));
        BullEvents.Add(new SpecialEvent("Springfield Nuclear Power", "Reactor meltdown avoided after employee presses random button on the control panel.", 5));
        BullEvents.Add(new SpecialEvent("Springfield Nuclear Power", "New chairs with added lumbar support provided to all control operators.", 5));
        BullEvents.Add(new SpecialEvent("Springfield Nuclear Power", "Worst performing employee quits to follow their dream of working at the bowling alley. We'll never see them again!", 10));

        BullEvents.Add(new SpecialEvent("North Haverbrook MONORAIL", "The sweet smell of ozone permeates the air as the electrified third rail is powered on.", 5));
        BullEvents.Add(new SpecialEvent("North Haverbrook MONORAIL", "An anchor was conviently found on the runaway monorail train.", 10));
        BullEvents.Add(new SpecialEvent("North Haverbrook MONORAIL", "The citizens of the town come to their good senses, cast away all doubts about the monorail project, and purchase the monorail.", 20));
        BullEvents.Add(new SpecialEvent("North Haverbrook MONORAIL", "No one in this one horse town has heard of, let alone seen, The Music Man.", 15));
        BullEvents.Add(new SpecialEvent("North Haverbrook MONORAIL", "Groundbreaking of the MONORAIL with William Shatner is broadcast on local news.", 10));

        BullEvents.Add(new SpecialEvent("PG&Enron", "Planned blackouts going just as planned. AHHNOLD is sure to win the presidency now!", 10));
        BullEvents.Add(new SpecialEvent("PG&Enron", "Another year of cooked books and paid off auditors. We can't lose!", 10));
        BullEvents.Add(new SpecialEvent("PG&Enron", "It rained ALL WINTER! No need to worry about wildfires this summer! Put off those repairs for another year, baby!", 5));
        BullEvents.Add(new SpecialEvent("PG&Enron", "State legislators approve our 100% price increases!", 5));
        BullEvents.Add(new SpecialEvent("PG&Enron", "Severe drought diverts attention away from us and toward the water companies and water subsidies provided to pistachio farmers.", 5));

        BullEvents.Add(new SpecialEvent("My Mutuals and Me Inc.", "Hey, we're mutuals!", 5));
        BullEvents.Add(new SpecialEvent("My Mutuals and Me Inc.", "New mutual fund created: The NON-STANDARD and RICH 500.", 5));
        BullEvents.Add(new SpecialEvent("My Mutuals and Me Inc.", "New mutual fund created: Small capital business with no slave labor in their supply chain.", 5));
        BullEvents.Add(new SpecialEvent("My Mutuals and Me Inc.", "New mutual fund created: Crypto currency index fund.", 5));
        BullEvents.Add(new SpecialEvent("My Mutuals and Me Inc.", "New mutual fund created: Target retirement fund for naive Zoomers who think they'll ever be able to retire.", 5));

        BullEvents.Add(new SpecialEvent("Unionized Submissives LLC", "The union makes us weaker (which makes us stronger submissives)!", 15));
        BullEvents.Add(new SpecialEvent("Unionized Submissives LLC", "Whose side are you on? The boss's? No! We're on the beautiful and powerful domme's side!", 10));
        BullEvents.Add(new SpecialEvent("Unionized Submissives LLC", "Submissives of the world, unite! You have nothing to gain but more whips and chains!", 15));
        BullEvents.Add(new SpecialEvent("Unionized Submissives LLC", "An owie to one is an owie to all and oh God do we love the owies. More owies please.", 5));
        BullEvents.Add(new SpecialEvent("Unionized Submissives LLC", "All the armies of Europe and Asia combined, could not by force, take a drink from our Golden Streams!", 5));

        BullEvents.Add(new SpecialEvent("My Pillow, Your Pillow, We're All Pillows!", "'My Pillow, Your Pillow, We're All Pillows!', the last manufacturer left in the U.S., moves factories to Laos to save on labor costs.", 15));
        BullEvents.Add(new SpecialEvent("My Pillow, Your Pillow, We're All Pillows!", "New cost saving pillow filling discovered.", 5));
        BullEvents.Add(new SpecialEvent("My Pillow, Your Pillow, We're All Pillows!", "Following the pandemic cruiselines replace all former pillows with 'My Pillow, Your Pillow, We're All Pillows' after experiments show no living thing can survive more than five minutes on the pillow's surface.", 10));
        BullEvents.Add(new SpecialEvent("My Pillow, Your Pillow, We're All Pillows!", "Recovering meth addict and best friend of the founder and owner plants head lice on the pillows in every Motel 6 across the country.", 10));
        BullEvents.Add(new SpecialEvent("My Pillow, Your Pillow, We're All Pillows!", "'My Pillow, Your Pillow, We're All Pillows!' featured in latest flat earth documentry.", 5));

        //Bear Events
        BearEvents.Add(new SpecialEvent("strYker!!!", "You invite your son to go bowling with you. Things don't end \"well.\"", -20));
        BearEvents.Add(new SpecialEvent("strYker!!!", "Coup in Venezuela failed. strYker's hand picked \"legitimate\" leader Otto Von Mengele gained zero popular support and was laughed out of the country.", -10));
        BearEvents.Add(new SpecialEvent("strYker!!!", "New regulations in the U.S. restrict dumping oil directly into rivers.", -15));
        BearEvents.Add(new SpecialEvent("strYker!!!", "Hired former Head of Safety and Quality Assurance from BP to design new offshore oilrigs.", -25));
        BearEvents.Add(new SpecialEvent("strYker!!!", "New policy enacted to allow oil tanker captains (1) cup of beer while on duty. All captains purchase gallon sized cup.", -15));

        BearEvents.Add(new SpecialEvent("Mafia HUD Scam", "Mafia don beats you with a belt after discovering your dalliance with a woman he once looked at.", -10));
        BearEvents.Add(new SpecialEvent("Mafia HUD Scam", "City Council member gets cold feet. You're forced to give them cement shoes. Scam on hold until you can blackmail another council member to play ball.", -20));
        BearEvents.Add(new SpecialEvent("Mafia HUD Scam", "A bright eyed idealist at HUD actually read your load application. Denied.", -20));
        BearEvents.Add(new SpecialEvent("Mafia HUD Scam", "After a hard day's night working (drinking and playing cards in the back of a strip club), your tummy grumbles and you need to poo. It's the first poo in a week, and you've been eating nothing but cured meats during that time.", -15));
        BearEvents.Add(new SpecialEvent("Mafia HUD Scam", "\"What ever happened to the Gary Cooper types\" you say to yourself as you start to cry watching James Cagney beat his wife with a grapefruit in 'The Public enemy.'", -15));

        BearEvents.Add(new SpecialEvent("Teamsters Pension Fund", "Pension fund now managed by Goldman Sachs & Co. and Northern Trust Global Advisors fiduciaries. no more corruption!", -25));
        BearEvents.Add(new SpecialEvent("Teamsters Pension Fund", "Motor Carrier Act passes congress.", -20));
        BearEvents.Add(new SpecialEvent("Teamsters Pension Fund", "No one can find Jimmy.", -15));
        BearEvents.Add(new SpecialEvent("Teamsters Pension Fund", "Strike broken. The 'man' won. Driver pay cut by 20%.", -15));
        BearEvents.Add(new SpecialEvent("Teamsters Pension Fund", "Vegas casinos convinced themselves that adding theme parks to their hotels will finally allow them to take money directly from children. All parks shuttered in early 2000's.", -15));

        BearEvents.Add(new SpecialEvent("Springfield Nuclear Power", "Danke, sir, we're from Germany and here to make your power plant more efficient.", -5));
        BearEvents.Add(new SpecialEvent("Springfield Nuclear Power", "Two headed toads found in river downstream from the plant.", -10));
        BearEvents.Add(new SpecialEvent("Springfield Nuclear Power", "New, cheaper vending machines installed in the break room. Items get stuck in the machine more frequently compared to the older machines. Producivity plummets as an increasing number of employees get their arms stuck in the vending machines.", -15));
        BearEvents.Add(new SpecialEvent("Springfield Nuclear Power", "Your long lost, cherished childhood toy is found by an employee's child. You reluctantly agree to pay the family to feel something for the first time in a decade.", -5));
        BearEvents.Add(new SpecialEvent("Springfield Nuclear Power", "Department of Energy Inspectors arrive at the plant.", -10));

        BearEvents.Add(new SpecialEvent("North Haverbrook MONORAIL", "At the townhall, a busybody citizen asks pointed questions about the details of the monorail project.", -10));
        BearEvents.Add(new SpecialEvent("North Haverbrook MONORAIL", "An intrepid citizen visits neighboring town to see the results of their monorail project.", -15));
        BearEvents.Add(new SpecialEvent("North Haverbrook MONORAIL", "Townsfolk confused by the PowerPoint slide defining what a monorail is.", -5));
        BearEvents.Add(new SpecialEvent("North Haverbrook MONORAIL", "Monorail train cars installed with no brakes.", -10));
        BearEvents.Add(new SpecialEvent("North Haverbrook MONORAIL", "Designer of the monorail train cars blows the whistle on shoddy contruction and cheap materials used to build the North Haverbrook MONORAIL.", -10));

        BearEvents.Add(new SpecialEvent("PG&Enron", "Ah crap we forgot about maintainence for the 50th year in a row!", -15));
        BearEvents.Add(new SpecialEvent("PG&Enron", "Former lead signer of the Dead Kennedys siezes the state capitol and nationalizes the power grid.", -20));
        BearEvents.Add(new SpecialEvent("PG&Enron", "Accounts Payable fat fingered the bank account number for the new auditor. Our books are inspected for the first time in 20 years.", -20));
        BearEvents.Add(new SpecialEvent("PG&Enron", "Earthquake on the San Andreas fault sends half the coastline, and 90% of our customer base, into the Pacific Ocean.", -15));
        BearEvents.Add(new SpecialEvent("PG&Enron", "Another dry winter, another summer of wildfires sparked by faulty transformers. How could we have ever forseen this?", -25));

        BearEvents.Add(new SpecialEvent("My Mutuals and Me Inc.", "You're not my mutual...", -5));
        BearEvents.Add(new SpecialEvent("My Mutuals and Me Inc.", "All firms in the 'Small capital business with no slave labor in their supply chain' fund found to utilize slave labor. Fund dissolved.", -5));
        BearEvents.Add(new SpecialEvent("My Mutuals and Me Inc.", "The NON-STANDARD and RICH index fund found to be too pricey for small investors.", -5));
        BearEvents.Add(new SpecialEvent("My Mutuals and Me Inc.", "New York Teacher's Pension Fund pulls out of My Mutals and Me to put all their money in the Roger Stone Hedge Fund.", -5));
        BearEvents.Add(new SpecialEvent("My Mutuals and Me Inc.", "Robinhood pre-installed on all new iPhones. Americans lose interest in putting their retirement savings in Mutual Funds.", -5));

        BearEvents.Add(new SpecialEvent("Unionized Submissives LLC", "The chastity cage won't stay shut. Everything is just hanging free down there.", -5));
        BearEvents.Add(new SpecialEvent("Unionized Submissives LLC", "If I could save the Union without ungagging any subs, I would do it!", -10));
        BearEvents.Add(new SpecialEvent("Unionized Submissives LLC", "The domme didn't have time to go on a run today. I can barely even smell their feet when they're stepping on my face.", -15));
        BearEvents.Add(new SpecialEvent("Unionized Submissives LLC", "The tiktok teens tried to hex the moon again.", -10));
        BearEvents.Add(new SpecialEvent("Unionized Submissives LLC", "The nipple jumper cables for the car battery are missing.", -10));

        BearEvents.Add(new SpecialEvent("My Pillow, Your Pillow, We're All Pillows!", "My pillow, my pillow, what have ye done!", -5));
        BearEvents.Add(new SpecialEvent("My Pillow, Your Pillow, We're All Pillows!", "Founder and owner convinced by current US president that they should run for Senate.", -10));
        BearEvents.Add(new SpecialEvent("My Pillow, Your Pillow, We're All Pillows!", "Class action lawsuit after its discovered that new cost saving pillow fillings combust with greater intensitiy than gasoline.", -15));
        BearEvents.Add(new SpecialEvent("My Pillow, Your Pillow, We're All Pillows!", "Small manufacturing defect results in hundreds of infants becoming swallowed and trapped within 'My Pillow, Your Pillow, We're All Pillows!'", -15));
        BearEvents.Add(new SpecialEvent("My Pillow, Your Pillow, We're All Pillows!", "Factory in Laos forgot to install suicide nets outside the windows of the worker dormitories.", -5));
    }
}
