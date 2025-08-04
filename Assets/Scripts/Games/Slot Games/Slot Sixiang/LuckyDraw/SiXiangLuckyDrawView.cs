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
using Unity.VisualScripting;
using Globals;
using Proto;
using Google.Protobuf;

public class SiXiangLuckyDrawView : MonoBehaviour
{

    public enum JackpotType
    {
        MINOR = 1,
        MAJOR = 2,
        MEGA = 3,
        GRAND = 4,
        NORMAL = 0
    }
    [SerializeField] private GameObject columnPrefab;
    [SerializeField] private Transform columnContainer, effectContainer;
    [SerializeField] private SkeletonGraphic animationResult;
    [SerializeField] private Button buttonCollect;
    [SerializeField] private TextNumberControl textTotalWin;

    private readonly List<LuckyDrawItem> listItem = new();
    private readonly List<LuckyDrawItem> listItemRemain = new();
    private SlotSixiangView gameView;
    private JackpotType jackpotType = JackpotType.NORMAL;
    private long winAmount = 0;
    private bool isAutoPlay = true;
    private bool canClick = true;
    private const string SEQUENCE_ID_AUTOPLAY = "autoPlay";
    private const string SEQUENCE_ID_AUTOEND = "autoEnd";
    private const string RESULT_WIN_NORMAL_ANIMATION_PATH = "SiXiang/Spine/BigWinGoldPick/skeleton_SkeletonData";
    private const string RESULT_WIN_JACKPOT_ANIMATION_PATH = "SiXiang/Spine/LuckyDraw/BigWin/skeleton_SkeletonData";

    private void Awake()
    {
        InitDrawItems();
        effectContainer.gameObject.SetActive(false);
        DOTween.Sequence(transform)
            .AppendInterval(10.0f)
            .AppendCallback(() =>
            {
                OnPlayAuto();
            }).SetId(SEQUENCE_ID_AUTOPLAY);
    }

    private void OnDisable()
    {
        gameView.OnUpdateTable -= SixiangView_OnUpdateTable;
    }

    public void SetInfo(SlotSixiangView slotSixiangView)
    {
        gameView = slotSixiangView;
        gameView.OnUpdateTable += SixiangView_OnUpdateTable;
        gameView.UpdateTotalChipWinValue();
    }

    private void InitDrawItems()
    {
        for (int i = 0; i < 5; i++)
        {
            if (i > 0)
            {
                Instantiate(columnPrefab, columnContainer);
            }
            Transform col = columnContainer.GetChild(i);
            for (int j = 0; j < col.transform.childCount; j++)
            {
                listItem.Add(col.GetChild(j).GetComponent<LuckyDrawItem>());
            }
        }
        listItemRemain.AddRange(listItem);
    }

    public void SixiangView_OnUpdateTable(BaseSlotSymbolView.OnUpdateTableEventArgs e)
    {
        SlotDesk data = e.data;
        SpinSymbol item = data.SpinSymbols[0];
        int itemIndex = item.Index;
        bool isFinishGame = data.IsFinishGame;
        winAmount = data.GameReward.TotalChipsWinByGame;
        listItem[itemIndex].SetResult(item, isFinishGame);
        gameView.UpdateTotalChipWinValue();

        CheckWinJackpot(data.WinJp);
        DOTween.Kill(SEQUENCE_ID_AUTOPLAY);

        if (isFinishGame)
        {
            DOTween.Sequence()
                .AppendInterval(1.0f)
                .AppendCallback(() =>
                {
                    if (jackpotType != 0)
                    {
                        ShowEffectItemJackpot();
                    }
                })
                .AppendInterval(3.0f)
                .AppendCallback(() =>
                {
                    ShowResult();
                });
     
        }
        else
        {
            DOTween
                .Sequence(transform)
                .AppendInterval(4.0f)
                .AppendCallback(() =>
                {
                    isAutoPlay = true;
                    OnPlayAuto();
                }).SetId(SEQUENCE_ID_AUTOPLAY);
        }
    
    }
    
    public void OnClickItem(LuckyDrawItem item)
    {
        if (canClick)
        {
            // SoundManager.instance.playEffectFromPath(Globals.SOUND_SLOT_BASE.CLICK);
            InfoBet infoBet = new() 
            {
                Id = listItem.IndexOf(item)
            };
            DataSender.SendMatchState((long)OpCodeRequest.Spin, infoBet.ToByteArray());
            listItemRemain.Remove(item);
            isAutoPlay = false;
            canClick = false;
            DOTween.Sequence().AppendInterval(0.75f).AppendCallback(() =>
            {
                canClick = true;
            });
        }

    }
    
    private void OnPlayAuto()
    {
        if (isAutoPlay)
        {
            int randomIndex = Random.Range(0, listItemRemain.Count);
            OnClickItem(listItemRemain[randomIndex]);
        }
    }

    public void OnClickCollect()
    {
        DOTween.Kill(SEQUENCE_ID_AUTOEND);
        animationResult.transform
            .DOScale(new Vector2(0.8f, 0.8f), 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                buttonCollect.gameObject.SetActive(false);
                effectContainer.gameObject.SetActive(false);
                gameView.ShowAnimationCutScene(true);
                foreach(LuckyDrawItem item in listItem)
                {
                    item.Reset();
                }
            });
    }

    public void ShowResult()
    {
        string soundPathStart = "";
        string soundPathEnd = "";
        // if (jackpotType != 0)
        // {
        //     spineResult.skeletonDataAsset = UIManager.instance.loadSkeletonData("GameView/SiXiang/Spine/LuckyDraw/BigWin/skeleton_SkeletonData");
        //     // soundPathStart = Globals.SOUND_SLOT_BASE.WIN_JACKPOT_START;
        //     // soundPathEnd = Globals.SOUND_SLOT_BASE.WIN_JACKPOT_END;
        // }
        // else
        // {
        //     spineResult.skeletonDataAsset = UIManager.instance.loadSkeletonData("GameView/SiXiang/Spine/BigWinGoldPick/skeleton_SkeletonData");
        //     // soundPathStart = Globals.SOUND_SLOT_BASE.COUNGTING_MONEY_START;
        //     // soundPathEnd = Globals.SOUND_SLOT_BASE.COUNGTING_MONEY_END;
        // }
        float duration = 0; 
        // AudioSource soundCount = SoundManager.instance.playEffectFromPath(soundPathStart);
       
        DOTween.Sequence()
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                effectContainer.gameObject.SetActive(true);
                if (jackpotType != JackpotType.NORMAL)
                {
                    Utility.PlayAnimationByPath(animationResult, RESULT_WIN_JACKPOT_ANIMATION_PATH, GetAnimationResultName(), false);
                    duration = animationResult.Skeleton.Data.FindAnimation(GetAnimationResultName()).Duration;
                }
                else
                {
                    Utility.PlayAnimationByPath(animationResult, RESULT_WIN_NORMAL_ANIMATION_PATH, "eng", false);
                    duration = 3f;
                }
                textTotalWin.SetValue(winAmount, true, duration * 0.85f, "", () =>
                {
                    // soundCount.Stop();
                    // SoundManager.instance.playEffectFromPath(soundPathEnd);
                });
            })
            .AppendInterval(duration)
            .AppendCallback(() =>
            {
                buttonCollect.gameObject.SetActive(true);
                if (gameView.GetSpinType() == SpinType.AUTO)
                {
                    DOTween.Sequence()
                        .SetId(SEQUENCE_ID_AUTOEND)
                        .AppendInterval(3.0f)
                        .AppendCallback(() =>
                        {
                            OnClickCollect();
                        });
                }

            });
        
        // await UniTask.Delay(TimeSpan.FromSeconds(duration));

    }
    
    private void OnDestroy()
    {
        DOTween.Kill(SEQUENCE_ID_AUTOEND);
    }
    
    private void ShowEffectItemJackpot()
    {
        // SoundManager.instance.playEffectFromPath(Globals.SOUND_SLOT_BASE.LUCKYDRAW_WIN_JACKPOT);
        listItem.ForEach(item =>
        {
            if (item.typeItem == jackpotType)
            {
                item.ShowEffectWinJackpot();
            }
        });
    }

    private void CheckWinJackpot(WinJackpot winJackpot)
    {
        jackpotType = winJackpot switch
        {
            WinJackpot.Minor => JackpotType.MINOR,
            WinJackpot.Major => JackpotType.MAJOR,
            WinJackpot.Mega => JackpotType.MEGA,
            WinJackpot.Grand => JackpotType.GRAND,
            _ => JackpotType.NORMAL,
        };
    }
    
    private string GetAnimationResultName()
    {
        string animationName;
        animationName = jackpotType switch
        {
            JackpotType.MINOR => "minor",
            JackpotType.MAJOR => "major",
            JackpotType.MEGA => "mega",
            JackpotType.GRAND => "grand",
            _ => "eng",
        };
        return animationName;
    }
}
