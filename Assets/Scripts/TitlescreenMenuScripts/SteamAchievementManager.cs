using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Mirror;

public class SteamAchievementManager : MonoBehaviour
{
    public static SteamAchievementManager instance;
    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        MakeInstance();
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
    }
    private void Start()
    {
        //SteamUserStats.ResetAllStats(true);

    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlayerWonGame()
    {
        if (!SteamManager.Initialized)
            Debug.Log("No steam manager???");
        else
            Debug.Log("YES steam manager!!!");
        Debug.Log("SteamAchievementManager: PlayerWonGame");
        /*bool hasPlayerWonGameYet = false;
        SteamUserStats.GetAchievement("SSS_Win_One_Game", out hasPlayerWonGameYet);
        if (!hasPlayerWonGameYet)
        {
            SteamUserStats.SetAchievement("SSS_Win_One_Game");
            SteamUserStats.StoreStats();
            Debug.Log("SteamAchievementManager: Submitted achievement for SSS_Win_One_Game");
        }*/
        //SteamUserStats.SetAchievement("SSS_WIN_ONE_GAME");
        //SteamUserStats.SetAchievement("SSS_Win_One_Game");
        //SteamUserStats.StoreStats();
        Steamworks.SteamUserStats.SetAchievement("SSS_Win_One_Game");
        Steamworks.SteamUserStats.StoreStats();

    }
    public void PlayerLostGame()
    {
        if (!SteamManager.Initialized)
            Debug.Log("No steam manager???");
        else
            Debug.Log("YES steam manager!!!");
        Debug.Log("SteamAchievementManager: PlayerLostGame");
        bool hasPlayerWonGameYet = false;
        SteamUserStats.GetAchievement("SSS_LOSE_ONE_GAME", out hasPlayerWonGameYet);
        if (!hasPlayerWonGameYet)
        {
            SteamUserStats.SetAchievement("SSS_LOSE_ONE_GAME");
            SteamUserStats.SetAchievement("SSS_Lose_One_Game");
            SteamUserStats.StoreStats();
            Debug.Log("SteamAchievementManager: Submitted achievement for SSS_Lose_One_Game");
        }
    }
    public void CheckLocalPlayerStatsEndOfGame(GamePlayer localPlayer)
    {
        bool didPlayerGetAchievement = false;
        SteamUserStats.RequestCurrentStats();
        if (localPlayer.netWorth >= 50000)
        {
            didPlayerGetAchievement = true;
            SteamUserStats.SetAchievement("SSS_NETWORTH_FIFTY");
        }
        if (localPlayer.netWorth <= 100)
        {
            didPlayerGetAchievement = true;
            SteamUserStats.SetAchievement("SSS_NETWORTH_HUNDRED");
        }
        int liftimeNetWorth = 0;
        SteamUserStats.GetStat("lifetime_networth", out liftimeNetWorth);
        if (liftimeNetWorth < 1000000)
        {
            liftimeNetWorth += localPlayer.netWorth;
            SteamUserStats.SetStat("lifetime_networth", liftimeNetWorth);            
            Debug.Log("CheckLocalPlayerStatsEndOfGame: Setting liftime net worth to " + liftimeNetWorth.ToString());
            didPlayerGetAchievement = true;
            if(liftimeNetWorth >= 1000000)
                SteamUserStats.SetAchievement("SSS_ONE_MIL");
        }
        else if (liftimeNetWorth >= 1000000 && liftimeNetWorth < 1000000000)
        {
            liftimeNetWorth += localPlayer.netWorth;
            SteamUserStats.SetStat("lifetime_networth", liftimeNetWorth);
            didPlayerGetAchievement = true;
            Debug.Log("CheckLocalPlayerStatsEndOfGame: Setting liftime net worth to " + liftimeNetWorth.ToString());
            if (liftimeNetWorth >= 1000000000)
                SteamUserStats.SetAchievement("SSS_ONE_BIL");
        }
        if (localPlayer.netWorth >= 20000)
        {
            didPlayerGetAchievement = true;
            SteamUserStats.SetAchievement("SSS_DOUBLED");
        }
        if (localPlayer.netWorth <= 5000)
        {
            didPlayerGetAchievement = true;
            SteamUserStats.SetAchievement("SSS_HALVED");
        }
        if (didPlayerGetAchievement)
        {
            SteamUserStats.StoreStats();
            Debug.Log("CheckLocalPlayerStatsEndOfGame: storestats() called");
        }
            
    }
    public void NumberOfYearsAchievements(int years)
    {
        if (years >= 30)
        {
            SteamUserStats.SetAchievement("SSS_MORTGAGE");
            SteamUserStats.StoreStats();
            Debug.Log("NumberOfYearsAchievements: Mortgage");
        }
    }
    public void TookOutLoan()
    {
        SteamUserStats.SetAchievement("SSS_TOOK_OUT_LOAN");
        SteamUserStats.StoreStats();
    }
    public void DefaultedOnLoan()
    {
        SteamUserStats.SetAchievement("SSS_DEFAULT_LOAN");
        SteamUserStats.StoreStats();
    }
    public void PaidOffLoan()
    {
        SteamUserStats.SetAchievement("SSS_PAID_LOAN");
        SteamUserStats.StoreStats();
    }

}
