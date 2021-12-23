using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameplayManager : NetworkBehaviour
{
    public static GameplayManager instance;

    [Header("Game Info")]
    [SyncVar(hook = nameof(UpdateCurrentYearText))] public int currentYear = 0;
    [SyncVar(hook = nameof(UpdateMaxYears))] public int maxNumberOfYears;
    [SyncVar(hook = nameof(UpdateBearOrBullText))] public string bearOrBull;
    [SyncVar(hook = nameof(HandleIsThisLastYear))] public bool isThisLastYear;
    [SyncVar] public bool areLoansEnabled;
    bool bankruptThisTurn;
    public bool errorMessageRoutineRunning = false;

    [Header("Special Events")]
    [SerializeField] private TextMeshProUGUI SpecialEventText;
    [SerializeField] private TextMeshProUGUI SpecialEventEffectText;
    [SyncVar(hook = nameof(UpdateSpecialEventDescription))] public string specialEventDescription;
    [SyncVar(hook = nameof(UpdateSpecialEventEffect))] public string specialEventEffect;

    [Header("Local GamePlayers")]
    [SerializeField] private GameObject LocalGamePlayer;
    [SerializeField] private GamePlayer LocalGamePlayerScript;

    [Header("Current Year Text Objects")]
    [SerializeField] private TextMeshProUGUI currentYearText;
    [SerializeField] private TextMeshProUGUI bearOrBullText;

    [Header("Yeehaw!")]
    [SerializeField] private Button YeehawButton;
    [SerializeField] private TextMeshProUGUI YeehawRequestText;
    [SyncVar] public string requestingYeehawPlayer;
    [SyncVar] public ulong requestingYeehawPlayerSteamId;
    [SyncVar] public string givingYeehawPlayer;
    public bool isYeehawRequested;
    private const string canIGetAYeehawButtonText = "Can I Get a Yeehaw?";
    private const string giveYeehawButtonText = "Give Yeehaw";
    private const string requestingYeehawText = " is requesting a Yeehaw!";
    private const string gaveYeehawText = " gave a Yeehaw to ";

    [Header("Stalk Board UI")]
    [SerializeField] private GameObject StalkListContent;

    [Header("Stalks Owned UI")]
    [SerializeField] private GameObject StalksOwnedContent;

    [Header("Player Rankings UI")]
    [SerializeField] private GameObject PlayerRankingsContent;
    //[SerializeField] private PlayerRankingsListItem PlayerRankingItems;
    public List<PlayerRankingsListItem> PlayerRankingItems = new List<PlayerRankingsListItem>();

    [Header("Loans UI Stuff")]
    [SerializeField] private GameObject WouldYouLikeALoanPanel;
    [SerializeField] private GameObject LoanInfoPanel;
    [SerializeField] private TextMeshProUGUI MaxLoanText;
    private const string maxLoanString = "The max loan you can take out is: ";
    [SerializeField] private TextMeshProUGUI MinPaymentText;
    private const string minPaymentString = "The minimum payment for the loan would be: ";
    [SerializeField] private GameObject TakeOutLoanButton;
    [SerializeField] private GameObject EnterLoanAmountPanel;
    [SerializeField] private TMP_InputField RequestLoanInput;
    [SerializeField] private GameObject LoanTakenOutPanel;
    [SerializeField] private TextMeshProUGUI LoanTakenOutText;
    private const string loanTakenOutString = "Congratulations on your loan of: ";
    [SerializeField] private GameObject PayLoanPanel;
    [SerializeField] private TextMeshProUGUI RemainingBalanceText;
    private const string remainingBalanceString1 = "The reamining balance on the loan is <B><I>";
    private const string remainingBalanceString2 = "</I></B> with <B><i>";
    private const string remainingBalanceString3 = "</i></B> years remaining.";
    [SerializeField] private TextMeshProUGUI CurrentMinPaymentText;
    private const string currentMinPaymentString = "Your minimum payment is: <B>";
    private const string currentMinPaymentString2 = " </B> (You must pay <I>at least</I> the minimum.)";
    [SerializeField] private TextMeshProUGUI PenaltiesText;
    private const string penaltiesString = "Penalties for missed payments: 1st missed payment: 10% added to loan balance. 2nd: 15% added. 3rd: All assets siezed and you are bankrupt.\n<B>Your missed payments: ";
    [SerializeField] private TMP_InputField PayLoanInput;


    [Header("Player Stats Text")]
    [SerializeField] private TextMeshProUGUI CashOnHandAmountText;
    [SerializeField] private TextMeshProUGUI NetworthAmountText;

    [Header("Ready Up Panel UI")]
    [SerializeField] private Button ReadyUpButton;

    [Header("Beg. and End Game Messages")]
    [SerializeField] private TextMeshProUGUI EndGameMessageText;
    [SerializeField] private TextMeshProUGUI GoalOfGameText;
    private const string endOfGameMessage = "This is the last year of the game! This is your last opportunity to buy/sell stalks to increase your wealth.Highest net worth wins, with cash on hand as a tie breaker!";

    [Header("Error messages")]
    [SerializeField] private TextMeshProUGUI ErrorMessageText;

    [Header("Game Winner Text Stuff")]
    [SyncVar] public bool isGameWinnerDecided;
    [SerializeField] private GameObject GameOverPanel;
    [SerializeField] private GameObject QuitButton2;
    [SerializeField] private TextMeshProUGUI GameWinnerText;
    [SerializeField] private TextMeshProUGUI DidLocalPlayerWinText;
    private const string andTheWinnerIs = "And the winner is... ";
    private const string localPlayerWon = "<B><I>Congratulations, that's you!!!!</I></B> You're the best at picking stalks! What a useful skill for society!!!";
    private const string localPlayerDidNotWin = "<I>You're terrible at picking stalks!</I> <B>You're fired and your father will never be proud of you!!!</B>";

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
    private static readonly string[] BearOrBull = new[]
    {
        "bear...","bear...","bear...","bear...","bear...","BULL!!!","BULL!!!","BULL!!!","BULL!!!","BULL!!!","BULL!!!","BULL!!!"
    };
    // Start is called before the first frame update
    private void Awake()
    {
        MakeInstance();
        GameOverPanel.SetActive(false);
        QuitButton2.SetActive(false);
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        SetMaxYears();
        SetCurrentYear(0);
        SetBearOrBullText("");
        GetLoansEnabled();
        StalkManager.instance.CreateNewStalks();
    }
    void Start()
    {
        if (isClient)
        {
            GetLocalGamePlayer();
        }
        /*if (isServer)
        {
            SetMaxYears();
        }*/
            
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!isServer)
        {
            ClientUpdateYearText();
        }
    }
    void MakeInstance()
    {
        Debug.Log("GameplayManager MakeInstance.");
        if (instance == null)
            instance = this;
    }
    [Client]
    void GetLocalGamePlayer()
    {
        Debug.Log("GetLocalGamePlayer: Trying to find local game player");
        LocalGamePlayer = GameObject.Find("LocalGamePlayer");
        LocalGamePlayerScript = LocalGamePlayer.GetComponent<GamePlayer>();
        if (LocalGamePlayerScript)
        {
            LocalGamePlayerScript.CmdGetPlayerStartingCashAndNetworth();
            LocalGamePlayerScript.CmdSpawnPlayerRankListItem();
        }
    }
    [Server]
    void SetMaxYears()
    {
        Debug.Log("Executing SetMaxYears on the server.");
        this.UpdateMaxYears(this.maxNumberOfYears, SteamLobby.instance.numberOfYears);
        //this.maxNumberOfYears = SteamLobby.instance.numberOfYears;
    }
    [Server]
    void SetCurrentYear(int yearNumber)
    {
        Debug.Log("Executing SetCurrentYear on the server.");
        this.UpdateCurrentYearText(this.currentYear, yearNumber);
    }
    [Server]
    void SetBearOrBullText(string bearOrBullTextToSet)
    {
        Debug.Log("Executing SetCurrentYear on the server.");
        this.UpdateBearOrBullText(this.bearOrBull, bearOrBullTextToSet);
    }
    [Server]
    void GetLoansEnabled()
    {
        Debug.Log("Executing GetLoansEnabled on the server.");
        areLoansEnabled = SteamLobby.instance.loansEnabled;
    }
    [Client]
    public void AddStalkListItemToBoard(GameObject stalkListItemToAdd)
    {
        Debug.Log("Executing AddStalkListItemToBoard");
        stalkListItemToAdd.transform.SetParent(StalkListContent.transform);
        stalkListItemToAdd.transform.localScale = new Vector3(1f, 1f, 1f);
    }
    [Client]
    public void UpdateLocalPlayersCashOnHand(int cash)
    {
        Debug.Log("Executing UpdateLocalPlayersCashOnHand with cash: " + cash.ToString());
        CashOnHandAmountText.text = cash.ToString("N0");
    }
    [Client]
    public void UpdateLocalPlayersNetworth(int networth)
    {
        Debug.Log("Executing UpdateLocalPlayersNetworth with cash: " + networth.ToString());
        NetworthAmountText.text = networth.ToString("N0");
    }
    [Client]
    public void AddPlayerToRankingsList(GameObject playerRankingToAdd)
    {
        Debug.Log("Executing AddPlayerToRankingsList");
        playerRankingToAdd.transform.SetParent(PlayerRankingsContent.transform);
        playerRankingToAdd.transform.localScale = new Vector3(1f, 1f, 1f);
        PlayerRankingItems.Add(playerRankingToAdd.GetComponent<PlayerRankingsListItem>());
        SortPlayerRankings();
    }
    [Client]
    public void RemovePlayerToRankingsList(PlayerRankingsListItem playerRankingToRemove)
    {
        Debug.Log("Executing RemovePlayerToRankingsList");
        if(PlayerRankingItems.Contains(playerRankingToRemove))
            PlayerRankingItems.Remove(playerRankingToRemove);

        SortPlayerRankings();
    }
    [Client]
    public void SortPlayerRankings()
    {
        Debug.Log("Executing SortPlayerRankings");
        //GameObject[] rankings = GameObject.FindGameObjectsWithTag("PlayerRankingsListItem");
        //GameObject[] rankingsOrder = rankings.OrderByDescending(go => go.GetComponent<PlayerRankingsListItem>().playerNetworth).ToArray();
        PlayerRankingsListItem[] rankingsOrder = PlayerRankingItems.OrderByDescending(x => x.playerNetworth).ToArray();
        for (int i = 0; i < rankingsOrder.Length; i++)
        {
            /*PlayerRankingsListItem playertoSort = rankingsOrder[i].GetComponent<PlayerRankingsListItem>();
            Debug.Log("SortPlayerRankings: player " + playertoSort.playerName + " with net worth " + playertoSort.playerNetworth);
            rankingsOrder[i].transform.SetSiblingIndex(i);*/
            Debug.Log("SortPlayerRankings: player " + rankingsOrder[i].playerName + " with net worth " + rankingsOrder[i].playerNetworth);
            rankingsOrder[i].gameObject.transform.SetSiblingIndex(i);
        }
    }
    void UpdateMaxYears(int oldValue, int newValue)
    {
        if (isServer)
            maxNumberOfYears = newValue;
        if (isClient)
        {
            currentYearText.text = "Year: " + newValue.ToString() + "/" + maxNumberOfYears.ToString();
        }
    }
    void UpdateCurrentYearText(int oldValue, int newValue)
    {
        if (isServer)
            currentYear = newValue;
        if (isClient)
        {
            currentYearText.text = "Year: " + newValue.ToString() +"/" + maxNumberOfYears.ToString();
            if (newValue == 1)
            {
                RemoveStartOfGameText();

            }
            if (newValue > 1 && areLoansEnabled && !LocalGamePlayerScript.doesPlayerHaveLoan && !LocalGamePlayerScript.isPlayerBankrupt)
            {
                ShowLoanTerms(false);
            }
            if (areLoansEnabled && LocalGamePlayerScript.doesPlayerHaveLoan && !LocalGamePlayerScript.isLoanPaidOff && !LocalGamePlayerScript.isPlayerBankrupt)
            {
                ShowLoanPaymentPanel();
            }
                
        }
    }
    void UpdateBearOrBullText(string oldValue, string newValue)
    {
        if (isServer)
            bearOrBull = newValue;
        if (isClient)
        {
            if(newValue.ToLower().Contains("bear"))
                bearOrBullText.text = "<I>" + newValue + "</I>";
            else
                bearOrBullText.text = "<B>" + newValue + "</B>";
        }
    }
    [Client]
    void ClientUpdateYearText()
    {
        Debug.Log("Executing ClientUpdateYearText on the client");
        currentYearText.text = "Year: " + currentYear.ToString() + "/" + maxNumberOfYears.ToString();
        bearOrBullText.text = bearOrBull;
    }
    [Client]
    public void PlayerReadyUp()
    {
        if (LocalGamePlayer)
        {
            Debug.Log("Executing PlayerReadyUp on client for player: " + LocalGamePlayerScript.name);
            LocalGamePlayerScript.ReadyUp();
        }        
    }
    [Client]
    public void UpdateReadyButtonText(bool readyStatus)
    {
        if (readyStatus)
        {
            ReadyUpButton.GetComponentInChildren<Text>().text = "Unready";
        }
        else
        {
            ReadyUpButton.GetComponentInChildren<Text>().text = "Ready for New Year";
        }
            
    }
    [Server]
    public void NewYear()
    {
        Debug.Log("Executing NewYear");
        if (currentYear == 0)
        {
            StalkManager.instance.activateButtons = true;
            StalkManager.instance.CreateStalksOwnedList(Game.GamePlayers);
        }
        bankruptThisTurn = false;
        AdvanceToNewYear();
        GetBearOrBull();
        if (currentYear > maxNumberOfYears)
        {
            Debug.Log("Game over stuff");
        }
        
    }
    [Server]
    void AdvanceToNewYear()
    {
        int newYear = currentYear + 1;
        Debug.Log("AdvanceToNewYear from " + currentYear.ToString() + " to " + newYear.ToString());
        UpdateCurrentYearText(currentYear, newYear);
        if (newYear == maxNumberOfYears)
        {
            Debug.Log("AdvanceToNewYear: The new year will be the last year.");
            HandleIsThisLastYear(isThisLastYear, true);
        }
    }
    [Server]
    void GetBearOrBull()
    {
        var rng = new System.Random();
        string BearBull = BearOrBull[rng.Next(BearOrBull.Length)];
        StalkManager.instance.AdjustStalksForNewYear(BearBull);
        UpdateBearOrBullText(this.bearOrBull, BearBull);
        RpcPlayBearOrBullSounds(BearBull);
    }
    [Server]
    public void SpecialEventForNewYear(string stalkName, string description, int effect)
    {
        if(stalkName == "" && description == "" && effect == 0)
        Debug.Log("SpecialEventForNewYear for stalk " + stalkName + " with effect " + effect.ToString());
        UpdateSpecialEventDescription(specialEventDescription, description);
        string eventEffect = "";
        if (stalkName == "" && description == "" && effect == 0)
        {
            eventEffect = "";
        }
        else
        {
            eventEffect = "<B>" + stalkName + "</B> <I>changed</I> <B>" + effect.ToString() + "</I>";
        }            
        UpdateSpecialEventEffect(specialEventEffect, eventEffect);
    }
    void UpdateSpecialEventDescription(string oldValue, string newValue)
    {
        if (isServer)
            specialEventDescription = newValue;
        if (isClient)
        {
            SpecialEventText.text = newValue;
        }
    }
    void UpdateSpecialEventEffect(string oldValue, string newValue)
    {
        if (isServer)
            specialEventEffect = newValue;
        if (isClient)
        {
            SpecialEventEffectText.text = newValue;
        }
    }
    public void PlayerBuyStalks(string stalkName, int amountToBuy)
    {
        Debug.Log("PlayerBuyStalks: player buying " + amountToBuy.ToString() + " shares of " + stalkName);
        LocalGamePlayerScript.PlayerBuyStalks(stalkName, amountToBuy);
    }
    public void AddNewStalkOwnedListItem(GameObject newStalkOwnedListItem)
    {
        Debug.Log("AddNewStalkOwnedListItem");
        StalkOwnedListItem newStalkOwnedListItemScript = newStalkOwnedListItem.GetComponent<StalkOwnedListItem>();
        if (newStalkOwnedListItemScript.ownerName == LocalGamePlayerScript.PlayerName && newStalkOwnedListItemScript.ownerSteamId == LocalGamePlayerScript.playerSteamId)
        {
            Debug.Log("AddNewStalkOwnedListItem: the newStalkOwnedListItem belongs to the local player. Adding to StalksOwnedContent list");
            newStalkOwnedListItem.transform.SetParent(StalksOwnedContent.transform);
            newStalkOwnedListItem.transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else
        {
            Debug.Log("AddNewStalkOwnedListItem: the newStalkOwnedListItem DOES NOT belong to the local player. Will hide.");
            newStalkOwnedListItem.transform.position = new Vector3(-1000f, -1000f, -1000f);
            newStalkOwnedListItem.SetActive(false);
        }
    }
    void RemoveStartOfGameText()
    {
        Debug.Log("RemoveStartOfGameText");
        EndGameMessageText.gameObject.SetActive(false);
        GoalOfGameText.gameObject.SetActive(false);
    }
    void HandleIsThisLastYear(bool oldValue, bool newValue)
    {
        if (isServer)
            isThisLastYear = newValue;
        if (isClient)
        {
            ActivateLastYearText(newValue);
        }
    }
    void ActivateLastYearText(bool isLastYearOrNot)
    {
        Debug.Log("ActivateLastYearText: " + isLastYearOrNot.ToString());
        if (isLastYearOrNot)
        {
            EndGameMessageText.text = endOfGameMessage;
            EndGameMessageText.gameObject.SetActive(true);
        }
    }
    [ClientRpc]
    public void RpcGameWinnerName(List<GamePlayer> winningPlayers, bool wasTieBroken, bool isThereATie)
    {
        Debug.Log("RpcGameWinnerName with " + winningPlayers.Count.ToString() + " winning players. Was a tie broken: " + wasTieBroken.ToString() + " Is there still a tie: " + isThereATie.ToString());
        //SpecialEventEffectText.gameObject.SetActive(false);
        //SpecialEventText.gameObject.SetActive(false);
        EndGameMessageText.gameObject.SetActive(false);
        ReadyUpButton.gameObject.SetActive(false);
        if (!isThereATie && winningPlayers.Count == 1)
        {
            GameWinnerText.text = andTheWinnerIs + winningPlayers[0].PlayerName;
            if (wasTieBroken)
                GameWinnerText.text += " (cash on hand tie breaker).";
            if (winningPlayers[0].playerSteamId == LocalGamePlayerScript.playerSteamId)
            {
                DidLocalPlayerWinText.text = localPlayerWon;
                //Game.PlayerWonGame();
                SteamAchievementManager.instance.PlayerWonGame();
            }
            else
            {
                DidLocalPlayerWinText.text = localPlayerDidNotWin;
                //Game.PlayerLostGame();
                SteamAchievementManager.instance.PlayerLostGame();
            }                
        }
        else if (isThereATie)
        {
            GameWinnerText.text = andTheWinnerIs + winningPlayers[0].PlayerName;
            for (int i = 1; i < winningPlayers.Count; i++)
            {
                GameWinnerText.text += " and " + winningPlayers[i].PlayerName;
            }
            if (winningPlayers.Any(x => x.playerSteamId == LocalGamePlayerScript.playerSteamId))
            {
                DidLocalPlayerWinText.text = localPlayerWon;
                //Game.PlayerWonGame();
                SteamAchievementManager.instance.PlayerWonGame();
            }
            else
            {
                DidLocalPlayerWinText.text = localPlayerDidNotWin;
                //Game.PlayerLostGame();
                SteamAchievementManager.instance.PlayerLostGame();
            }
                
        }
        GameOverPanel.SetActive(true);
        GameWinnerText.gameObject.SetActive(true);
        DidLocalPlayerWinText.gameObject.SetActive(true);
        SteamAchievementManager.instance.CheckLocalPlayerStatsEndOfGame(LocalGamePlayerScript);
        SteamAchievementManager.instance.NumberOfYearsAchievements(maxNumberOfYears);
    }
    public void QuitGame()
    {
        try
        {
            LocalGamePlayerScript.QuitGame();
        }
        catch (Exception e)
        {
            Debug.Log("LocalGamePlayerScript no longer exists. " + e);
        }
        //LocalGamePlayerScript.QuitGame();
        SceneManager.LoadScene("TitleScreen");
    }
    public void ExitToDesktop()
    {
        Application.Quit();
    }
    public void CloseGameOverPanel()
    {
        GameOverPanel.SetActive(false);
        QuitButton2.SetActive(true);
    }
    [ClientRpc]
    public void RpcPlayBearOrBullSounds(string soundTypeToPlay)
    {
        Debug.Log("RpcPlaySoundOnClient: " + soundTypeToPlay);
        //SoundManager.instance.PlaySound(clipName);
        if (soundTypeToPlay.ToLower().Contains("bear"))
        {
            SoundManager.instance.PlaySound("bear1");
            SoundManager.instance.PlaySound("bear2");
        }
        else if (soundTypeToPlay.ToLower().Contains("bull"))
        {
            SoundManager.instance.PlaySound("bull1");
            SoundManager.instance.PlaySound("bull2");
        }
        else if (soundTypeToPlay.Contains("bankrupt"))
        {
            if (!bankruptThisTurn)
            {
                SoundManager.instance.PlaySound("glassShatter");
                SoundManager.instance.PlaySound("scream");
                bankruptThisTurn = true;
            }
        }
        else if (soundTypeToPlay.Contains("yeehaw"))
        {
            SoundManager.instance.PlaySound("yeehaw");
        }
    }
    public void RequestOrGiveYeehaw()
    {
        if (!isYeehawRequested)
        {
            Debug.Log("RequestOrGiveYeehaw: player requesting yeehaw");
            LocalGamePlayerScript.PlayerRequestOrGiveYeehaw(true);
        }
        else
        {
            Debug.Log("RequestOrGiveYeehaw: player giving yeehaw");
            LocalGamePlayerScript.PlayerRequestOrGiveYeehaw(false);
        }
    }
    public void YeehawRequested(string name, ulong steamId)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            YeehawRequestText.text = name + requestingYeehawText;
            YeehawRequestText.gameObject.SetActive(true);

            if (LocalGamePlayerScript.PlayerName == name && LocalGamePlayerScript.playerSteamId == steamId)
                YeehawButton.gameObject.SetActive(false);

            YeehawButton.GetComponentInChildren<Text>().text = giveYeehawButtonText;
            isYeehawRequested = true;
        }
    }
    public void YeehawGiven(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            RpcPlayBearOrBullSounds("yeehaw");
            YeehawRequestText.text = name + gaveYeehawText + requestingYeehawPlayer;
            YeehawButton.GetComponentInChildren<Text>().text = canIGetAYeehawButtonText;
            YeehawButton.gameObject.SetActive(true);
            isYeehawRequested = false;
        }
    }
    public void UpdateMaxPlayerCanAfford()
    {
        try
        {
            LocalGamePlayerScript.GetMaxPlayerCanAfford(LocalGamePlayerScript.cashOnHand);
        }
        catch (Exception e)
        {
            Debug.Log("UpdateMaxPlayerCanAfford failed to execute LocalGamePlayerScript.GetMaxPlayerCanAfford(LocalGamePlayerScript.cashOnHand) " + e);
        }            
    }
    void DisplayWouldYouLikeALoan()
    {
        WouldYouLikeALoanPanel.SetActive(true);
    }
    public void ShowLoanTermsButtonPressed()
    {
        ShowLoanTerms(true);
        LocalGamePlayerScript.GetLoanTermsForPlayer();
    }
    public void ShowLoanTerms(bool showTerms)
    {
        Debug.Log("ShowLoanTerms: " + showTerms.ToString());

        if (currentYear > maxNumberOfYears)
        {
            Debug.Log("ShowLoanTerms: game is over. Hiding all of the loan stuff.");
            WouldYouLikeALoanPanel.SetActive(false);
            LoanInfoPanel.SetActive(false);
            TakeOutLoanButton.SetActive(false);
            EnterLoanAmountPanel.SetActive(false);
            LoanTakenOutPanel.SetActive(false);
            return;
        }

        if (showTerms)
        {
            WouldYouLikeALoanPanel.SetActive(false);
            LoanInfoPanel.SetActive(true);
            TakeOutLoanButton.SetActive(true);
            EnterLoanAmountPanel.SetActive(false);
            LoanTakenOutPanel.SetActive(false);
        }
        else
        {
            WouldYouLikeALoanPanel.SetActive(true);
            LoanInfoPanel.SetActive(false);
            TakeOutLoanButton.SetActive(false);
            EnterLoanAmountPanel.SetActive(false);
            LoanTakenOutPanel.SetActive(false);
        }

        
    }
    public void UpdateMaxLoanAmountText(int newMaxLoanAmount)
    {
        Debug.Log("UpdateMaxLoanAmountText for " + newMaxLoanAmount.ToString());
        MaxLoanText.text = maxLoanString + newMaxLoanAmount.ToString("N0");
        RequestLoanInput.text = "";
        RequestLoanInput.placeholder.GetComponent<TextMeshProUGUI>().text = newMaxLoanAmount.ToString();
    }
    public void UpdateMinPaymentAmountText(int newMinPaymentAmount)
    {
        Debug.Log("UpdateMinPaymentAmountText for " + newMinPaymentAmount.ToString());
        MinPaymentText.text = minPaymentString + newMinPaymentAmount.ToString("N0");
    }
    public void TakeOutLoanButtonPressed()
    {
        Debug.Log("TakeOutLoanButtonPressed");
        TakeOutLoanButton.gameObject.SetActive(false);
        EnterLoanAmountPanel.SetActive(true);
    }
    public void RequestLoanForPlayer()
    {
        Debug.Log("RequestLoanForPlayer");
        int requestedLoanAmount = 0;
        Int32.TryParse(RequestLoanInput.text, out requestedLoanAmount);
        if (requestedLoanAmount > 0)
        {
            Debug.Log("RequestLoanForPlayer: Player is requesting loan for " + requestedLoanAmount.ToString());
            LocalGamePlayerScript.RequestLoan(requestedLoanAmount);
        }
    }
    public void LoanTakenOut(int loanAmount)
    {
        LoanInfoPanel.SetActive(true);
        TakeOutLoanButton.SetActive(false);
        EnterLoanAmountPanel.SetActive(false);
        LoanTakenOutPanel.SetActive(true);
        LoanTakenOutText.text = loanTakenOutString + loanAmount.ToString("N0");
    }
    void ShowLoanPaymentPanel()
    {
        WouldYouLikeALoanPanel.SetActive(false);
        LoanInfoPanel.SetActive(false);
        PayLoanPanel.SetActive(true);
    }
    public void HideLoanPaymentPanel()
    {
        PayLoanPanel.SetActive(false);
    }
    public void NewRemainingBalance(int balance, int years)
    {
        Debug.Log("NewRemainingBalance with " + balance.ToString() + " with " + years.ToString() + " years remaining");
        RemainingBalanceText.text = remainingBalanceString1 + balance.ToString("N0") + remainingBalanceString2 + years.ToString() + remainingBalanceString3;
    }
    public void NewMinPayment(int payment)
    {
        Debug.Log("NewMinPayment of " + payment.ToString());
        CurrentMinPaymentText.text = currentMinPaymentString + payment.ToString() + currentMinPaymentString2;
        PayLoanInput.text = "";
        PayLoanInput.placeholder.GetComponent<TextMeshProUGUI>().text = payment.ToString();
    }
    public void MakeLoanPayment()
    {
        Debug.Log("MakeLoanPayment");
        int paymentAmount = 0;
        Int32.TryParse(PayLoanInput.text, out paymentAmount);
        if (paymentAmount > 0)
        {
            LocalGamePlayerScript.PayLoan(paymentAmount);
        }
    }
    public void HideAllLoanPanels()
    {
        WouldYouLikeALoanPanel.SetActive(false);
        LoanInfoPanel.SetActive(false);
        PayLoanPanel.SetActive(false);
        TakeOutLoanButton.SetActive(false);
        EnterLoanAmountPanel.SetActive(false);
        LoanTakenOutPanel.SetActive(false);
    }
    public void NewPenaltiesText(int missed)
    {
        PenaltiesText.text = penaltiesString + missed.ToString();
    }

    public IEnumerator ShowErrorMessage(string errorMessage)
    {
        errorMessageRoutineRunning = true;
        ErrorMessageText.text = errorMessage;
        ErrorMessageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3.5f);
        ErrorMessageText.gameObject.SetActive(false);
        errorMessageRoutineRunning = false;
    }
}
