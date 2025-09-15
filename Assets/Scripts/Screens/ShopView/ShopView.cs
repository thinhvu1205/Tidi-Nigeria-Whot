using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using TMPro;
using UnityEngine;

public class ShopView : BaseView
{
    [SerializeField] private Transform listDealContainer;
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private TextMeshProUGUI textAccountChip;
    private Deal bestDeal;
    private List<Deal> listDeal;

    protected override void Awake()
    {
        base.Awake();
        UpdateProfileData();
        _ = LoadListDealInShop();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        UIManager.Instance.OpenBanner(TypeInAppMessage.Banner);
    }

    public override void OnClickCloseButton()
    {
        base.OnClickCloseButton();
        UIManager.Instance.OpenBanner(TypeInAppMessage.Banner);
    }
    private async UniTask LoadListDealInShop()
    {
        foreach (GameObject mailItem in listDealContainer)
        {
            Destroy(mailItem);
        }
        try
        {
            DealInShop listDeal = await DataSender.GetListDeal();
            this.bestDeal = listDeal.Best;
            this.listDeal = listDeal.Gcashes.ToList();
            Debug.Log("CHIP ONLINE LIST: " + listDeal.ToString());
            ShopItem bestDealInstance = Instantiate(shopItemPrefab, listDealContainer).GetComponent<ShopItem>();
            bestDealInstance.SetInfo(bestDeal, 0, true);

            for (int i = 1; i <= this.listDeal.Count; i++)
            {
                Deal deal = this.listDeal[i - 1];
                ShopItem dealInstance = Instantiate(shopItemPrefab, listDealContainer).GetComponent<ShopItem>();
                dealInstance.SetInfo(deal, i, false);
            }
        }
        catch (Exception ex)
        {
            Debug.Log("err load list noti : " + ex.Message);
        }
    }

    public void UpdateProfileData()
    {
        if (User.userProfile != null)
        {
            textAccountChip.text = User.userProfile.AccountChip.ToString();
        }
    }
}
