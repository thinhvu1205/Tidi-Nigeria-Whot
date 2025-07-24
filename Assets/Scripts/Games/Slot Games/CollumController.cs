using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using UnityEngine;

namespace Games.Slot_Games
{
    public class CollumController : MonoBehaviour
    {
        // Start is called before the first frame update
    [SerializeField]
    public BaseSlotXisiang slotView;
    [SerializeField]
    protected List<SymbolController> listSymbols;

    [SerializeField]
    protected SkeletonGraphic spineNFS;
    public float positionOutScreen = -408;
    public float positionReset = 352;
    public int stepMove = 170;
    public bool isStop = false;
    public int collumIndex = 0;
    public JObject SPEED_TYPE = new JObject();
    [HideInInspector]
    public List<int> listIDResult = new List<int>();
    private UniTaskCompletionSource collumSpinTask;
    public bool isNeerSpin = false;
    private bool isChangeSpeedNFS = false;
    public float speedNFS = 0.05f;
    private float timeNFS = 2.7f;

    protected virtual void Start()
    {
        SPEED_TYPE["NORMAL"] = 0.08f;
        SPEED_TYPE["AUTO"] = 0.05f;
    }

    public UniTask Stop(List<int> listID)
    {

        listIDResult.AddRange(listID);
        collumSpinTask = new UniTaskCompletionSource();
        if (isNeerSpin)
        {
            listSymbols.ForEach((sym) => { sym.Speed = (float)SPEED_TYPE["AUTO"]; });
            isChangeSpeedNFS = true;
            spineNFS.gameObject.SetActive(true);
            slowDownNFS();
            slotView.hideAllSymbol(10);
            listSymbols.ForEach((sym) => { sym.sprite.color = Color.white; });
            // SoundManager.instance.playEffectFromPath(SOUND_SLOT_BASE.NEAR_FREESPIN_START);
            DOTween.Sequence().AppendInterval(timeNFS).AppendCallback(() =>
            {
                listSymbols.ForEach((sym) => { sym.Speed = speedNFS; });
                isChangeSpeedNFS = false;
                isStop = true;
            });
        }
        else
        {
            isStop = true;
            // SoundManager.instance.playEffectFromPath(SOUND_SLOT_BASE.COLLUM_STOP);
        }
        return collumSpinTask.Task;
    }
    private void slowDownNFS()
    {
        DOTween.To(() => speedNFS, x => speedNFS = x, 0.5f, timeNFS).OnUpdate(() =>
        {
        }).SetEase(Ease.InQuad).OnComplete(() =>
        {

        });
    }
    public void spinSymbol()
    {
        listSymbols.ForEach((sym) =>
        {
            if (slotView.spintype == BaseSlotXisiang.SPIN_TYPE.NORMAL)
            {
                sym.Speed = (float)SPEED_TYPE["NORMAL"];
            }
            else
            {
                sym.Speed = (float)SPEED_TYPE["AUTO"];
            }
            sym.spin();
        });
    }
    public void setRandomView()
    {
        listSymbols.ForEach((sym) =>
        {
            int randomID = UnityEngine.Random.Range(0, 9);
            sym.setSprite(randomID);
        });
    }
    public virtual void stopCollumCompleted()
    {

        spineNFS.gameObject.SetActive(false);
        listSymbols.Sort((a, b) =>
        {
            return a.indexSymbol - b.indexSymbol;
        });

        if (collumIndex == 4)
        {
            slotView.activeAllSymbol();
            slotView.allCollumStopCompleted();
        }
    }
    public void prepareStop()
    {
        collumSpinTask.TrySetResult();
    }
    public void Reset()
    {
        isStop = false;
        //listIDResult.Clear();
        listSymbols.ForEach((symbol) =>
        {
            symbol.Reset();
        });
        isNeerSpin = false;
        speedNFS = 0.05f;
    }
    protected SymbolController getSylbolFromIndex(int index)
    {
        return listSymbols.Find(symbol => symbol.indexSymbol == index);

    }
    public void showScatterSymbol()
    {
        listSymbols.ForEach(symbol =>
        {
            if (symbol.id == 10)
            {
                symbol.showScatterSpine();
            }
        });
    }
    public void hideAllSymbol(int ignoreID = -1)
    {
        listSymbols.ForEach((symbol) =>
        {
            if (symbol.id != ignoreID)
            {
                symbol.sprite.color = Color.gray;
            }
            else
            {
                symbol.sprite.color = Color.white;
            }
        });
    }
    public void activeAllSymbol()
    {
        listSymbols.ForEach((symbol) =>
        {
            symbol.sprite.color = Color.white;
        });
    }
    public async UniTask activeSymbol(int index)
    {
        listSymbols[index].setSpine(listSymbols[index].id);
        listSymbols[index].setBgWin(true);
        DOTween.Kill(listSymbols[index]);
        DOTween.Sequence().AppendInterval(2.0f).AppendCallback(() =>
        {
            listSymbols[index].setBgWin(false);
            listSymbols[index].setSprite(listSymbols[index].id);
        }).SetTarget(listSymbols[index]);
        await UniTask.Delay(TimeSpan.FromSeconds(2.0f), cancellationToken: slotView.cts_ShowEffect.Token);
    }
    public Vector2 getPosSymbol(int index)
    {
        //listSymbols[index].transform.localScale = new Vector2(1.5f, 1.5f);
        return listSymbols[index].transform.position;
    }
    }
}