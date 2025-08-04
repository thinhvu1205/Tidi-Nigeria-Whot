using Proto;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class SixiangSymbol : SlotSymbol
{
    public async UniTask showEffectSpeadWild(Vector2 initPos)
    {
        // spine.transform.SetParent(collumCtrl.transform);
        // spine.transform.localPosition = initPos;
        // float timeScale = collumCtrl.slotView.spinType == BaseSlotSymbolView.SPIN_TYPE.NORMAL ? 1.0f : 1.5f;
        // setSpine(9, timeScale);
        // Sequence seq = DOTween.Sequence();
        // float timeEff = collumCtrl.slotView.spinType == BaseSlotSymbolView.SPIN_TYPE.NORMAL ? 1.0f : 0.7f;
        // seq.Append(spine.transform.DOLocalMoveY(transform.localPosition.y, timeEff).OnComplete(() =>
        // {
        //     spine.transform.SetParent(transform);
        //     spine.transform.localPosition = Vector2.zero;
        // })).AppendInterval(timeEff).AppendCallback(() =>
        // {
        //     spine.gameObject.SetActive(false);
        //     spine.timeScale = 1.0f;
        //     setSprite(9);
        // }).SetId("seqWildSpead");
        // Tween tw = DOTween.TweensById("seqWildSpead")[0];
        // await tw.AsyncWaitForCompletion();
    }

    public void showWild(float timeScale = 1.0f)
    {
        // SetSpine(9, timeScale);
    }

    public override void SetSprite(int idSprite, bool isBlur = false)
    {
        base.SetSprite(idSprite, isBlur);
        if (idSprite > 4 && idSprite < 9)
        {
            sprite.transform.localScale = new Vector2(0.9f, 0.9f);
        }
        else if (idSprite == 9 || idSprite == 10)
        {
            sprite.transform.localScale = new Vector2(0.9f, 0.9f);
        }
        else
        {
            sprite.transform.localScale = Vector2.one;
        }

        // if (SlotSixiangView.Instance != null &&
        //     SlotSixiangView.Instance.currentGame == (int)SiXiangGame.DragonPearl)
        // {
        //     sprite.transform.localScale = sprite.transform.localScale * new Vector2(0.97f, 0.97f);
        // }
    }
    
}
