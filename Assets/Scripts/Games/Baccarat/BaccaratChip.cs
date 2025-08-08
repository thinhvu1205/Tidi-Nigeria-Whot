using System;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Games.Baccarat
{
    public class BaccaratChip : ChipBet
    {
        public string playerName = "", idPl;
        public int gateId = 0, chipSprite = 0 ;
        public long chipValue = 0;

        public void SetInfo(string id, int gateID, Vector2 posSta, long chipValueBet, int colorIndex = 0 )
        {
            if (colorIndex == -1)
            {
                colorIndex = 5;
            }
            idPl = id;
            gateId = gateID;
            chipValue = chipValueBet;
            transform.localPosition = posSta;
            chipSprite = colorIndex;
            transform.GetComponentInChildren<TextMeshProUGUI>().text = Utility.FormatMoney(chipValueBet, true);
            
            imgChip.sprite = sprChips[colorIndex];
        }
        
    }
    
}


