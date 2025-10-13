using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using TMPro;
using UnityEngine;

public class ExchangeView : BaseView
{
    [SerializeField] private GameObject tabExchange, tabHistory, imageSelectedTabExchange, imageSelectedTabHistory;
    [SerializeField] private TMP_InputField idInputField, confirmIdInputField, cashInputField;
    [SerializeField] private TextMeshProUGUI textYourChip, textAccountChip;
    [SerializeField] private ExchangeDealItem exchangeDealItemPrefab;
    [SerializeField] private ExchangeHistoryItem exchangeHistoryItemPrefab;
    [SerializeField] private Transform exchangeDealItemParent, exchangeHistoryItemParent;
    private ExchangePresenter exchangePresenter;
    private List<Deal> listDeal;
    private string selectedDealId;

    protected override void Awake()
    {
        base.Awake();
        exchangePresenter = new ExchangePresenter();
        exchangePresenter.Init(this);

        GetListExchangeDeal();
        GetListExchangeHistory();
        textYourChip.text = "Your chips: " + Utility.FormatNumber(User.userProfile.AccountChip);
        textAccountChip.text = Utility.FormatNumber(User.userProfile.AccountChip);
    }

    public async void GetListExchangeDeal()
    {
        try
        {
            ExchangeDealInShop exchangeDealInShop = await exchangePresenter.GetListExchangeDeal();
            UIManager.Instance.HideProgressing();
            listDeal = exchangeDealInShop.Gcashes.ToList();
            UpdateUIListExchangeDeal();
            Debug.Log("DEAL LIST: " + listDeal.ToString());
        }
        catch (Exception ex)
        {
            UIManager.Instance.ShowAlertDialog("Fail to get list deal");
            // throw;
        }
    }

    public async void GetListExchangeHistory()
    {
        try
        {
            ListExchangeInfo exchangeDealInShop = await exchangePresenter.GetListExchange();
            UIManager.Instance.HideProgressing();
            UpdateUIListExchangeDeal();
            Debug.Log("DEAL LIST: " + listDeal.ToString());
        }
        catch (Exception ex)
        {
            UIManager.Instance.ShowAlertDialog("Fail to get list deal");
            // throw;
        }
    }

    private void UpdateUIListExchangeDeal()
    {
        foreach (Transform child in exchangeDealItemParent)
        {
            Destroy(child.gameObject);
        }
        foreach (Deal deal in listDeal)
        {
            ExchangeDealItem exchangeDealItem = Instantiate(exchangeDealItemPrefab, exchangeDealItemParent);
            exchangeDealItem.SetInfo(deal);
            exchangeDealItem.OnClicked += ExchangeDealItem_OnClicked;

        }
    }

    private void ExchangeDealItem_OnClicked(long chips, string dealId)
    {
        cashInputField.text = chips.ToString();
        selectedDealId = dealId;
    }

    public async void HandleConfirm()
    {
        string id = idInputField.text;
        string confirmId = confirmIdInputField.text;
        Debug.Log("HANDLE CONFIRM");

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(confirmId))
        {
            UIManager.Instance.ShowAlertDialog("WingId is empty!");
            return;
        }
        if (id != confirmId)
        {
            UIManager.Instance.ShowAlertDialog("WingIds do not match!");
            return;
        }
        try
        {
            UIManager.Instance.ShowProgressing();
            await exchangePresenter.AddExchange(id, selectedDealId);
            UIManager.Instance.ShowAlertDialog("Request exchange successful!", () => OnClickTabHistory());
            await UIManager.Instance.LoadProfileUser();
            UIManager.Instance.HideProgressing();
        }
        catch (Exception ex)
        {
            UIManager.Instance.HideProgressing();
            UIManager.Instance.ShowAlertDialog(ex.Message);
        }
    }

    public void OnClickConfirm()
    {
        HandleConfirm();
    }

    public void OnClickTabExchange()
    {
        tabExchange.SetActive(true);
        tabHistory.SetActive(false);
        imageSelectedTabExchange.SetActive(true);
        imageSelectedTabHistory.SetActive(false);
    }

    public void OnClickTabHistory()
    {
        tabExchange.SetActive(false);
        tabHistory.SetActive(true);
        imageSelectedTabExchange.SetActive(false);
        imageSelectedTabHistory.SetActive(true);
    }
}
