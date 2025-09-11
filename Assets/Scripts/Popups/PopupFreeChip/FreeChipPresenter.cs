using System;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using UnityEngine;

public class FreeChipPresenter
{
    public static FreeChipPresenter Instance { get; private set; }
    private FreeChipView freeChipView;

    public void Init(FreeChipView view)
    {
        Instance = this;
        freeChipView = view;
    }

    public async UniTask<ListFreeChip> LoadRewardList()
    {
        try
        {
            ListFreeChip listFreeChip = await DataSender.GetListClaimedFreeChips();
            return listFreeChip;
        }
        catch (Exception)
        {

            return null;
        }
    }

    public async UniTask OnClickClaim(FreeChip freeChip)
    {
        try
        {
            var result = await DataSender.ClaimFreeChip(freeChip);
            await freeChipView.OnSuccess();
        }
        catch (Exception)
        {
            freeChipView.OnError("Error when claim gift code!");
        }
    }
}

