using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StalkListItem : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI StalkNameText;
    [SerializeField] private TextMeshProUGUI PricePerShareText;
    [SerializeField] private TextMeshProUGUI BullDividendText;
    [SerializeField] private TextMeshProUGUI BearDividendText;
    [SerializeField] private TextMeshProUGUI YearlyChangeText;
    [SerializeField] private Image YearlyChangeImage;
    [SerializeField] private TMP_InputField BuyInputField;
    [SerializeField] private Button BuyButton;
    [SerializeField] private GameObject ShareDetailsPanel;
    [SerializeField] private GameObject BankruptPanel;

    [Header("Yearly Change Sprites")]
    [SerializeField] private Sprite positiveChange;
    [SerializeField] private Sprite negativeChange;
    [SerializeField] private Sprite noChange;

    [Header("StalkListItem Info")]
    public string stalkName;
    public int pricePerShare;
    public int bullDividend;
    public int bearDividend;
    public int yearlyChange;
    public bool bankrupt;
    // Start is called before the first frame update
    void Awake()
    {
        BuyInputField.gameObject.SetActive(false);
        BuyButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetStalkListItemInfo()
    {
        Debug.Log("Executing SetStalkListItemInfo");
        StalkNameText.text = stalkName;
        PricePerShareText.text = pricePerShare.ToString();
        BullDividendText.text = bullDividend.ToString();
        BearDividendText.text = bearDividend.ToString();
        SetYearlyChange(yearlyChange);
    }
    void SetYearlyChange(int change)
    {
        Debug.Log("Executing SetYearlyChange");
        if (change > 0)
        {
            Debug.Log("SetYearlyChange: Positive change");
            YearlyChangeText.text = "+" + yearlyChange.ToString();
            YearlyChangeImage.sprite = positiveChange;
        }
        else if (change < 0)
        {
            Debug.Log("SetYearlyChange: negative change");
            YearlyChangeText.text = yearlyChange.ToString();
            YearlyChangeImage.sprite = negativeChange;
        }
        else if (change == 0)
        {
            Debug.Log("SetYearlyChange: NO change");
            YearlyChangeText.text = yearlyChange.ToString();
            YearlyChangeImage.sprite = noChange;
        }
        try
        {
            GameplayManager.instance.UpdateMaxPlayerCanAfford();
        }
        catch (Exception e)
        {
            Debug.Log("SetYearlyChange: GameplayManager.instance.UpdateMaxPlayerCanAfford() failed. " + e);
        }

    }
    public void ActivateBuyButtons()
    {
        BuyInputField.gameObject.SetActive(true);
        BuyButton.gameObject.SetActive(true);
    }
    public void DeActivateBuyButtons()
    {
        BuyInputField.gameObject.SetActive(false);
        BuyButton.gameObject.SetActive(false);
    }
    public void SetStalkPricePerShare(int newValue)
    {
        pricePerShare = newValue;
        PricePerShareText.text = pricePerShare.ToString();
        try
        {
            GameplayManager.instance.UpdateMaxPlayerCanAfford();
        }
        catch (Exception e)
        {
            Debug.Log("SetYearlyChange: GameplayManager.instance.UpdateMaxPlayerCanAfford() failed. " + e);
        }
    }
    public void SetStalkYearlyChange(int newValue)
    {
        yearlyChange = newValue;
        SetYearlyChange(yearlyChange);
    }
    public void BuyStalk()
    {
        int amountToBuy = 0;
        Int32.TryParse(BuyInputField.text, out amountToBuy);
        Debug.Log("BuyStalk: player is trying to buy " + amountToBuy.ToString() + " shares of " + stalkName);
        if (amountToBuy > 0)
        {
            GameplayManager.instance.PlayerBuyStalks(stalkName, amountToBuy);
        }
    }
    public void BankruptStalk(bool isBankrupt)
    {
        Debug.Log("BankruptStalk on StalkListItem");
        if (isBankrupt)
        {
            ShareDetailsPanel.SetActive(false);
            BankruptPanel.SetActive(true);
            bankrupt = true;
        }
    }
    public void SplitStalkText(bool didStalkSplit)
    {
        if (didStalkSplit)
        {
            PricePerShareText.text = "<I><B>SPLIT!!!</B></I>\n" + pricePerShare.ToString();
        }
    }
    public void MaxPlayerCanAfford(int cashOnHand)
    {
        Debug.Log("MaxPlayerCanAfford for stalk " + stalkName + " price of " + this.pricePerShare.ToString() + " from cash " + cashOnHand.ToString());
        if (!bankrupt)
        {
            BuyInputField.text = "";
            int maxPlayerCanAfford = 0;
            maxPlayerCanAfford = (cashOnHand / pricePerShare);
            BuyInputField.placeholder.GetComponent<TextMeshProUGUI>().text = maxPlayerCanAfford.ToString();
        }        
    }
}
