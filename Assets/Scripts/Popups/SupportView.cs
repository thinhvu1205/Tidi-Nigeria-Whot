using System.Collections;
using System.Collections.Generic;
using Globals;
using UnityEngine;

public class SupportView : BaseView
{
    [SerializeField] private GameObject teleItem, messItem;

    protected override void OnEnable()
    {
        base.OnEnable();
        teleItem.SetActive(true);
        messItem.SetActive(true);
    }

    public void OnClickTele()
    {
        Application.OpenURL(Config.chatMessSupportLink);
    }

    public void OnClickMessenger()
    {
        Application.OpenURL(Config.chatTeleSupportLink);
    }
}
