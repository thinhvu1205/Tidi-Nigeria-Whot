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

    public async UniTask<ListFreeChip> GetFreeChipList()
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            ListFreeChip listFreeChip = await DataSender.GetListClaimedFreeChips();
            await freeChipView.OnSuccess();
            return listFreeChip;
        }
        catch (Exception)
        {
            freeChipView.OnError("Fail to ger reward list!");
            return null;
        }
    }

    public async UniTask OnClickClaim(FreeChip freeChip)
    {
        try
        {
            UIManager.Instance.ShowProgressing();
            var result = await DataSender.ClaimFreeChip(freeChip);
            await freeChipView.OnSuccess("You have received " + Utility.FormatNumber(result.Chips) + " chips!", true);
        }
        catch (Exception)
        {
            freeChipView.OnError("Fail to claim gift code!");
        }
    }
}

