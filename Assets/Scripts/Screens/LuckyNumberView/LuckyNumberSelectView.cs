using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuckyNumberSelectView : BaseView
{
    private LuckyNumberView luckyNumberView;

    public void Init(LuckyNumberView view)
    {
        luckyNumberView = view;
    }
    
    public override void OnClickCloseButton()
    {
        Hide(false);
    }
}
