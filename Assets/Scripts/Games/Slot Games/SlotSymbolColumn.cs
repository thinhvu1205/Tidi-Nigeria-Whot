using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Globals;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using UnityEngine;


public class SlotSymbolColumn : MonoBehaviour
{
    [SerializeField] protected List<SlotSymbol> listSymbols;
    [SerializeField] protected SkeletonGraphic animationThirdScatter;
    protected int columnIndex = 0;
    public bool IsLastColumn { get { return columnIndex == 4; } }
    public bool IsSpinning { get; set; } = false;
    public bool IsShowingThirdScatter { get; set; } = false;
    public float ExtraTime { get; set; } = 0f;
    public List<int> FinishView { get; private set; } = new();
    public List<int> FinishSymbolView { get; private set; } = new();
    public float speedThirdScatterAnimation = 0.05f;
    protected const float DEFAULT_SPIN_DURATION = 0.5f;
    protected const float SPEED_NORMAL = 0.12f;
    protected const float SPEED_AUTO = 0.08f;
    protected BaseSlotSymbolView slotView;

    public float PositionOutScreen { get; private set; } = -408f;
    public float PositionReset { get; private set; } = 352f;
    public int StepMove { get; private set; } = 170;


    protected virtual void Start()
    {
        // SPEED_TYPE["NORMAL"] = 0.08f;
        // SPEED_TYPE["AUTO"] = 0.05f;
    }

    public void SetInfo(BaseSlotSymbolView slotView, int index)
    {
        this.slotView = slotView;
        columnIndex = index;
    }

    public void SetFinishView(List<int> symbolIdArray)
    {
        FinishView = new List<int>(symbolIdArray);
        FinishSymbolView = new List<int>(symbolIdArray);
     
    }

    public void UpdateStartViewUI()
    {
        listSymbols[0].SetRandomSprite();
        for (int i = 0; i < FinishView.Count; i++)
        {
            listSymbols[i + 1].SetSprite(FinishView[i]);
        }
    }

    public void StartSpin(SpinType spinType)
    {
        IsSpinning = true;
        StartCoroutine(SpinDuration());

        float speed = spinType == SpinType.NORMAL ? SPEED_NORMAL : SPEED_AUTO;
        listSymbols.ForEach((symbol) =>
        {
            symbol.StartSpin(speed);
        });
    }

    private IEnumerator SpinDuration()
    {
        float delayBetweenColumns = slotView.GetSpinType() == SpinType.NORMAL ? 0.4f : 0.35f;
        float defaultSpinDuration = slotView.GetSpinType() == SpinType.NORMAL ? DEFAULT_SPIN_DURATION : DEFAULT_SPIN_DURATION * 0.75f;
        float duration = defaultSpinDuration + delayBetweenColumns * (columnIndex - 1);

        float timer = 0f;
        while (timer < duration + ExtraTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        IsSpinning = false;
    }
    public void SetRandomView()
    {
        listSymbols.ForEach((sym) =>
        {
            int randomID = UnityEngine.Random.Range(0, 9);
            sym.SetSprite(randomID);
        });
    }
    public virtual void OnColumnStop()
    {
        // animationThirdScatter.gameObject.SetActive(false);
        listSymbols.Sort((a, b) =>
        {
            return a.IndexSymbol - b.IndexSymbol;
        });
        if (FinishView.Contains(9))
        {
            IncreaseScatterCount();

            if (GetScatterCount() >= 2)
            {
                CheckThirdScatter();
            }
        }

        if (IsLastColumn)
        {
            // slotView.activeAllSymbol();
            slotView.OnStopSpin();
        }
    }
    public void prepareStop()
    {
 
    }

    public void ShowAnimationWild()
    {
        int wildIndex = FinishView.FindIndex((item) => item == 10);
        SlotSymbol symbol = listSymbols[wildIndex + 1];
        symbol.ShowSpineWild();
    }

    public void ShowSpreadWild()
    {
        int wildIndex = FinishView.FindIndex((item) => item == 10);
        if (wildIndex != -1)
        {
            Vector2 startPosition = GetWildPosition();
            float scaleTime = slotView.GetSpinType() == SpinType.NORMAL ? 0.75f : 1.25f;
            float animationTime = slotView.GetSpinType() == SpinType.NORMAL ? 1.5f : 1.25f;
            for (int i = 0; i < listSymbols.Count; i++)
            {
                if (i == wildIndex + 1 || i == 0) continue;
                SlotSymbol symbol = listSymbols[i];
                symbol.ShowEffectSpreadWild(startPosition, scaleTime, animationTime);
            }
        }
    }

    public void ShowThirdScatter()
    {
        animationThirdScatter.gameObject.SetActive(true);
        Utility.PlayAnimation(animationThirdScatter, "animation", true);
    }

    public void HideThirdScatter()
    {
        animationThirdScatter.gameObject.SetActive(false);
    }

    public void CheckThirdScatter()
    {
        slotView.CheckThirdScatter(columnIndex);
    }

    public void UpdateThirdScatterSpeed()
    {
        listSymbols.ForEach((symbol) => symbol.UpdateThirdScatterSpeed());
    }

    public void HideAllSpine()
    {
        listSymbols.ForEach((symbol) =>
        {
            symbol.HideSpine();
        });
    }

    public bool HasWild()
    {
        return FinishView.Contains(10);
    }

    public void SetLightAllSymbols()
    {
        listSymbols.ForEach((symbol) =>
        {
            symbol.Sprite.color = Color.white;
        });
    }

    public void SetDarkAllSymbols()
    {
        listSymbols.ForEach((symbol) =>
        {
            symbol.Sprite.color = Color.gray;
        });
    }
    public void Reset()
    {
        IsSpinning = false;
        IsShowingThirdScatter = false;
        animationThirdScatter.gameObject.SetActive(false);
        listSymbols.ForEach((symbol) =>
        {
            symbol.Reset();
            symbol.HideBackgroundWin();
        });
        speedThirdScatterAnimation = 0.05f;
        ExtraTime = 0f;
        HideAllSpine();
    }

    public void SetAnimationForItemAtIndex(int index)
    {
        SlotSymbol symbol = listSymbols[index + 1];
        symbol.SetSpine();
        symbol.ShowBackgroundWin();
        DOTween.Kill(listSymbols[index + 1]);
        DOTween.Sequence().AppendInterval(2.0f).AppendCallback(() =>
        {
            symbol.HideBackgroundWin();
            symbol.HideSpine();
            // listSymbols[index].SetSp(listSymbols[index].id);
        }).SetTarget(listSymbols[index + 1]);
    }
    public Vector2 GetItemPositionAtIndex(int index)
    {
        return listSymbols[index + 1].transform.position;
    }

    public Vector2 GetWildPosition()
    {
        int wildIndex = FinishView.FindIndex((item) => item == 10);
        Debug.Log("WILD INDEX: " + wildIndex);
        return listSymbols[wildIndex + 1].transform.position;
    }
    
    public int GetScatterCount() => slotView.ScatterCount;
    public void IncreaseScatterCount()
    {
        slotView.ScatterCount++;
    }
}
