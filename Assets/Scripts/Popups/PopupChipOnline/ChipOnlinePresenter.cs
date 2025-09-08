using System;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using UnityEngine;

public class ChipOnlinePresenter
{
    public static ChipOnlinePresenter Instance { get; private set; }
    private ChipOnlineView chipOnlineView;

    public void Init(ChipOnlineView view)
    {
        Instance = this;
        chipOnlineView = view;
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
            await chipOnlineView.OnSuccess();
        }
        catch (Exception)
        {
            chipOnlineView.OnError("Error when claim gift code!");
        }
    }
}

