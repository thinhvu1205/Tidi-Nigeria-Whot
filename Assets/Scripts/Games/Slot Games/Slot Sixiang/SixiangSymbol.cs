using Proto;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class SixiangSymbol : SlotSymbol
{
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
