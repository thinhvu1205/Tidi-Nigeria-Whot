using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Proto;
using UnityEngine;
using static FreeChipItem;

public class FreeChipView : BaseView
{
    public static event Action OnClaimed;
    [SerializeField] private GameObject freeChipItemPrefab;
    [SerializeField] private Transform freeChipItemContainer;
    private FreeChipPresenter freeChipPresenter;
    private List<FreeChip> listFreeChip = new();

    protected override void Awake()
    {
        base.Awake();
        freeChipPresenter = new FreeChipPresenter();
        freeChipPresenter.Init(this);
    }

    protected override void OnEnable()
    {
        _ = LoadRewardList();
    }

    private async UniTask LoadRewardList()
    {
        ListFreeChip listFreeChipResponse = await freeChipPresenter.GetFreeChipList();
        listFreeChip = listFreeChipResponse.Freechips.ToList();
        UpdateUIRewardList();
    }

    private void UpdateUIRewardList()
    {
        foreach(Transform child in freeChipItemContainer)
        {
            DestroyImmediate(child.gameObject);
        }
        foreach (FreeChip freeChip in listFreeChip)
        {
            FreeChipItem freeChipItem = Instantiate(freeChipItemPrefab, freeChipItemContainer).GetComponent<FreeChipItem>();
            freeChipItem.SetInfo(freeChip);
            freeChipItem.OnItemClicked += FreeChipItem_OnItemClicked;
        }
    }

    private async void FreeChipItem_OnItemClicked(object sender, OnItemClickedEventArgs e)
    {
        await freeChipPresenter.OnClickClaim(e.freeChip);
        await LoadRewardList();
        OnClaimed?.Invoke();
    }
}
