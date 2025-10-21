using System;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using UnityEngine;

public class LuckyNumberPresenter
{
    public static LuckyNumberPresenter Instance { get; private set; }
    private LuckyNumberView luckyNumberView;

    public void Init(LuckyNumberView view)
    {
        Instance = this;
        luckyNumberView = view;
    }
}