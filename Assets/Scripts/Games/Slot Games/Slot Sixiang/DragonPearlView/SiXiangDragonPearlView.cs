using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Linq;
using DG.Tweening;
using System;
using Api;
using Cysharp.Threading.Tasks;
// using Facebook.Unity;
using Globals;
using System.Linq;

public class SiXiangDragonPearlView : MonoBehaviour
{
    [SerializeField] private Sprite[] listBackgroundItem;
    [SerializeField] private GameObject itemGoldPrefab, itemPrefab;
    [SerializeField] private Transform itemContainer;
    private List<List<DragonPearlItem>> listItem = new();
    private List<SpinSymbol> listSpinSymbol = new();
    private List<GameObject> listItemGold = new();
    public SlotSixiangView GameView { get; private set; }
    public bool IsWinWarriorEye { get; private set; }
    public bool IsWinTigerEye { get; private set; }
    public bool IsWinBirdEye { get; private set; }
    private bool hasInitFirst6Gold = false, isFinishGame = false, isWinGrandJackpot = false;
    private bool isAutoPlay = true;
    public UnityEngine.Pool.ObjectPool<GameObject> itemGoldPool;

    private void Awake()
    {
        itemGoldPool = new UnityEngine.Pool.ObjectPool<GameObject>(
            createFunc: () =>
            {
                var item = Instantiate(itemGoldPrefab, transform);
                item.SetActive(false); 
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
            maxSize: 20             
        );  
        InitItems();
    }

    private void OnEnable()
    {
        DOTween.Sequence()
            .AppendInterval(10f)
            .AppendCallback(() =>
            {
                if (isAutoPlay)
                {
                    // SlotSixiangView.Instance.onClickSpinDP();
                }
            });
    }

    private void OnDisable()
    {
        Reset();
        GameView.OnUpdateTable -= SixiangView_OnUpdateTable;
    }

    public void SetInfo(SlotSixiangView slotSixiangView)
    {
        hasInitFirst6Gold = false;
        GameView = slotSixiangView;
        GameView.OnUpdateTable += SixiangView_OnUpdateTable;
        GameView.UpdateTotalChipWinValue();
        GameView.SetDarkAllItems();
        StartView6Gold();
    }

    private void InitItems()
    {
        itemPrefab.GetComponent<DragonPearlItem>().SetDragonPearlView(this);
        for (int i = 1; i < 15; i++)
        {
            DragonPearlItem item = Instantiate(itemPrefab, itemContainer.transform).GetComponent<DragonPearlItem>();
            item.SetBackground(listBackgroundItem[i]);
            item.SetDragonPearlView(this);
        }
        for (int i = 0; i < 5; i++)
        {
            List<DragonPearlItem> list = new()
            {
                itemContainer.transform.GetChild(i).GetComponent<DragonPearlItem>(),
                itemContainer.transform.GetChild(i + 5).GetComponent<DragonPearlItem>(),
                itemContainer.transform.GetChild(i + 10).GetComponent<DragonPearlItem>()
            };
            listItem.Add(list);
        }
    }

    public void SixiangView_OnUpdateTable(BaseSlotSymbolView.OnUpdateTableEventArgs e)
    {
        SlotDesk data = e.data;
        listSpinSymbol = data.SpinSymbols.ToList();
        // List<SpinSymbol> listSpinSymbol = data.Matrix.SpinLists.ToList();
        isFinishGame = data.IsFinishGame;
        IsWinWarriorEye = listSpinSymbol.Any(symbol => symbol.Symbol == SiXiangSymbol.DragonpearlEyeWarrior);
        IsWinTigerEye = listSpinSymbol.Any(symbol => symbol.Symbol == SiXiangSymbol.DragonpearlEyeTiger);
        IsWinBirdEye = listSpinSymbol.Any(symbol => symbol.Symbol == SiXiangSymbol.DragonpearlEyeBird);
        isWinGrandJackpot = data.WinJp == WinJackpot.Grand;
    }

    public void OnStopSpin()
    {
        GameView.UpdateTotalChipWinValue();
        GameView.UpdateDragonPearlFreeSpinLeft();

        Sequence mainSequence = DOTween.Sequence();

        foreach (SpinSymbol spinSymbol in listSpinSymbol)
        {
            DragonPearlItem dragonPearlItem = listItem[spinSymbol.Col][spinSymbol.Row];

            if (dragonPearlItem.Symbol != SiXiangSymbol.Unspecified)
                continue;

            // Gọi và join từng sequence
            Sequence itemSequence = dragonPearlItem.SetInfo(spinSymbol);
            mainSequence.Join(itemSequence);
        }

        // Khi toàn bộ sequence hoàn tất, gọi NextTween
        mainSequence.OnComplete(() =>
        {
            if (isFinishGame)
            {
                mainSequence.AppendInterval(0.5f);
                Debug.Log("FINISH GAME");
                GameView.OnFinishDragonPearl(isWinGrandJackpot);
            }
            else
            {
                Debug.Log("SPIN TIEP");
                GameView.NextTween();
            }
        });
    }

    public void StartView6Gold()
    {
        if (hasInitFirst6Gold) return;
        Debug.Log("START VIEW 6 GOLD");
        hasInitFirst6Gold = true;
        Reset();
        listItemGold.Clear();
        List<SpinSymbol> listSpinSymbol = GameView.ListSpinSymbol;
        Sequence sequence = DOTween.Sequence();
        sequence.SetAutoKill(true);
        sequence.AppendInterval(0.7f);

        for (int i = 0; i < listSpinSymbol.Count; i++)
        {

            SpinSymbol data = listSpinSymbol[i];
            if (data.WinAmount <= 0) continue;
            GameObject itemGold = itemGoldPool.Get();

            listItemGold.Add(itemGold);
            // SoundManager.instance.playEffectFromPath(Globals.SOUND_SLOT_BASE.PEARL_RUNITEM);
            sequence
                .AppendCallback(() =>
                {
                    Vector2 posSymbol = transform.InverseTransformPoint(GameView.ListColumn[data.Col].GetItemPositionAtIndex(data.Row));
                    itemGold.transform.DOLocalMove(posSymbol, 0.5f).SetEase(Ease.OutSine);
                    itemGold.transform.DOScale(new Vector2(1.0f, 1.0f), 1.0f).SetEase(Ease.OutSine);
                })
                .AppendInterval(i != listSpinSymbol.Count - 1 ? 0.1f : 0.9f);
        }
        sequence
            .AppendInterval(1f)
            .OnComplete(() =>
            {
                foreach (GameObject item in listItemGold)
                {
                    itemGoldPool.Release(item);
                }
                foreach (SpinSymbol item in listSpinSymbol)
                {
                    if (item.WinAmount > 0)
                    {
                        DragonPearlItem dragonPearlItem = listItem[item.Col][item.Row];
                        dragonPearlItem.SetInfo(item);
                    }
                }
            });
    }
    
    private void Reset()
    {
        listItem.ForEach(col =>
        {
            col.ForEach(item =>
            {
                item.Reset();
            });
        });
        isWinGrandJackpot = false;
        isAutoPlay = true;
    }
    
    public Vector2 GetEyeWarriorPosition()
    {
        Vector2 position = Vector2.zero;
        listSpinSymbol.ForEach(data =>
        {
            if (data.Symbol == SiXiangSymbol.DragonpearlEyeWarrior)
            {
                position = GetItemPosition(data.Col, data.Row);
            }
        });
        return position;
    }
    
    public Vector2 GetItemPosition(int col, int row)
    {
        return listItem[col][row].transform.position;
    }

    public void SetDoubleItem()
    {
        GameView.ListSpinSymbol.ForEach(item =>
        {
            if (item.WinAmount > 0)
            {
                DragonPearlItem dragonPearlItem = listItem[item.Col][item.Row];
                dragonPearlItem.SetInfo(item, true);
            }
        });
        // dataPearl.ForEach(dataPearl =>
        // {
        //     if ((bool)dataPearl["isDoubled"] == true && isDPSpin == true)
        //     {
        //         int row = (int)dataPearl["row"];
        //         int col = (int)dataPearl["col"];
        //         DragonPearlItem item = listItem[col][row];
        //         item.setInfo(dataPearl, this);
        //         isWait = true;
        //     }
        // });
        // await UniTask.Delay(TimeSpan.FromSeconds(isWait ? 2.0f : 0));
    }
}
