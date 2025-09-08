using System;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using UnityEngine;

public class GiftCodePresenter
{
    public static GiftCodePresenter Instance { get; private set; }
    private GiftCodeView giftCodeView;

    public void Init(GiftCodeView view)
    {
        Instance = this;
        giftCodeView = view;
    }

    public async UniTask OnSubmit(string giftCode)
    {
        try
        {
            var result = await DataSender.ClaimGiftCode(giftCode);
            switch (result.ErrCode) {
                case (int)GiftCodeError.NotOpen:
                    giftCodeView.OnSubmitFinished("Code not open!");
                    break;
                case (int)GiftCodeError.HasClosed:
                    giftCodeView.OnSubmitFinished("Code has closed!");
                    break;
                case (int)GiftCodeError.ReachMaxClaimed:
                    giftCodeView.OnSubmitFinished("Code reach max claimed!");
                    break;
                case (int)GiftCodeError.AlreadyClaimed:
                    giftCodeView.OnSubmitFinished("Code already claimed!");
                    break;
                case (int)GiftCodeError.LvVipNotMeetRequire:
                    giftCodeView.OnSubmitFinished("Vip level not met require!");
                    break;
                default:
                    giftCodeView.OnSubmitFinished(result.Message);
                    break;
            }
        }
        catch (Exception)
        {
            giftCodeView.OnSubmitFinished("Error when claim gift code!");
            throw;
        }
    }
}

