using Globals;
using TMPro;
using UnityEngine;

namespace Games.Baccarat
{
    public class BaccaratChip : ChipBet
    {
        public string playerName = "", idPl;
        public int gateId = 0, chipSprite = 0 ;
        public long chipValue = 0;

        public void SetInfo(string id, int gateID, Vector2 posSta, long chipValueBet )
        {
            idPl = id;
            gateId = gateID;
            chipValue = chipValueBet;
            transform.localPosition = posSta;
            transform.GetComponentInChildren<TextMeshProUGUI>().text = Utility.FormatMoney(chipValueBet, true);
        }
 
    }
}


