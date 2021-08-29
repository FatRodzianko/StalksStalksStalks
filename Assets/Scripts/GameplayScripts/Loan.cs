using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Loan : NetworkBehaviour
{
    [Header("Loan Info")]
    [SyncVar] public string ownerName;
    [SyncVar] public ulong ownerSteamId;
    [SyncVar] public uint ownerNetId;
    [SyncVar(hook = nameof(UpdateLoanBalance))] public int loanBalance;
    [SyncVar(hook = nameof(UpdateMinPayment))] public int minPayment;
    [SyncVar(hook = nameof(UpdateYearsRemaining))] public int yearsRemaining;
    [SyncVar(hook = nameof(HandlePaidThisYear))] public bool paidThisYear;
    [SyncVar(hook = nameof(UpdateMissedPayments))] public int missedPayments;
    public GamePlayer myOwnerScript;

    public override void OnStartClient()
    {
        base.OnStartClient();
        FindLoanOwner();
        if (hasAuthority)
        {
            try
            {
                GameplayManager.instance.LoanTakenOut(loanBalance);
                GameplayManager.instance.NewRemainingBalance(loanBalance, yearsRemaining);
                GameplayManager.instance.NewMinPayment(minPayment);
            }
            catch (Exception e)
            {
                Debug.Log("Loan: Failed to execute GameplayManager.instance.LoanTakenOut(loanBalance). " + e);
            }
        }
            
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void FindLoanOwner()
    {
        try
        {
            myOwnerScript = NetworkIdentity.spawned[ownerNetId].gameObject.GetComponent<GamePlayer>();
            myOwnerScript.myLoanObject = this.gameObject;
            myOwnerScript.myLoanScript = this;
            this.transform.SetParent(myOwnerScript.gameObject.transform);
        }
        catch (Exception e)
        {
            Debug.Log("FindLoanOwner: failed to find owner. " + e);
        }
    }
    public void UpdateLoanBalance(int oldValue, int newValue)
    {
        Debug.Log("UpdateLoanBalance " + newValue.ToString());
        if (isServer)
            loanBalance = newValue;
        if (isClient)
        {
            if (hasAuthority)
            {
                GameplayManager.instance.NewRemainingBalance(newValue, this.yearsRemaining);
            }
        }
    }
    public void UpdateMinPayment(int oldValue, int newValue)
    {
        Debug.Log("UpdateMinPayment " + newValue.ToString());
        if (isServer)
            minPayment = newValue;
        if (isClient)
        {
            if (hasAuthority)
            {
                GameplayManager.instance.NewMinPayment(newValue);
            }
        }
    }
    public void UpdateYearsRemaining(int oldValue, int newValue)
    {
        Debug.Log("UpdateYearsRemaining " + newValue.ToString());
        if (isServer)
            yearsRemaining = newValue;
        if (isClient)
        {
            if (hasAuthority)
            {
                GameplayManager.instance.NewRemainingBalance(this.loanBalance, newValue);
            }
        }
    }
    public void HandlePaidThisYear(bool oldValue, bool newValue)
    {
        if (isServer)
            paidThisYear = newValue;
        if (isClient)
        {
            if (hasAuthority)
            {
                if (newValue)
                {
                    GameplayManager.instance.HideLoanPaymentPanel();
                }
            }
        }
    }
    public void UpdateMissedPayments(int oldValue, int newValue)
    {
        if (isServer)
            missedPayments = newValue;
        if (isClient)
        {
            if (hasAuthority)
                GameplayManager.instance.NewPenaltiesText(newValue);
        }
    }
    private void OnDestroy()
    {
        try
        {
            StalkManager.instance.allLoans.Remove(this);
            if(hasAuthority)
                GameplayManager.instance.HideLoanPaymentPanel();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        
    }
}
