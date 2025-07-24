using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Games.Slot_Games.Slot_Sixiang
{
    public class SiXiangCollumController : CollumController
    {
        protected override void Start()
        {
            base.Start();
        }

        public override void stopCollumCompleted()
        {
            base.stopCollumCompleted();
        }

        public async UniTask checkWildSpread()
        {
            float timeDelay = slotView.spintype == BaseSlotXisiang.SPIN_TYPE.NORMAL ? 2f : 1.33f;
            float timeScale = slotView.spintype == BaseSlotXisiang.SPIN_TYPE.NORMAL ? 1f : 1.5f;
            SiXiangSymbolController symbolWild = (SiXiangSymbolController)listSymbols.Find(symbol =>
            {
                return (symbol.id == 9 && symbol.indexSymbol != 0);
            });
            if (symbolWild != null)
            {
                symbolWild.showWild(timeScale);
                await UniTask.Delay(TimeSpan.FromSeconds(timeDelay));
                if (symbolWild.indexSymbol > 0)
                {
                    await showWildSpread(symbolWild.indexSymbol);
                }
            }
        }

        private async UniTask showWildSpread(int indexWild)
        {
            List<UniTask> tasks = new List<UniTask>();
            float timeDelay = slotView.spintype == BaseSlotXisiang.SPIN_TYPE.NORMAL ? 2f : 1.33f;
            float timeScale = slotView.spintype == BaseSlotXisiang.SPIN_TYPE.NORMAL ? 1f : 1.5f;
            for (int i = 0; i < listSymbols.Count; i++)
            {
                SiXiangSymbolController symbol = (SiXiangSymbolController)listSymbols[i];
                SymbolController symbolWild = getSylbolFromIndex(indexWild);
                if (symbol.indexSymbol != indexWild &&
                    symbol.indexSymbol > 0) //check ne thang wild ra de move spine den vi tri 2 thang nay
                {
                    Vector2 wildItemPos = symbolWild.transform.localPosition;
                    tasks.Add(symbol.showEffectSpeadWild(wildItemPos));
                }

                symbolWild.setSpine(9, timeScale);
                DOTween.Sequence().AppendInterval(timeDelay).AppendCallback(() =>
                {
                    symbolWild.spine.gameObject.SetActive(false);
                });
            }

            await UniTask.WhenAny(tasks.ToArray());
        }

        public bool checkWildSymbol()
        {
            bool isHasWild = false;
            foreach (SymbolController symbol in listSymbols)
            {
                if (symbol.id == 9)
                {
                    isHasWild = true;
                }
            }

            return isHasWild;
        }

        public bool checkScatterSymbol()
        {
            bool isHasScatter = false;
            foreach (SymbolController symbol in listSymbols)
            {
                if (symbol.id == 10)
                {
                    isHasScatter = true;
                }
            }

            return isHasScatter;
        }
       
    }
}