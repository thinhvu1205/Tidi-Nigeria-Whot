using System;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using UnityEngine;

public class BannerPresenter
{
    public static BannerPresenter Instance { get; private set; }
    private ListBannerView bannerView;

    public void Init(ListBannerView view)
    {
        Instance = this;
        bannerView = view;
    }

    public async UniTask<ListInAppMessage> GetBanner(TypeInAppMessage typeInAppMessage)
    {
        var inAppMessageData = await DataSender.GetListInAppMessage(typeInAppMessage);
        return inAppMessageData;
    }
}

