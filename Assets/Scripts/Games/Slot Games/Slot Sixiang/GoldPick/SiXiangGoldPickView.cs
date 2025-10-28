using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Spine.Unity;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;
using Globals;
using Proto;
using Google.Protobuf;

public class SiXiangGoldPickView : MonoBehaviour
{
    [SerializeField] private GameObject itemGoldPrefab;

    [SerializeField] private Transform itemContainer;
    [SerializeField] private TextMeshProUGUI textRemainingPick;
    [SerializeField] private TextNumberControl textTotalWin;
    [SerializeField] private Button buttonConfirm;
    [SerializeField] private SkeletonGraphic animationResult;
    [SerializeField] List<Material> materialText = new(); //0 green,1 gold

    private UnityEngine.Pool.ObjectPool<GameObject> itemPool;
    private SlotSixiangView gameView;
    private GameObject currentItemClick;
    private int remainPick = 20;
    private bool canClick = true;
    private readonly List<GameObject> listItem = new();
    private const string LUCKY_GOLD_ITEM_ANIMATION_PATH = "SiXiang/Spine/LuckyGoldItem/skeleton_SkeletonData";
    private const string LUCKY_GOLD_NICE_TRY_ANIMATION_PATH = "SiXiang/Spine/LuckyGoldTryAgain/skeleton_SkeletonData";
    
    private void OnEnable()
    {
        itemPool = new UnityEngine.Pool.ObjectPool<GameObject>(
            createFunc: () =>
            {
                var item = Instantiate(itemGoldPrefab, itemContainer);
                item.SetActive(false); // bắt đầu ẩn
                return item;
            },
            actionOnGet: (item) =>
            {
                item.SetActive(true);
                item.transform.localScale = Vector3.one;
            },
            actionOnRelease: (item) =>
            {
                item.SetActive(false);
            },
            actionOnDestroy: (item) =>
            {
                Destroy(item);
            },
            defaultCapacity: 5,    
            maxSize: 30             
        );  
        canClick = true;
        InitRainItems();

        // Sau 7 giây, tự động gọi hàm AutoPlay
        DOTween
            .Sequence()
            .AppendInterval(7.0f)
            .AppendCallback(() =>
            {
                AutoPlay();
            }).SetId("autoPlay");
    }

    private void OnDisable()
    {
        gameView.OnUpdateTable -= SixiangView_OnUpdateTable;
    }

    private void OnDestroy()
    {
        DOTween.Kill("autoPlay");
    }

    public void SetInfo(SlotSixiangView sixiangView, int remainingPick)
    {
        gameView = sixiangView;
        gameView.OnUpdateTable += SixiangView_OnUpdateTable;
        gameView.UpdateTotalChipWinValue();
        if (remainingPick == 0) remainingPick = 20;
        textRemainingPick.text = remainingPick + " Remaining Picks";

    }
    
    private void AutoPlay()
    {
        Debug.Log("onAutoPlay");
        GameObject itemAuto = listItem.Find((item) =>
        {
            return item.transform.localPosition.y > 0 && item.transform.localPosition.y < 250 && item.activeSelf && item.transform.localPosition.x > -350 && item.transform.localPosition.x < 350;
        });
        if (itemAuto != null)
        {
            OnClickItemGold(itemAuto);
        }
        else
        {
            DOTween.Sequence()
                .AppendInterval(1.0f)
                .AppendCallback(() =>
                {
                    AutoPlay();
                }).SetId("autoPlay");
        }
    }

    private void InitRainItems()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    GameObject item = CreateItemGold();
                    item.transform.localPosition = new Vector2(Random.Range(-600, 600), Random.Range(400, 500));
                    item.transform.localEulerAngles = new Vector3(0, 0, Random.Range(0, 360));
                    MoveItem(item, i);
                }
            })
            .AppendInterval(3.5f).SetLoops(-1).SetId("initRainItem");

    }
    
    private void MoveItem(GameObject item, int index)
    {
        GoldPickItem itemComp = item.GetComponent<GoldPickItem>();
        DOTween.Sequence()
            .AppendInterval(index * 0.2f);
        int time = Random.Range(5, 7);
        item.transform
            .DOBlendableLocalMoveBy(new Vector3(0, -700), time, true)
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                if (item.transform.localPosition.y < -300)
                {
                    RemoveItem(item);
                }
            })
            .SetLoops(-1, LoopType.Incremental);
        item.transform
            .DOBlendableLocalRotateBy(new Vector3(0, 0, Random.Range(1, 3) < 2 ? 360 : -360), time, RotateMode.FastBeyond360)
            .OnUpdate(() =>
            {
                itemComp.TextMoney.transform.localEulerAngles = new Vector3(0, 0, -item.transform.localEulerAngles.z);
            })
            .SetLoops(-1, LoopType.Incremental);
    }
    
    public void OnClickItemGold(GameObject item)
    {
        if (canClick)
        {
            canClick = false;
            currentItemClick = item;

            InfoBet infoBet = new()
            {
                ReqSpecGame = (int)SiXiangGame.Goldpick,
            };
            DataSender.SendMatchState((long)OpCodeRequest.Spin, infoBet.ToByteArray());
            // SocketSend.sendGoldPickSlotSixiang(Globals.ACTION_SLOT_SIXIANG.goldPick);
            DOTween.Kill("autoPlay");
            DOTween.Sequence()
                .AppendInterval(0.75f)
                .AppendCallback(() =>
                {
                    canClick = true;
                })
                .AppendInterval(3.0f).AppendCallback(() =>
                {
                    AutoPlay();
                }).SetId("autoPlay");
        }
    }
    
    public void SixiangView_OnUpdateTable(SlotSixiangView.OnUpdateTableEventArgs e)
    {
        SlotDesk data = e.data;
        if (data.SpinSymbols.Count == 0) return;
        SpinSymbol item = data.SpinSymbols[0];

        if (currentItemClick != null)
        {
            GoldPickItem currentItemComp = currentItemClick.GetComponent<GoldPickItem>();
            SkeletonGraphic spineItem = currentItemComp.Spine;
            spineItem.gameObject.SetActive(true);
            currentItemComp.ImageBackGround.enabled = false;
            currentItemComp.Button.interactable = false;
            currentItemComp.ImageBackGround.raycastTarget = false;
            currentItemComp.ImageItem.gameObject.SetActive(false);
            currentItemClick.transform.SetAsLastSibling();
            long coinAmount = item.WinAmount;
            remainPick = (int)data.NumSpinLeft;
            textRemainingPick.text = remainPick + " Remaining Picks";

            if (coinAmount != 0)
            {
                SoundManager.Instance.PlayEffectFromPath(SoundSlot.CLICK_ITEM_WIN);
                TextMeshProUGUI textChipWin = currentItemComp.TextMoney;
                textChipWin.gameObject.SetActive(true);
                textChipWin.text = item.Symbol switch
                {
                    SiXiangSymbol.GoldPickJpMinor => "MINOR",
                    SiXiangSymbol.GoldPickJpMajor => "MAJOR",
                    SiXiangSymbol.GoldPickJpMega => "MEGA",
                    _ => Utility.FormatMoney(coinAmount, true),
                };
                textChipWin.transform.localScale = new Vector2(0, 0);
                textChipWin.transform.DOScale(new Vector2(1, 1), 0.2f).SetEase(Ease.OutBack);
                Utility.PlayAnimationByPath(spineItem, LUCKY_GOLD_ITEM_ANIMATION_PATH, "animation", false);
                gameView.UpdateTotalChipWinValue();
            }
            else
            {
                SoundManager.Instance.PlayEffectFromPath(SoundSlot.CLICK_ITEM_MISS);
                Utility.PlayAnimationByPath(spineItem, LUCKY_GOLD_NICE_TRY_ANIMATION_PATH, "eng", false);
            }


            if (data.IsFinishGame)
            {
                //Globals.Config.tweenNumberToNumber(SiXiangView.instance.lbChipWins, (int)data["winAmount"], totalWinAmount);
                // totalWinAmount = (long)data["winAmount"];
                DOTween.Sequence()
                    .AppendInterval(2.0f)
                    .AppendCallback(() =>
                    {
                        ShowAnimationResult(data.GameReward.TotalChipsWinByGame);
                    });
                DOTween.Kill("initRainItem");
                DOTween.Kill("autoPlay");
            }
            currentItemClick = null;
        }
    }
    
    private void ShowAnimationResult(long totalWinAmount)
    {
        AudioSource soundMoney = SoundManager.Instance.PlayEffectFromPath(SoundSlot.COUNGTING_MONEY_START);
        animationResult.transform.parent.gameObject.SetActive(true);

        Utility.PlayAnimation(animationResult, "eng", false);
        // textTotalWin.SetValue(totalWinAmount, false, 0.85f);
        buttonConfirm.gameObject.SetActive(false);
        textTotalWin.SetValue(totalWinAmount, true, 2.0f, "", () =>
        {
            soundMoney.Stop();
            SoundManager.Instance.PlayEffectFromPath(SoundSlot.COUNGTING_MONEY_END);
        });
        DOTween.Sequence()
            .AppendInterval(2.0f);
        buttonConfirm.gameObject.SetActive(true);
   
        DOTween.Sequence()
            .AppendInterval(10.0f)
            .AppendCallback(() =>
            {
                if (gameObject.activeSelf)
                {
                    OnClickCollect();
                }
            }).SetId("autoEnd");
        
    }
    
    private GameObject CreateItemGold()
    {
        GameObject item = itemPool.Get();
        GoldPickItem itemComp = item.GetComponent<GoldPickItem>();
        itemComp.ImageBackGround.enabled = true;
        itemComp.Spine.gameObject.SetActive(false);
        itemComp.Button.interactable = true;
        itemComp.TextMoney.gameObject.SetActive(false);
        itemComp.ImageBackGround.raycastTarget = true;
        itemComp.ImageItem.gameObject.SetActive(true);
        if (!listItem.Contains(item))
        {
            listItem.Add(item);
        }
        item.transform.SetAsLastSibling();
        return item;
    }
   
    private void RemoveItem(GameObject item)
    {
        DOTween.Kill(item.transform);
    }
    
    public void OnClickCollect()
    {
        DOTween.Kill("autoPlay");
        DOTween.Kill("autoEnd");
        DOTween.Kill("initRainItem");

        animationResult.transform.parent.gameObject.SetActive(false);
        buttonConfirm.gameObject.SetActive(false);
        gameView.ShowAnimationCutScene(true);
        remainPick = 20;
        listItem.ForEach(item =>
        {
            RemoveItem(item);
        });
        listItem.Clear();
        gameObject.SetActive(false);
    }

}
