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
                    _ = giftCodeView.OnSubmitFinished("Code not open!");
                    break;
                case (int)GiftCodeError.HasClosed:
                    _ = giftCodeView.OnSubmitFinished("Code has closed!");
                    break;
                case (int)GiftCodeError.ReachMaxClaimed:
                    _ = giftCodeView.OnSubmitFinished("Code reach max claimed!");
                    break;
                case (int)GiftCodeError.AlreadyClaimed:
                    _ = giftCodeView.OnSubmitFinished("Code already claimed!");
                    break;
                case (int)GiftCodeError.LvVipNotMeetRequire:
                    _ = giftCodeView.OnSubmitFinished("Vip level not met require!");
                    break;
                default:
                    _ = giftCodeView.OnSubmitFinished(result.Message);
                    break;
            }
        }
        catch (Exception ex)
        {
            Error error = DataSender.DecodeFromJson<Error>(ex.Message);
            _ = giftCodeView.OnSubmitFinished(error.Error_, true);
        }
    }
}

