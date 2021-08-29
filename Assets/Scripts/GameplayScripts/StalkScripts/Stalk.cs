using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Stalk : NetworkBehaviour
{
    [Header("Stalk Info")]
    [SyncVar] public string stalkName;
    [SyncVar(hook = nameof(UpdatePricePerShare))] public int pricePerShare;
    [SyncVar(hook = nameof(UpdateYearlyChange))] public int YearlyChange;
    [SyncVar] public int minChangeBear;
    [SyncVar] public int maxChangeBear;
    [SyncVar] public int minChangeBull;
    [SyncVar] public int maxChangeBull;
    [SyncVar] public int bearDividend;
    [SyncVar] public int bullDividend;
    [SyncVar(hook = nameof(HandleDidStalkSplit))] public bool didStalkSplit;
    [SyncVar(hook = nameof(HandleIsBankrupt))] public bool isBankrupt;

    [Header("Stalk List Item")]
    [SerializeField] private GameObject stalkListItemPrefab;
    public GameObject myStalkListItemObject;
    public StalkListItem myStalkListItemScript;

    // Start is called before the first frame update
    public override void OnStartClient()
    {
        Debug.Log("OnStartClient for stalk: " + stalkName);
        base.OnStartClient();
        GameObject stalkManager = GameObject.FindGameObjectWithTag("StalkManager");
        this.transform.SetParent(stalkManager.transform);
        CreateStalkListItem();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [Client]
    void CreateStalkListItem()
    {
        Debug.Log("Executing CreateStalkListItem on a client");
        GameObject newStalkListItem = Instantiate(stalkListItemPrefab, transform.position, Quaternion.identity);
        myStalkListItemObject = newStalkListItem;
        myStalkListItemScript = newStalkListItem.GetComponent<StalkListItem>();
        myStalkListItemScript.stalkName = this.stalkName;
        myStalkListItemScript.pricePerShare = this.pricePerShare;
        myStalkListItemScript.yearlyChange = this.YearlyChange;
        myStalkListItemScript.bullDividend = this.bullDividend;
        myStalkListItemScript.bearDividend = this.bearDividend;
        myStalkListItemScript.SetStalkListItemInfo();
        GameplayManager.instance.AddStalkListItemToBoard(myStalkListItemObject);
    }
    public void UpdatePricePerShare(int oldValue, int newValue)
    {
        if (isServer)
            pricePerShare = newValue;
        if (isClient)
        {
            Debug.Log("Updating " + this.stalkName + " to new price of " + newValue.ToString());
            myStalkListItemScript.SetStalkPricePerShare(newValue);
            
        }
    }
    public void UpdateYearlyChange(int oldValue, int newValue)
    {
        if (isServer)
            YearlyChange = newValue;
        if (isClient)
        {
            Debug.Log("Updating " + this.stalkName + " to YearlyChange of " + newValue.ToString());
            myStalkListItemScript.SetStalkYearlyChange(newValue);
        }
    }
    public void HandleIsBankrupt(bool oldValue, bool newValue)
    {
        if (isServer)
            isBankrupt = newValue;
        if (isClient)
        {
            if (newValue)
            {
                myStalkListItemScript.BankruptStalk(newValue);
            }
        }
    }
    public void HandleDidStalkSplit(bool oldValue, bool newValue)
    {
        if (isServer)
            didStalkSplit = newValue;
        if (isClient)
        {
            myStalkListItemScript.SplitStalkText(newValue);
        }
    }
}
