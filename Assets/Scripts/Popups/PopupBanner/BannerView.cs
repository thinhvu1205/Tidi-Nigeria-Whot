using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Proto;
using UnityEngine;
using UnityEngine.UI;

public class BannerView : BaseView
{
    [SerializeField] private Image imageBanner;

    private Action callbaclClick = null;

    public async void SetInfo(InAppMessageData data)
    {
       
    }

}
