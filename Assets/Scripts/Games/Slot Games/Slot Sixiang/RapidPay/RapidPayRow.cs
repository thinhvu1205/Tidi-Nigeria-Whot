using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using Proto;
using Google.Protobuf;
using Color = UnityEngine.Color;
using Globals;
using System.Linq;
using DG.Tweening;
using System;

public class RapidPayRow : MonoBehaviour
{
    [SerializeField] private int indexRow = 0;
    public List<Button> listButtonItem = new();
    private Button currentItemPick;
    private const string ITEM_ANIMATION_PATH = "SiXiang/Spine/ItemPick/skeleton_SkeletonData";

    private readonly int[][] listResultIndex = new int[][]
    {
        new int[] { 1, 0 },
        new int[] { 7, 6, 5 },
        new int[] { 13, 12, 11, 10 },
        new int[] { 18, 17, 16, 15 },
        new int[] { 24, 23, 22, 21, 20 }
    };

    private void Awake()
    {
        listButtonItem.ForEach((btn) =>
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                OnClickItem(btn);
            });
            btn.interactable = false;
        });
    }

    public void ActiveAllButtons()
    {
        listButtonItem.ForEach((btn) =>
        {
            btn.interactable = true;
        });
    }

    public void OnClickItem(Button btn)
    {
        currentItemPick = btn;
        listButtonItem.ForEach((btn) =>
        {
            btn.interactable = false;
        });
        Debug.Log("SEND BUTTON INDEX: " + (listButtonItem.Count - 1 - listButtonItem.IndexOf(btn)));
        InfoBet infoBet = new()
        {
            ReqSpecGame = (int)SiXiangGame.Rapidpay,
            Id = (listButtonItem.Count - 1 - listButtonItem.IndexOf(btn))
        };
        DataSender.SendMatchState((long)OpCodeRequest.Spin, infoBet.ToByteArray());
    }

    public void SetResult(SlotDesk data)
    {
        if (data.SpinSymbols == null || data.SpinSymbols.Count == 0)
        {
            return;
        }
        Debug.Log("SET RESULT");
        SpinSymbol item = data.SpinSymbols[0];
        List<SiXiangSymbol> matrix = data.Matrix.Lists.ToList();
        // int indexAnimOpen = 0;
        int floorNumber = item.Row;
        int[] listIndex = listResultIndex[floorNumber];
        listButtonItem.ForEach((btn) =>
        {
            btn.interactable = false;
        });
        int buttonIndex = Array.IndexOf(listIndex, item.Index);
        if (item != null)
        {
            DOTween.Sequence()
                .AppendCallback(() =>
                {
                    SkeletonGraphic spineItemCurrent = listButtonItem[buttonIndex].GetComponentInChildren<SkeletonGraphic>();
                    Utility.PlayAnimationByPath(spineItemCurrent, ITEM_ANIMATION_PATH, GetAnimationName(item.Symbol), false);

                    if (GetAnimationName(item.Symbol).Equals("end"))
                        SoundManager.Instance.PlayEffectFromPath(SoundSlot.CLICK_ITEM_MISS);
                    else
                        SoundManager.Instance.PlayEffectFromPath(SoundSlot.RAPID_ITEM_WIN);
                })
                .AppendInterval(1f)
                .AppendCallback(() =>
                {
                    for (int i = 0; i < listButtonItem.Count; i++)
                    {
                        if (listButtonItem[i] != listButtonItem[buttonIndex])
                        {
                            SkeletonGraphic spineItemCurrent = listButtonItem[i].GetComponentInChildren<SkeletonGraphic>();
                            spineItemCurrent.color = Color.gray;
                            SiXiangSymbol symbol = matrix[listIndex[i]];
                            Utility.PlayAnimationByPath(spineItemCurrent, ITEM_ANIMATION_PATH, GetAnimationName(symbol), false);
                        }
                    }
                });
        }
    }

    private string GetAnimationName(SiXiangSymbol symbol)
    {
        string animationName = "normal";
        switch (symbol)
        {
            case SiXiangSymbol.RapidpayEnd: animationName = "end"; break;
            case SiXiangSymbol.RapidpayX2: animationName = "2x"; break;
            case SiXiangSymbol.RapidpayX3: animationName = "3x"; break;
            case SiXiangSymbol.RapidpayX4: animationName = "4x"; break;
        }
        return animationName;
    }

    public void Reset()
    {
        foreach (Button button in listButtonItem)
        {
            SkeletonGraphic spine = button.GetComponentInChildren<SkeletonGraphic>();
            spine.color = Color.white;
            spine.gameObject.SetActive(false);
            Utility.PlayAnimationByPath(spine, ITEM_ANIMATION_PATH, "normal", true);
        }   
    }
}
