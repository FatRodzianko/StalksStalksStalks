using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;
using TMPro;
using System;

public class MainMenuManager : MonoBehaviour
{

    [Header("Create Lobby UI")]
    //[SerializeField] private GameObject CreateLobbyCanvas;
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private TMP_InputField numberOfYearsInputField;
    [SerializeField] private TMP_InputField maxNumberOfPlayersInputField;
    [SerializeField] private Toggle friendsOnlyToggle;
    [SerializeField] private Toggle enableLoansToggle;
    public bool didPlayerNameTheLobby = false;
    public string lobbyName;
    [SerializeField] private TMP_InputField startingWealthInputField;
    [Header("Lobby List UI")]
    [SerializeField] private GameObject LobbyListItemPrefab;
    [SerializeField] private GameObject ContentPanel;
    [SerializeField] private GameObject LobbyListScrollRect;
    [SerializeField] private TMP_InputField searchBox;
    public bool didPlayerSearchForLobbies = false;

    public List<GameObject> listOfLobbyListItems = new List<GameObject>();

    public static MainMenuManager instance;

    [SerializeField] GameObject ThanksForSupport;
    // Start is called before the first frame update
    void Awake()
    {
        MakeInstance();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
    }
    private void Start()
    {
        if (System.IO.File.Exists("support-dlc.txt"))
        {
            ThanksForSupport.SetActive(true);
        }
        else
            ThanksForSupport.SetActive(false);
    }
    public void CreateNewLobby()
    {
        ELobbyType newLobbyType;
        if (friendsOnlyToggle.isOn)
        {
            Debug.Log("CreateNewLobby: friendsOnlyToggle is on. Making lobby friends only.");
            newLobbyType = ELobbyType.k_ELobbyTypeFriendsOnly;
        }
        else
        {
            Debug.Log("CreateNewLobby: friendsOnlyToggle is OFF. Making lobby public.");
            newLobbyType = ELobbyType.k_ELobbyTypePublic;
        }

        if (!string.IsNullOrEmpty(lobbyNameInputField.text))
        {
            Debug.Log("CreateNewLobby: player created a lobby name of: " + lobbyNameInputField.text);
            didPlayerNameTheLobby = true;
            lobbyName = lobbyNameInputField.text;
        }

        int numberOfYearsForLobby = 0;
        Int32.TryParse(numberOfYearsInputField.text, out numberOfYearsForLobby);
        if (string.IsNullOrWhiteSpace(numberOfYearsInputField.text))
        {
            Debug.Log("CreateNewLobby: Using default number of years of 10.");
            numberOfYearsForLobby = 10;
        }
        else if (!(numberOfYearsForLobby > 1 && numberOfYearsForLobby < 100))
        {
            Debug.Log("CreateNewLobby: player did not provide numberOfYearsForLobby");
            return;
        }
        int maxNumberOfPlayers = 0;
        Int32.TryParse(maxNumberOfPlayersInputField.text, out maxNumberOfPlayers);
        if (string.IsNullOrWhiteSpace(maxNumberOfPlayersInputField.text))
        {
            Debug.Log("Using default number of players of 4.");
            maxNumberOfPlayers = 4;
        }
        else if (!(maxNumberOfPlayers > 0 && maxNumberOfPlayers < 11))
        {
            Debug.Log("CreateNewLobby: CreateNewLobby: player did not provide maxNumberOfPlayers");
            return;
        }
        bool loansEnabled = false;
        if (enableLoansToggle.isOn)
            loansEnabled = true;
        else
            loansEnabled = false;
        //Get starting wealth from player input
        int playerStartingWealth = 0;
        Int32.TryParse(startingWealthInputField.text, out playerStartingWealth);
        if (string.IsNullOrWhiteSpace(startingWealthInputField.text))
        {
            Debug.Log("CreateNewLobby: Using default starting wealth of 10,000");
            playerStartingWealth = 10000;
        }
        else if (!(playerStartingWealth > 149 && playerStartingWealth < 69421))
        {
            Debug.Log("CreateNewLobby: player did not provide valid number for player starting wealth");
            return;
        }

        SteamLobby.instance.CreateNewLobby(newLobbyType, maxNumberOfPlayers, numberOfYearsForLobby, loansEnabled, playerStartingWealth);
    }
    public void GetListOfLobbies()
    {
        Debug.Log("Trying to get list of available lobbies ...");
        if (listOfLobbyListItems.Count > 0)
        {
            Debug.Log("GetListOfLobbies: Already have list of Lobbies. Don't need to get it again unless player refreshes the list");
        }
        else
        {
            Debug.Log("GetListOfLobbies: No lobby list. Getting list of lobbies.");
            SteamLobby.instance.GetListOfLobbies();
        }
        
    }
    public void DisplayLobbies(List<CSteamID> lobbyIDS, LobbyDataUpdate_t result)
    {
        for (int i = 0; i < lobbyIDS.Count; i++)
        {
            if (lobbyIDS[i].m_SteamID == result.m_ulSteamIDLobby)
            {
                Debug.Log("Lobby " + i + " :: " + SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "name") + " number of players: " + SteamMatchmaking.GetNumLobbyMembers((CSteamID)lobbyIDS[i].m_SteamID).ToString() + " max players: " + SteamMatchmaking.GetLobbyMemberLimit((CSteamID)lobbyIDS[i].m_SteamID).ToString());

                //if (SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "GameName").Equals("CardConquest"))
                if (SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "GameName").Equals("StalksStalksStalks"))
                {
                    if (didPlayerSearchForLobbies)
                    {
                        Debug.Log("OnGetLobbyInfo: Player searched for lobbies");
                        if (SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "name").ToLower().Contains(searchBox.text.ToLower()))
                        {
                            GameObject newLobbyListItem = Instantiate(LobbyListItemPrefab) as GameObject;
                            LobbyListItem newLobbyListItemScript = newLobbyListItem.GetComponent<LobbyListItem>();

                            newLobbyListItemScript.lobbySteamId = (CSteamID)lobbyIDS[i].m_SteamID;
                            newLobbyListItemScript.lobbyName = SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "name");
                            newLobbyListItemScript.numberOfPlayers = SteamMatchmaking.GetNumLobbyMembers((CSteamID)lobbyIDS[i].m_SteamID);
                            newLobbyListItemScript.maxNumberOfPlayers = SteamMatchmaking.GetLobbyMemberLimit((CSteamID)lobbyIDS[i].m_SteamID);

                            int years = 0;
                            Int32.TryParse(SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "NumberOfYears"), out years);
                            Debug.Log("DisplayLobbies: Number of years was: " + years.ToString());
                            newLobbyListItemScript.numberOfYears = years;

                            bool areLoansEnabled = false;
                            Boolean.TryParse(SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "LoansEnabled"), out areLoansEnabled);
                            newLobbyListItemScript.loansEnabled = areLoansEnabled;

                            newLobbyListItemScript.SetLobbyItemValues();


                            newLobbyListItem.transform.SetParent(ContentPanel.transform);
                            newLobbyListItem.transform.localScale = Vector3.one;

                            listOfLobbyListItems.Add(newLobbyListItem);
                        }
                    }
                    else
                    {
                        Debug.Log("OnGetLobbyInfo: Player DID NOT search for lobbies");
                        GameObject newLobbyListItem = Instantiate(LobbyListItemPrefab) as GameObject;
                        LobbyListItem newLobbyListItemScript = newLobbyListItem.GetComponent<LobbyListItem>();

                        newLobbyListItemScript.lobbySteamId = (CSteamID)lobbyIDS[i].m_SteamID;
                        newLobbyListItemScript.lobbyName = SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "name");
                        newLobbyListItemScript.numberOfPlayers = SteamMatchmaking.GetNumLobbyMembers((CSteamID)lobbyIDS[i].m_SteamID);
                        newLobbyListItemScript.maxNumberOfPlayers = SteamMatchmaking.GetLobbyMemberLimit((CSteamID)lobbyIDS[i].m_SteamID);
                        
                        int years = 0;
                        Int32.TryParse(SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "NumberOfYears"), out years);
                        Debug.Log("DisplayLobbies: Number of years was: " + years.ToString());
                        newLobbyListItemScript.numberOfYears = years;

                        bool areLoansEnabled = false;
                        Boolean.TryParse(SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "LoansEnabled"), out areLoansEnabled);
                        newLobbyListItemScript.loansEnabled = areLoansEnabled;

                        newLobbyListItemScript.SetLobbyItemValues();

                        newLobbyListItem.transform.SetParent(ContentPanel.transform);
                        newLobbyListItem.transform.localScale = Vector3.one;

                        listOfLobbyListItems.Add(newLobbyListItem);
                    }
                }

                return;
            }
        }
        if (didPlayerSearchForLobbies)
            didPlayerSearchForLobbies = false;
    }
    public void DestroyOldLobbyListItems()
    {
        Debug.Log("DestroyOldLobbyListItems");
        foreach (GameObject lobbyListItem in listOfLobbyListItems)
        {
            GameObject lobbyListItemToDestroy = lobbyListItem;
            Destroy(lobbyListItemToDestroy);
            lobbyListItemToDestroy = null;
        }
        listOfLobbyListItems.Clear();
    }
    public void RefreshLobbyList()
    {
        Debug.Log("RefreshLobbyList");
        SteamLobby.instance.GetListOfLobbies();
    }
    public void SearchForLobby()
    {
        Debug.Log("SearchForLobby");
        if (!string.IsNullOrEmpty(searchBox.text))
        {
            didPlayerSearchForLobbies = true;
        }
        else
            didPlayerSearchForLobbies = false;
        SteamLobby.instance.GetListOfLobbies();
    }
    public void QuitToDesktop()
    {
        Application.Quit();
    }
}
