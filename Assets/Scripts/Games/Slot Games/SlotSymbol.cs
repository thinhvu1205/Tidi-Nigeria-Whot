using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Globals;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class SlotSymbol : MonoBehaviour
{
    [SerializeField] protected SkeletonGraphic spine, backgroundWin;
    [SerializeField] protected Image sprite;
    [SerializeField] protected SlotSymbolColumn column;

    [SerializeField] Sprite[] listSpriteNormal, listSpriteBlur;
    [SerializeField] protected int indexSymbol;
    public int IndexSymbol => indexSymbol;
    public Image Sprite => sprite;
    protected int indexStop = -1;
    protected float Speed = 0.1f;
    protected float SpeedShowingThirdScatter = 0.4f;
    protected int id = 0;
    protected virtual string ICON_ANIMATION_PATH => "SiXiang/Spine/Icon/%s/skeleton_SkeletonData";

    // Update is called once per frame
    private void Update()
    {
        // if (column.isNeerSpin)
        // {
        //     Speed = column.speedThirdScatterAnimation;
        // }
    }

    public void StartSpin(float speed)
    {
        Speed = speed;
        float newPosition = transform.localPosition.y - column.StepMove;
        indexSymbol++;
        transform
            .DOLocalMoveY(newPosition, 0.4f)
            .SetEase(Ease.InBack)
            .OnComplete(() => { StartCoroutine(MoveDownLoopCoroutine()); });
    }

    private IEnumerator MoveDownLoopCoroutine()
    {
        while (column.IsSpinning)
        {
            if (indexSymbol > 3)
            {
                transform.localPosition = new Vector2(transform.localPosition.x, column.PositionReset);
                indexSymbol = 0;
                SetSprite(Random.Range(0, 8), true);
            }

            indexSymbol++;
            float moveSpeed = column.IsShowingThirdScatter ? SpeedShowingThirdScatter : Speed;
            yield return transform
                .DOBlendableLocalMoveBy(new Vector2(0, -column.StepMove), moveSpeed)
                .SetEase(Ease.Linear)
                .WaitForCompletion(); // đợi tween xong rồi mới lặp tiếp
        }

        StopSpin();
    }

    private void StopSpin()
    {
        // Debug.Log("indexsymbol: " + indexSymbol + " indexStop: " + indexStop);
        if (indexSymbol > 3)
        {
            transform.localPosition = new Vector2(transform.localPosition.x, column.PositionReset);
            indexSymbol = 0;
            if (column.FinishSymbolView.Count > 0)
            {
                indexStop = column.FinishSymbolView.Count;
                int lastIndex = column.FinishSymbolView.Count - 1;
                int spriteIndex = column.FinishSymbolView[lastIndex];
                column.FinishSymbolView.RemoveAt(lastIndex);
                SetSprite(spriteIndex);
                StopSpin();
            }
            else
            {
                transform.localPosition = new Vector2(transform.localPosition.x,
                    column.PositionReset + column.StepMove);
                DOTween.Sequence()
                    .Append(transform.DOBlendableLocalMoveBy(new Vector2(0, -column.StepMove), Speed)
                    .SetEase(Ease.OutBack));
                SetRandomSprite();
            
            }
        }
        else
        {
            Ease easing = (indexSymbol + 1 == indexStop) ? Ease.OutBack : Ease.Linear;
            float moveSpeed = column.IsShowingThirdScatter ? SpeedShowingThirdScatter : Speed;
            float speedMove = (indexSymbol + 1 == indexStop) ? moveSpeed * 3 : moveSpeed;
            indexSymbol++;
            DOTween.Sequence()
                .Append(transform.DOBlendableLocalMoveBy(new Vector2(0, -column.StepMove), speedMove)
                .SetEase(easing)).AppendCallback(() =>
                {
                    if (indexSymbol != indexStop)
                    {
                        StopSpin();
                    }
                    else if (indexSymbol == 3)
                    {
   
                        column.OnColumnStop();
                    }
                });
                if (indexSymbol == 3)
                {
                    DOTween.Sequence().AppendInterval(0.85f * speedMove).AppendCallback(() =>
                    {
                        column.prepareStop();
                    });
                }
        }
    }

    public virtual void SetSprite(int idSprite, bool isBlur = false)
    {
        id = idSprite;
        spine.gameObject.SetActive(false);
        if (!isBlur)
        {
            sprite.sprite = listSpriteNormal[id];
        }
        else
        {
            sprite.sprite = listSpriteBlur[id];
        }

        if (id == 11 || id == 13)
        {
            sprite.color = Color.white;
        }

        sprite.SetNativeSize();
    }

    public void SetRandomSprite()
    {
        SetSprite(Random.Range(0, 9));
    }

    public void SetSpine(float time = 1.0f, bool isLoop = false)
    {
        // Action<SkeletonDataAsset> cb = (skeData) =>
        // {
        //     spine.gameObject.SetActive(true);
        //     spine.skeletonDataAsset = skeData;
        //     spine.Initialize(true);
        //     spine.AnimationState.SetAnimation(0, "animation", false);
        //     spine.timeScale = timeScale;
        //     spine.startingLoop = false;
        //     if (idSpine == 9 || idSpine == 10)
        //     {
        //         spine.transform.localScale = new Vector2(0.9f, 0.9f);
        //     }
        //     else
        //     {
        //         spine.transform.localScale = Vector2.one;
        //     }
        // };
        // StartCoroutine(Utility.PlayAnimationByPath(getSpinePath(idSpine), cb));
        if (id == 9 || id == 10)
        {
            spine.transform.localScale = new Vector2(0.85f, 0.85f);
        }
        else
        {
            spine.transform.localScale = Vector2.one;
        }
        spine.gameObject.SetActive(true);
        spine.timeScale = time;
        spine.Initialize(true);
        Utility.PlayAnimationByPath(spine, GetSpinePath(id), "animation", isLoop);
    }

    public void HideSpine()
    {
        spine.gameObject.SetActive(false);
    }

    public void ShowSpineWild(float timeScale = 1.0f)
    {
        id = 10;
        SetSpine(timeScale, true);
    }

    public void ShowEffectSpreadWild(Vector2 startPosition, float scaleTime, float animationTime)
    {
        spine.transform.SetParent(column.transform);
        spine.transform.position = startPosition;
        id = 10;
        SetSpine(scaleTime, true);
        Sequence sequence = DOTween.Sequence();
        sequence
            .Append(spine.transform.DOMoveY(transform.position.y, animationTime)
            .OnComplete(() =>
            {
                spine.transform.SetParent(transform);
                spine.transform.localPosition = Vector2.zero;
            }))
            .AppendInterval(animationTime)
            .AppendCallback(() =>
            {
                column.HideAllSpine();
                spine.timeScale = 1.0f;
                SetSprite(10);
            });
    }

    public async UniTask showScatterSpine()
    {
        // SetSpine(10);
        sprite.gameObject.SetActive(false);
        await UniTask.Delay(1000);
        spine.gameObject.SetActive(false);
        sprite.gameObject.SetActive(true);
    }


    public void ShowBackgroundWin()
    {
        backgroundWin.gameObject.SetActive(true);
        Utility.PlayAnimation(backgroundWin, "animation", true);
    }

    public void HideBackgroundWin()
    {
        backgroundWin.gameObject.SetActive(false);
    }

    private string GetSpinePath(int idSpine)
    {
        List<string> listFolderName = new()
            { "10", "J", "Q", "K", "A", "Dragon", "Tiger", "Turtle", "Phoenix", "Scatter", "Wild" };
        return Utility.FormatString(ICON_ANIMATION_PATH, listFolderName[idSpine]);
    }

    public void UpdateThirdScatterSpeed()
    {
        DOTween.To(() => Speed, x => Speed = x, SpeedShowingThirdScatter, 2.5f)
            .SetEase(Ease.Linear);
    }

    public void Reset()
    {
        if (indexSymbol == 0)
        {
            SetSprite(Random.Range(0, 9));
        }
        Speed = 0.1f;
        // sprite.color = Color.white;
        indexStop = -1;
        backgroundWin.gameObject.SetActive(false);
        spine.gameObject.SetActive(false);
    }

}
