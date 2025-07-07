using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class SlotItem : MonoBehaviour
{
    [SerializeField] protected Image[] slotImageList;
    [SerializeField] protected Sprite[] spriteList;
    [SerializeField] protected SkeletonGraphic[] spineList;

    protected Vector2[] itemPositionList = {
        new(0, 144),
        new(0, 0),
        new(0, -144),
    };
    protected SlotColumn column;
    protected List<int> iconIdList = new();
    protected List<int> finishView = new();
    protected int typePosition = 0;

    public float Speed { get; set; } = 0.075f;
    public float SpeedBackSpin { get; set; } = 0.175f;
    protected const float POSITION_RESET_Y = 1023;

    public void SetInfo(SlotColumn slotColumn)
    {
        column = slotColumn;
    }

    #region Spin Animations
    public virtual void StartSpin(float speed, float backspinSpeed)
    {
        Speed = speed;
        SpeedBackSpin = backspinSpeed;
        Debug.Log("Item start spin");
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
                rect.localPosition = new Vector3(rect.localPosition.x, POSITION_RESET_Y, 0);
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

        // float moveSpeed = Speed * (CollumSpinCtrl.gameView.countScatter > 2 ? 1.75f : 1);
        float moveSpeed = Speed;
            rect.DOBlendableLocalMoveBy(new Vector2(0, -rect.sizeDelta.y), moveSpeed)
            .SetEase(Ease.Linear) // Quan trọng: để tốc độ đều
            .OnComplete(() =>
            {
                typePosition--;
                if (rect.localPosition.y < 0)
                {
                    rect.localPosition = new Vector3(localPos.x, POSITION_RESET_Y, 0);
                    typePosition = 3;
                }

                if (column.IsSpinning)
                {
                    SetRandomData();
                    MoveDownLoop();
                }
                else
                {
                    StopSpin();
                }
            });
    }

    protected virtual void StopSpin()
    {
        Sequence seq = DOTween.Sequence();
        RectTransform rect = GetComponent<RectTransform>();
        Vector3 downPos = rect.localPosition + new Vector3(0, -rect.sizeDelta.y - 30, 0);
        Vector3 endPos = rect.localPosition + new Vector3(0, -rect.sizeDelta.y, 0);
        seq.AppendCallback(() =>
        {
            if (typePosition == 2)
            {
                column.SetResultItem(this);
                SetFinishView();
                // if (arrID.Contains(12))
                // {
                //     CollumSpinCtrl.gameView.countScatter++;
                // }
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

            if (typePosition == 1)
            {
                column.IsSpinning = false;
                // if (column.isNearFreeSpin)
                // {
                //     column.gameView.offAnimNearFreeSpin();
                //     //column.isNearFreeSpin = false;
                // }
                if (column.IsLastColumn)
                {
                    column.OnAllColumnsStop();
                }
            }
        });
    }
    #endregion

    #region Getters
    public Vector2 GetItemPositionAtIndex(int index)
    {
        GameObject sprItem = slotImageList[index].gameObject;
        return sprItem.transform.parent.TransformPoint(itemPositionList[index]);

    }
    #endregion

    #region Set Data
    protected void SetFinishView()
    {
        iconIdList = finishView;
        for (int i = 0; i < finishView.Count; i++)
        {
            Image image = slotImageList[i];
            image.sprite = spriteList[finishView[i]];
            image.SetNativeSize();
            image.transform.localScale = Vector2.one * 0.77f;

            // if (finishView[i] == 12)
            // {
            //     slotImageList[i].transform.localScale = new Vector2(0.8f, 0.8f);
            // }
            // else
            // {
            //     slotImageList[i].transform.localScale = Vector2.one;
            // }
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
            image.transform.localScale = Vector2.one * 0.77f;
        }
    }

    private void SetItemAnimation(int index, int id, bool isWild = false)
    {

    }

    public void SetDark(bool isDark, int index = -1)
    {
        Color colorState = isDark ? Color.gray : Color.white;
        if (index >= 0 && index < slotImageList.Length)
        {
            slotImageList[index].color = colorState;
            // SetItemAnimation(index, iconIdList[index]);
        }
        else
        {
            for (int i = 0; i < slotImageList.Length; i++)
            {
                slotImageList[i].color = colorState;
                slotImageList[i].gameObject.SetActive(true);

                // Nếu cần clear animation thì xử lý ở đây
                // listSpineItem[i].gameObject.SetActive(false);
                // CollumSpinCtrl.gameView.removeAnimIcon(listSpineItem[i].gameObject);
            }

            // Clear animation list nếu cần
            // listSpineItem.Clear();
        }
    }

    public void SetFinishIndices(List<int> arrayId)
    {
        finishView = arrayId;
    }
    #endregion
}
