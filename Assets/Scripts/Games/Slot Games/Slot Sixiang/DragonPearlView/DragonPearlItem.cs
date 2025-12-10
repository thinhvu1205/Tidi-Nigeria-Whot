using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using System;
using DG.Tweening;
using System.Threading;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;

public class DragonPearlItem : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] TextMeshProUGUI textChipValue;
    [SerializeField] TextMeshProUGUI textChipFSP;

    [SerializeField] Image imageBackground;
    [SerializeField] SkeletonGraphic spine;
    [SerializeField] List<Material> materialText = new();
    private SiXiangDragonPearlView dragonPearlView;
    public SiXiangSymbol Symbol { get; private set; } = SiXiangSymbol.Unspecified;
    public long WinAmount { get; private set; } = 0;

    private const string GOLD_ANIMATION_PATH = "SiXiang/Spine/DragonPearl/ItemGold/skeleton_SkeletonData";
    private const string ENVELOPE_ANIMATION_PATH = "SiXiang/Spine/DragonPearl/Lixi/skeleton_SkeletonData";
    private const string DRAGON_EYE_ANIMATION_PATH = "SiXiang/Spine/DragonPearl/ThanhLong/skeleton_SkeletonData";
    private const string TIGER_EYE_ANIMATION_PATH = "SiXiang/Spine/DragonPearl/BachHo/skeleton_SkeletonData";
    private const string TURTLE_EYE_ANIMATION_PATH = "SiXiang/Spine/DragonPearl/ChuTuoc/skeleton_SkeletonData";
    private const string EYE_BIRD_ANIMATION_PATH = "SiXiang/Spine/DragonPearl/HuyenVu/skeleton_SkeletonData";

    public void SetDragonPearlView(SiXiangDragonPearlView dragonPearlView)
    {
        this.dragonPearlView = dragonPearlView;
    }
    public Sequence SetInfo(SpinSymbol data, bool isDouble = false)
    {
        Symbol = data.Symbol;
        if (data.Symbol == SiXiangSymbol.Unspecified) return null;
        WinAmount = data.WinAmount;
        spine.gameObject.SetActive(false);
        spine.transform.localPosition = Vector2.zero;
        spine.transform.localScale = new Vector2(1, 1);

        Sequence sequence = DOTween.Sequence();
      
        if (WinAmount > 0 && data.Symbol != SiXiangSymbol.DragonpearlEyeDragon)
        {
            if (isDouble)
            {
                spine.gameObject.SetActive(true);
                WinAmount *= 2;
                sequence    
                    .AppendInterval(1f);
            }
            if (dragonPearlView.IsWinWarriorEye)
            {
                sequence
                    .AppendInterval(2f)
                    .AppendCallback(() =>
                    {
                        Vector2 tigerEyePosition = dragonPearlView.GetEyeWarriorPosition();
                        Vector2 positionItem = dragonPearlView.GetItemPosition(data.Col, data.Row);
                        GameObject itemGold = dragonPearlView.itemGoldPool.Get();

                        itemGold.transform.localPosition = dragonPearlView.transform.InverseTransformPoint(tigerEyePosition);
                        itemGold.transform.localScale = Vector2.one;
                        itemGold.transform.DOLocalMove(dragonPearlView.transform.InverseTransformPoint(positionItem), 1.0f).SetEase(Ease.OutSine).OnComplete(() =>
                        {
                            dragonPearlView.itemGoldPool.Release(itemGold);
                        });
                        
                    }).
                    AppendInterval(1f);
            }
            
            sequence
                .AppendCallback(() =>
                {
                    imageBackground.enabled = true;
                    Utility.PlayAnimationByPath(spine, GOLD_ANIMATION_PATH, "start", false);
                })
                .AppendInterval(0.6f)
                .AppendCallback(() =>
                {
                    Utility.PlayAnimationByPath(spine, GOLD_ANIMATION_PATH, "rung", false);
                    SoundManager.Instance.PlayEffectFromPath(SoundSlot.PEARL_Item_Normal);
                })
                .AppendInterval(0.33f)
                .AppendCallback(() =>
                {
                    dragonPearlView.GameView.UpdateDragonPearlFreeSpinLeft();
                    dragonPearlView.GameView.UpdateTotalChipWinValue();
                    textChipValue.fontMaterial = materialText[0];
                    textChipValue.gameObject.SetActive(true);
                    textChipValue.text = Utility.FormatMoney(WinAmount, true);
                    textChipValue.transform.localScale = Vector2.zero;
                    textChipValue.transform.DOScale(Vector2.one, 0.2f).SetEase(Ease.OutBack);
                    Utility.PlayAnimationByPath(spine, GOLD_ANIMATION_PATH, "normal", false);
                });
        }
        else
        {
            textChipValue.gameObject.SetActive(false);
            imageBackground.enabled = true;
            string soundSymbol = SoundSlot.PEARL_ITEM;
            string animationPath = "";
            switch (data.Symbol)
            {
                case SiXiangSymbol.DragonpearlEyeBird:
                    animationPath = EYE_BIRD_ANIMATION_PATH;
                    soundSymbol = SoundSlot.PEARL_Phoenix;
                    break; // + thêm số lượt quay 
                case SiXiangSymbol.DragonpearlEyeTiger:
                    animationPath = TIGER_EYE_ANIMATION_PATH;
                    soundSymbol = SoundSlot.PEARL_Tiger;
                    break; // x2 giá trị ở tất cả các ô
                case SiXiangSymbol.DragonpearlEyeWarrior:
                    animationPath = TURTLE_EYE_ANIMATION_PATH;
                    soundSymbol = SoundSlot.PEARL_Turtle;
                    break; // rơi 3 ngọc bất kì
                case SiXiangSymbol.DragonpearlEyeDragon:
                    animationPath = DRAGON_EYE_ANIMATION_PATH;
                    soundSymbol = SoundSlot.PEARL_Dragon;
                    break; // rơi 1 ngọc jackpot
            }
            sequence
                .AppendInterval(0.1f)
                .AppendCallback(() =>
                {
                    Utility.PlayAnimationByPath(spine, ENVELOPE_ANIMATION_PATH, "animation", false);
                })
                .AppendInterval(1f)
                .AppendCallback(() =>
                {
                    SoundManager.Instance.PlayEffectFromPath(soundSymbol);
                })
                .AppendInterval(0.1f)
                .AppendCallback(() =>
                {
                    Utility.PlayAnimationByPath(spine, animationPath, "animation", false);
                    spine.transform.localScale = new Vector2(0.9f, 0.9f);
                    if (!dragonPearlView.IsWinBirdEye)
                    {
                        dragonPearlView.GameView.UpdateDragonPearlFreeSpinLeft();
                    }
                    else
                    {
                        dragonPearlView.GameView.DragonPearlFreeSpinLeft += 1;
                        dragonPearlView.GameView.UpdateVisualDragonPearlFreeSpinLeft();

                    }
                    // Vector2 posSymbol = spine.transform.parent.InverseTransformPoint(SiXiangView.Instance.getPosSymbol((int)data["col"], (int)data["row"] + 1));
                    //  SpineItem.transform.localPosition = new Vector2(posSymbol.x + 2, posSymbol.y);
                });
            switch (data.Symbol)
            {
                case SiXiangSymbol.DragonpearlEyeBird:
                    {
                        sequence
                            .AppendInterval(2f)
                            .AppendCallback(() =>
                            {
                                textChipFSP.gameObject.SetActive(true);
                                textChipFSP.alpha = 1.0f;
                                textChipFSP.transform.localPosition = textChipFSP.transform.parent.InverseTransformPoint(transform.position);
                                textChipFSP.fontMaterial = materialText[1];
                                textChipFSP.text = "+3 freespin";
                                Vector2 posJump = textChipFSP.transform.parent.InverseTransformPoint(dragonPearlView.GameView.InfoSessionBar.transform.position);
                                textChipFSP.transform.DOLocalJump(posJump, 150, 1, 0.5f)
                                    .OnComplete(() =>
                                    {
                                        textChipFSP.transform.localPosition = Vector2.zero;
                                        textChipFSP.gameObject.SetActive(false);
                                        // dragonPearlView.GameView.DragonPearlFreeSpinLeft += 3;
                                        dragonPearlView.GameView.UpdateDragonPearlFreeSpinLeft(true);
                                    });
                                textChipFSP.DOFade(0, 0.5f).SetEase(Ease.InSine).SetId("fadeEffect");
                            });
                        break;
                    }
                case SiXiangSymbol.DragonpearlEyeTiger:
                    {
                        sequence
                            .AppendCallback(() =>
                            {
                                // dragonPearlView.GameView.DragonPearlFreeSpinLeft += 1;
                                dragonPearlView.GameView.UpdateDragonPearlFreeSpinLeft();
                            })
                            .AppendInterval(2f)
                            .AppendCallback(() =>
                            {
                                dragonPearlView.SetDoubleItem();
                            });
                        break;
                    }
                case SiXiangSymbol.DragonpearlEyeWarrior:
                    {
                        sequence
                            .AppendCallback(() =>
                            {
                                // dragonPearlView.GameView.DragonPearlFreeSpinLeft += 1;
                                dragonPearlView.GameView.UpdateDragonPearlFreeSpinLeft();
                            })
                            .AppendInterval(2f)
                            .AppendCallback(() =>
                            {
                                Utility.PlayAnimationByPath(spine, animationPath, "animation", false);

                            });
                        break;
                    }
                case SiXiangSymbol.DragonpearlEyeDragon:
                    {
                        string jackpotShakeAnimationName = "rung_" + GetJackpotAnimationName(data.WinJp);
                        string jackpotNormalAnimationName = "normal_" + GetJackpotAnimationName(data.WinJp);

                        sequence
                            .AppendCallback(() =>
                            {
                                // dragonPearlView.GameView.DragonPearlFreeSpinLeft += 1;
                                dragonPearlView.GameView.UpdateDragonPearlFreeSpinLeft();
                            })
                            .AppendInterval(2f)
                            .AppendCallback(() =>
                            {
                                Utility.PlayAnimationByPath(spine, GOLD_ANIMATION_PATH, jackpotShakeAnimationName, false);
                                spine.transform.localScale = Vector2.one;

                            })
                            .AppendInterval(1.3f)
                            // .AppendInterval(spine.Skeleton.Data.FindAnimation(jackpotShakeAnimationName).Duration)
                            .AppendCallback(() =>
                            {
                                Debug.Log("VAO DAY K ???");
                                textChipValue.gameObject.SetActive(true);
                                textChipValue.fontMaterial = materialText[1];
                                textChipValue.text = data.WinJp switch
                                {
                                    WinJackpot.Minor => "MINOR",
                                    WinJackpot.Major => "MAJOR",
                                    WinJackpot.Mega => "MEGA",
                                    WinJackpot.Grand => "GRAND",
                                    _ => ""
                                };
                                spine.AnimationState.SetAnimation(0, jackpotNormalAnimationName, true);
                                dragonPearlView.GameView.UpdateTotalChipWinValue();
                            });
                        break;
                    }
            }
        }
        return sequence;
    }

    public void SetupEye(SpinSymbol data)
    {
        string animationPath = "";
        string jackpotNormalAnimationName = "normal_" + GetJackpotAnimationName(data.WinJp);
        imageBackground.enabled = true;
        switch (data.Symbol)
        {
            case SiXiangSymbol.DragonpearlEyeBird:
                animationPath = EYE_BIRD_ANIMATION_PATH;
                break; // + thêm số lượt quay 
            case SiXiangSymbol.DragonpearlEyeTiger:
                animationPath = TIGER_EYE_ANIMATION_PATH;
                break; // x2 giá trị ở tất cả các ô
            case SiXiangSymbol.DragonpearlEyeWarrior:
                animationPath = TURTLE_EYE_ANIMATION_PATH;
                break; // rơi 3 ngọc bất kì
            case SiXiangSymbol.DragonpearlEyeDragon:
                {
                    textChipValue.gameObject.SetActive(true);
                    textChipValue.fontMaterial = materialText[1];
                    textChipValue.text = data.WinJp switch
                    {
                        WinJackpot.Minor => "MINOR",
                        WinJackpot.Major => "MAJOR",
                        WinJackpot.Mega => "MEGA",
                        WinJackpot.Grand => "GRAND",
                        _ => ""
                    };
                }
                break; // rơi 1 ngọc jackpot
        }
        if (data.Symbol == SiXiangSymbol.DragonpearlEyeDragon)
        {
            Utility.PlayAnimationByPath(spine, GOLD_ANIMATION_PATH, jackpotNormalAnimationName, false);
            // spine.AnimationState.SetAnimation(0, jackpotNormalAnimationName, true);
        }
        else
        {
            Utility.PlayAnimationByPath(spine, animationPath, "animation", false);  
        }
    }
    
    private string GetJackpotAnimationName(WinJackpot winJackpot)
    {
        string animationName = winJackpot switch
        {
            WinJackpot.Minor => "xanh",
            WinJackpot.Major => "bien",
            WinJackpot.Mega => "tim",
            WinJackpot.Grand => "do",
            _ => "",
        };
        return animationName;
    }
    public void Reset()
    {
        imageBackground.enabled = false;
        spine.gameObject.SetActive(false);
        textChipValue.gameObject.SetActive(false);
        textChipFSP.gameObject.SetActive(false);
        Symbol = SiXiangSymbol.Unspecified;
        WinAmount = 0;
    }
    public void SetBackground(Sprite spr)
    {
        imageBackground.sprite = spr;
    }

}
