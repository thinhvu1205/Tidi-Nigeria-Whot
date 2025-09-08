using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Proto;
using UnityEngine;

public class FreeChipView : BaseView
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void OnEnable()
    {
        _ = LoadRewardList();
    }

    private async UniTask LoadRewardList()
    {
        ListFreeChip listFreeChip = await DataSender.GetListClaimedFreeChips();
        Debug.Log("listFreeChip " + listFreeChip);
    }
}
