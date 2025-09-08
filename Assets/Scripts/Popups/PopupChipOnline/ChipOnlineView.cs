using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Proto;
using UnityEngine;

public class ChipOnlineView : BaseView
{
    [SerializeField] private GameObject chipOnlineItemPrefab;
    [SerializeField] private Transform chipOnlineItemContainer;
    private ChipOnlinePresenter chipOnlinePresenter;

    protected override void Awake()
    {
        base.Awake();
        chipOnlinePresenter = new ChipOnlinePresenter();
        chipOnlinePresenter.Init(this);
    }
}
