using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using TMPro;

public class LobbyListItem : MonoBehaviour
{
    public CSteamID lobbySteamId;
    public string lobbyName;
    public int numberOfYears;
    public int numberOfPlayers;
    public int maxNumberOfPlayers;
    public bool loansEnabled;

    [SerializeField] private Text LobbyNameText;
    [SerializeField] private Text NumberOfYearsText;
    [SerializeField] private Text NumerOfPlayersText;
    [SerializeField] private TextMeshProUGUI lobbyNameTMP;
    [SerializeField] private TextMeshProUGUI numberOfYearsTMP;
    [SerializeField] private TextMeshProUGUI numberOfPlayersTMP;
    [SerializeField] private TextMeshProUGUI loansEnabledTMP;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SetLobbyItemValues()
    {
        /*LobbyNameText.text = lobbyName;
        NumberOfYearsText.text = "Years: " + numberOfYears.ToString();
        NumerOfPlayersText.text = "Players: " + numberOfPlayers.ToString() + "/" + maxNumberOfPlayers.ToString();*/

        lobbyNameTMP.text = lobbyName;
        numberOfYearsTMP.text = "Years: " + numberOfYears.ToString();
        numberOfPlayersTMP.text = "Players: " + numberOfPlayers.ToString() + "/" + maxNumberOfPlayers.ToString();
        if (loansEnabled)
            loansEnabledTMP.text = "Loans: Y";
        else
            loansEnabledTMP.text = "Loans: N";

    }
    public void JoinLobby()
    {
        Debug.Log("JoinLobby: Player selected to join lobby with steam id of: " + lobbySteamId.ToString());
        SteamLobby.instance.JoinLobby(lobbySteamId);
    }
}
