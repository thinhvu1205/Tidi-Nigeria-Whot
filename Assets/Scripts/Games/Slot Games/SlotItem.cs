using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Globals;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class SlotItem : MonoBehaviour
{
    [SerializeField] protected Image[] slotImageList;
    [SerializeField] protected Sprite[] spriteList;
    [SerializeField] protected SkeletonGraphic spineItem;
    public Image[] ImageList => slotImageList;
    protected virtual Vector2[] ItemPositionList => new Vector2[]
    {
        new(0, 144),
        new(0, 0),
        new(0, -144),
    };
    protected SlotColumn column;
    protected int[] finishView = new int[3];
    protected int[] spreadFinishView = new int[3];
    protected int position = 0;

    public float Speed { get; set; } = 0.075f;
    public float SpeedBackSpin { get; set; } = 0.175f;
    protected virtual float IconScale => 0.77f;
    protected virtual float PositionResetY => 1023f;
    protected virtual string ICON_ANIMATION_PATH => "SlotSpine/Noel/SpineIcon/%id/skeleton_SkeletonData";
    protected virtual string ICON_ANIMATION_NAME => "animation";

    public void SetInfo(SlotColumn slotColumn)
    {
        column = slotColumn;
    }

    #region Spin Animations
    public void StartSpin(float speed, float backspinSpeed)
    {
        Speed = speed;
        SpeedBackSpin = backspinSpeed;
        Sequence seq = DOTween.Sequence();
        RectTransform rect = GetComponent<RectTransform>();
        Vector3 backPos = rect.localPosition + new Vector3(0, 30, 0);
        Vector3 initPos = rect.localPosition;
        seq.Append(rect.DOLocalMoveY(backPos.y, Speed).SetEase(Ease.OutSine));
        seq.Append(rect.DOLocalMoveY(initPos.y, SpeedBackSpin));
        seq.AppendCallback(() =>
        {
            if (rect.localPosition.y < 150)
            {
                rect.localPosition = new Vector3(rect.localPosition.x, PositionResetY, 0);
            }
            // if (Globals.Config.curGameId == (int)Globals.GAMEID.SLOT_JUICY_GARDEN)
            // {
            //     hideAllTextPackage();
            // }
            MoveDownLoop();
        });
    }

    protected void MoveDownLoop()
    {
        Sequence seq = DOTween.Sequence();
        RectTransform rect = GetComponent<RectTransform>();
        Vector3 nextPos = rect.localPosition - new Vector3(0, rect.sizeDelta.y, 0);
        Vector3 localPos = rect.localPosition;

        float multiplier = column.GetScatterCount() > 2 ? 2f : 1.25f;
        float moveSpeed = Speed * multiplier;
        rect.DOBlendableLocalMoveBy(new Vector2(0, -rect.sizeDelta.y), moveSpeed)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                position--;
                if (rect.localPosition.y < 0)
                {
                    rect.localPosition = new Vector3(localPos.x, PositionResetY, 0);
                    position = 3;
                }

                if (column.IsSpinning)
                {
                    MoveDownLoop();
                }
                else
                {
                    StopSpin();
                }
            });
    }

    protected void StopSpin()
    {
        Sequence seq = DOTween.Sequence();
        RectTransform rect = GetComponent<RectTransform>();
        Vector3 downPos = rect.localPosition + new Vector3(0, -rect.sizeDelta.y - 30, 0);
        Vector3 endPos = rect.localPosition + new Vector3(0, -rect.sizeDelta.y, 0);
        seq.AppendCallback(() =>
        {
            if (position == 2)
            {
                column.ResultItem = this;
                SetFinishView();
                SetItemValuePackage();
                if (finishView.Contains(12))
                {
                    column.IncreaseScatterCount();
                    column.CheckThirdScatter();

                }
                // SoundManager.instance.playEffectFromPath(Globals.SOUND_SLOT.STOP_SPIN);
            }
            else
            {
                SetRandomData();
            }
        });
        seq.Append(rect.DOLocalMoveY(downPos.y, Speed).SetEase(Ease.OutCirc));
        seq.Append(rect.DOLocalMoveY(endPos.y, SpeedBackSpin).SetEase(Ease.InCirc));
        seq.OnComplete(() =>
        {

            if (position == 1)
            {
                column.IsSpinning = false;
                // if (column.isNearFreeSpin)
                // {
                //     column.gameView.offAnimNearFreeSpin();
                //     //column.isNearFreeSpin = false;
                // }
                column.OnColumnStop();

                if (column.IsLastColumn)
                {
                    column.OnAllColumnsStop();

                }

            }
        });
    }

    public virtual void SetItemValuePackage()
    {

    }
    #endregion

    #region Getters
    public Vector2 GetItemPositionAtIndex(int index)
    {
        GameObject sprItem = slotImageList[index].gameObject;
        return sprItem.transform.parent.TransformPoint(ItemPositionList[index]);
    }

    public int[] GetFinishView() => finishView;
    #endregion

    #region Set Data
    public virtual void SetFinishView()
    {
        for (int i = 0; i < finishView.Length; i++)
        {
            Image image = slotImageList[i];
            image.sprite = spriteList[finishView[i]];
            image.SetNativeSize();
            // image.transform.localScale = Vector2.one * IconScale;

            if (finishView[i] == 12)
            {
                slotImageList[i].transform.localScale = new Vector2(0.8f, 0.8f);
            }
            else
            {
                slotImageList[i].transform.localScale = Vector2.one * IconScale;
            }
        }
    }
    protected void SetIconData(int index, int id)
    {
        Image image = slotImageList[index];
        image.sprite = spriteList[id];
        image.SetNativeSize();
        image.color = Color.white;
    }
    public void SetRandomData()
    {
        for (int i = 0; i < slotImageList.Length; i++)
        {
            Image image = slotImageList[i];
            image.sprite = spriteList[UnityEngine.Random.Range(0, 10)];
            image.SetNativeSize();
            image.transform.localScale = Vector2.one * IconScale;
        }
    }

    public virtual void SetItemAnimation(int index, bool isWild = false)
    {
        int itemIndex = finishView[index];
        spineItem.transform.localScale = itemIndex switch
        {
            0 or 1 or 2 or 3 => (Vector3)new Vector2(0.8f, 0.8f),
            11 => (Vector3)new Vector2(0.52f, 0.6f),
            12 => (Vector3)new Vector2(0.52f, 0.6f),
            4 or 5 or 6 or 7 or 8 => (Vector3)new Vector2(0.75f, 0.75f),
            _ => (Vector3)Vector2.one,
        };
        spineItem.gameObject.transform.localPosition = slotImageList[index].gameObject.GetComponent<RectTransform>().localPosition;
        spineItem.gameObject.SetActive(true);
        string itemAnimationPath = ICON_ANIMATION_PATH.Replace("%id", itemIndex.ToString());
        Utility.PlayAnimationByPath(spineItem, itemAnimationPath, ICON_ANIMATION_NAME);
    }

    public void ShowScatterAnimation()
    {
        int indexScatter = Array.IndexOf(finishView, 12);
        SetItemAnimation(indexScatter);
    }

    public virtual void SetDark(bool isDark, int index = -1)
    {
        if (isDark) spineItem.gameObject.SetActive(false);
        Color colorState = isDark ? Color.gray : Color.white;
        if (index >= 0 && index < slotImageList.Length)
        {
            slotImageList[index].color = colorState;
            // Debug.Log("iconIdList[index]: " + iconIdList[index]);
        }
        else
        {
            for (int i = 0; i < slotImageList.Length; i++)
            {
                slotImageList[i].color = colorState;
                slotImageList[i].gameObject.SetActive(true);
                spineItem.gameObject.SetActive(false);
                // Nếu cần clear animation thì xử lý ở đây
                // listSpineItem[i].gameObject.SetActive(false);
                // CollumSpinCtrl.gameView.removeAnimIcon(listSpineItem[i].gameObject);
            }

            // Clear animation list nếu cần
            // listSpineItem.Clear();
        }
    }

    public void SetFinishIndices(int[] symbolIdArray)
    {
        finishView = symbolIdArray;
    }

    public void SetSpreadFinishIndices(int[] symbolIdArray)
    {
        spreadFinishView = symbolIdArray;
    }
    #endregion
}
