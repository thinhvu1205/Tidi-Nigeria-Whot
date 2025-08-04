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

public class RapidPayRow : MonoBehaviour
{
    [SerializeField] private int indexRow = 0;
    public List<Button> listButtonItem = new();
    private Button currentItemPick;
    private readonly int[][] listResultIndex = new int[][]
    {
        new int[] { 0, 1 },
        new int[] { 5, 6, 7 },
        new int[] { 10, 11, 12, 13 },
        new int[] { 15, 16, 17, 18 },
        new int[] { 20, 21, 22, 23, 24 }
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
        Debug.Log("SEND BUTTON INDEX: " + listButtonItem.IndexOf(btn));
        InfoBet infoBet = new()
        {
            ReqSpecGame = (int)SiXiangGame.Rapidpay,
            Id = listButtonItem.IndexOf(btn)
        };
        DataSender.SendMatchState((long)OpCodeRequest.Spin, infoBet.ToByteArray());
    }

    public void SetResult(SlotDesk data)
    {
        if (data.SpinSymbols == null || data.SpinSymbols.Count == 0)
        {
            return;
        }
        SpinSymbol item = data.SpinSymbols[0];
        List<SiXiangSymbol> matrix = data.Matrix.Lists.ToList();
        // int indexAnimOpen = 0;
        int floorNumber = item.Row;
        int[] listIndex = listResultIndex[floorNumber];
        listButtonItem.ForEach((btn) =>
        {
            btn.interactable = false;
        });
        if (currentItemPick != null)
        {
            DOTween.Sequence()
                .AppendCallback(() =>
                {
                    SkeletonGraphic spineItemCurrent = currentItemPick.GetComponentInChildren<SkeletonGraphic>();
                    Utility.PlayAnimation(spineItemCurrent, GetAnimationName(item.Symbol), false);

                    // if (getAnimName(result).Equals("end"))
                    //     SoundManager.instance.playEffectFromPath(Globals.SOUND_SLOT_BASE.CLICK_ITEM_MISS);
                    // else
                    //     SoundManager.instance.playEffectFromPath(Globals.SOUND_SLOT_BASE.RAPID_ITEM_WIN);
                })
                .AppendInterval(1f)
                .AppendCallback(() =>
                {
                    for (int i = 0; i < listButtonItem.Count; i++)
                    {
                        if (listButtonItem[i] != currentItemPick)
                        {
                            SkeletonGraphic spineItemCurrent = listButtonItem[i].GetComponentInChildren<SkeletonGraphic>();
                            spineItemCurrent.color = Color.gray;
                            SiXiangSymbol symbol = matrix[listIndex[i]];
                            Utility.PlayAnimation(spineItemCurrent, GetAnimationName(symbol), false);
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
        }   
    }
}
