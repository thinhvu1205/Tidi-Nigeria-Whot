using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Spine.Unity;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Random = UnityEngine.Random;
using Globals;
using UnityEngine.Events;
using Api;
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

    private bool isRaining = true;
    private TextNumberControl lbWinAmount;
    private UnityEngine.Pool.ObjectPool<GameObject> itemPool;
    private RectTransform itemConTainerRect;
    private SlotSixiangView gameView;
    private GameObject currentItemClick;
    private int remainPick = 20;
    private bool isFinished = false;
    private long totalWinAmount = 0;
    private long userAmount = 0;
    private bool canClick = true;
    private List<GameObject> listItem = new();
    public bool isAutoPlay = true;
    private bool isSelectBonusGame = false;

    private void Awake()
    {
        itemConTainerRect = itemContainer.GetComponent<RectTransform>();

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
    }
    
    private void OnEnable()
    {
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

    public void SetInfo(SlotSixiangView sixiangView)
    {
        gameView = sixiangView;
    }
    
    private void AutoPlay()
    {
        Debug.Log("onAutoPlay");
        GameObject itemAuto = listItem.Find((item) =>
            {
                return (item.transform.localPosition.y > 0 && item.transform.localPosition.y < 250 && item.gameObject.activeSelf && (item.transform.localPosition.x > -350 && item.transform.localPosition.x < 350));
            });
        if (itemAuto != null)
        {
            OnClickItemGold(itemAuto);
        }
        else
        {
            DOTween.Sequence().AppendInterval(1.0f).AppendCallback(() =>
            {
                AutoPlay();
            }).SetId("autoPlay");
        }
    }
    
    public void Show(SlotSixiangView SiXiangView)
    {
        // GameObject bottom = Instantiate(SlotSixiangView.Instance.transform.Find("Bottom").gameObject, transform);

        // lbWinAmount = bottom.transform.Find("lbTotalWin").GetComponent<TextNumberControl>();
        // lbWinAmount.Text = Utility.FormatNumber(SlotSixiangView.Instance.winAmount);
        // Destroy(bottom.transform.Find("infoBar").gameObject);
        // bottom.transform.SetSiblingIndex(transform.Find("EffectContainer").GetSiblingIndex() - 1);
        // lbRemainPick.text = remainPick + " Remaining Picks";
        // gameView = SiXiangView;
        // luckyGoldTask = new UniTaskCompletionSource();
        // SiXiangView.gameState = BaseSlotSymbolView.GAME_STATE.SHOWING_RESULT;
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
    
    public void setResult(JObject data)
    {
        if (currentItemClick != null)
        {

            GoldPickItem currentItemComp = currentItemClick.GetComponent<GoldPickItem>();
            SkeletonGraphic spineItem = currentItemComp.Spine;
            currentItemComp.ImageBackGround.enabled = false;
            currentItemComp.Button.interactable = false;
            long coinAmount = (long)data["coinAmount"];
            userAmount = (long)data["userAmount"];
            isFinished = (bool)data["isFinished"];
            totalWinAmount = (int)data["winAmount"];
            isSelectBonusGame = (bool)data["isSelectBonusGame"];

            if (coinAmount != 0)
            {
                // SoundManager.instance.playEffectFromPath(Globals.SOUND_SLOT_BASE.CLICK_ITEM_WIN);
                TextMeshProUGUI lbChipWin = currentItemComp.TextMoney;
                lbChipWin.gameObject.SetActive(true);
                switch ((int)data["jackpot"])
                {
                    case 0:
                        lbChipWin.text = Utility.FormatMoney(coinAmount, true);
                        break;
                    case 1:
                        lbChipWin.text = "MINOR";
                        break;
                    case 2:
                        lbChipWin.text = "MAJOR";
                        break;
                    case 3:
                        lbChipWin.text = "MEGA";
                        break;
                    case 4:
                        lbChipWin.text = "GRAND";
                        break;
                }
                if ((int)data["jackpot"] == 0)
                {
                    lbChipWin.fontMaterial = materialText[0];
                }
                else
                {
                    lbChipWin.fontMaterial = materialText[1];
                }
                //Globals.Config.tweenNumberToNumber(SiXiangView.instance.lbChipWins, (int)data["winAmount"], totalWinAmount);
                // SlotSixiangView.Instance.textChipWin.SetValue(totalWinAmount, true);
                lbWinAmount.SetValue(totalWinAmount, true);
                lbChipWin.transform.localScale = new Vector2(0, 0);
                lbChipWin.transform.DOScale(new Vector2(1, 1), 0.2f).SetEase(Ease.OutBack);
                // spineItem.skeletonDataAsset = UIManager.instance.loadSkeletonData("GameView/SiXiang/Spine/GoldPickItem/skeleton_SkeletonData");
                spineItem.Initialize(true);
                spineItem.AnimationState.SetAnimation(0, "animation", false);
            }
            else
            {
                // SoundManager.instance.playEffectFromPath(Globals.SOUND_SLOT_BASE.CLICK_ITEM_MISS);
                // spineItem.skeletonDataAsset = UIManager.instance.loadSkeletonData("GameView/SiXiang/Spine/LuckyGoldTryAgain/skeleton_SkeletonData");
                spineItem.Initialize(true);
                spineItem.AnimationState.SetAnimation(0, "eng", false);
            }
            spineItem.gameObject.SetActive(true);
            remainPick = (int)data["numberOfPick"];
            textRemainingPick.text = remainPick + " Remaining Picks";
            currentItemComp.ImageBackGround.raycastTarget = false;
            currentItemComp.ImageItem.gameObject.SetActive(false);
            currentItemClick.transform.SetAsLastSibling();
            if (isFinished)
            {
                //Globals.Config.tweenNumberToNumber(SiXiangView.instance.lbChipWins, (int)data["winAmount"], totalWinAmount);
                totalWinAmount = (long)data["winAmount"];
                DOTween.Sequence()
                    .AppendInterval(2.0f)
                    .AppendCallback(() =>
                    {
                        ShowAnimationResult();
                    });
                DOTween.Kill("initRainItem");
                DOTween.Kill("autoPlay");
            }
            currentItemClick = null;
        }
    }
    
    private void ShowAnimationResult()
    {
        // animResult.skeletonDataAsset = UIManager.instance.loadSkeletonData("GameView/SiXiang/Spine/BigWinGoldPick/skeleton_SkeletonData");
        // AudioSource soundMoney = SoundManager.instance.playEffectFromPath(Globals.SOUND_SLOT_BASE.COUNGTING_MONEY_START);
        animationResult.transform.parent.gameObject.SetActive(true);

        Utility.PlayAnimation(animationResult, "eng", false);
        // textTotalWin.SetValue(totalWinAmount, false, 0.85f);
        buttonConfirm.gameObject.SetActive(false);
        textTotalWin.SetValue(totalWinAmount, true, 2.0f, "", () =>
        {
            // soundMoney.Stop();
            // SoundManager.instance.playEffectFromPath(Globals.SOUND_SLOT_BASE.COUNGTING_MONEY_END);
        });
        DOTween.Sequence()
            .AppendInterval(2.0f);
        buttonConfirm.gameObject.SetActive(true);
        if (gameView.GetSpinType() == SpinType.AUTO)
        {
            DOTween.Sequence()
                .AppendInterval(3.0f)
                .AppendCallback(() =>
                {
                    if (gameObject.activeSelf)
                    {
                        OnClickCollect();
                    }
                }).SetId("autoEnd");
        }
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
        itemPool.Release(item);
        DOTween.Kill(item.transform);
    }
    
    public void OnClickCollect()
    {
        DOTween.Kill("autoPlay");
        DOTween.Kill("autoEnd");
        DOTween.Kill("initRainItem");

        animationResult.transform.parent.gameObject.SetActive(false);
        buttonConfirm.gameObject.SetActive(false);
        gameView.ShowAnimationCutScene();
        gameView.HideBackgroundGoldPick();

        // gameView.setStateNodeGameForLuckyGold(true);
        totalWinAmount = 0;
        remainPick = 20;
        listItem.ForEach(item =>
        {
            RemoveItem(item);
        });
        listItem.Clear();
        isFinished = false;
        gameObject.SetActive(false);
        // await gameView.endMinigame(dataEnd);
    }

}
