
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HongKongPokerBoxBet : MonoBehaviour
{
    [SerializeField] Image icon;

    [SerializeField] Sprite[] listSprite;

    [SerializeField] TextMeshProUGUI textChip;
    public int Chip { get; private set; }

    public void SetInfo(HongKongPokerView.BetStatus status, int index, int chipBet = 0)
    {
        Chip = chipBet;
        transform.localScale = index <= 4 ? Vector2.one : Vector2.one * -1;
        textChip.transform.localScale = index <= 4 ? Vector2.one : Vector2.one * -1;
        icon.transform.localScale = index <= 4 ? Vector2.one : Vector2.one * -1;
        if (chipBet == 0)
        {
            GetComponent<Image>().enabled = false;
            textChip.text = "";
        }
        else
        {
            if (!GetComponent<Image>().enabled)
            {
                GetComponent<Image>().enabled = true;
            }
            textChip.text = Utility.FormatMoney2(chipBet, true);
        }
        switch (status)
        {
            case HongKongPokerView.BetStatus.ALL_IN:
                icon.sprite = listSprite[0];
                // SoundManager.instance.playEffectFromPath(SOUND_GAME.ALL_IN);
                break;
            case HongKongPokerView.BetStatus.RAISE:
                icon.sprite = listSprite[1];
                // SoundManager.instance.playEffectFromPath(SOUND_GAME.BET);
                break;
            case HongKongPokerView.BetStatus.CALL:
                icon.sprite = chipBet == 0 ? listSprite[3] : listSprite[2];
                // SoundManager.instance.playEffectFromPath(SOUND_GAME.BET);
                break;
            case HongKongPokerView.BetStatus.CHECK:
                GetComponent<Image>().enabled = false;
                icon.sprite = listSprite[3];
                break;
            case HongKongPokerView.BetStatus.FOLD:
                GetComponent<Image>().enabled = false;
                textChip.text = "";
                icon.sprite = listSprite[4];
                break;
            default:
                icon.enabled = false;
                icon.sprite = null;
                break;
        }
    }

    public void HideAll()
    {
        GetComponent<Image>().enabled = false;
        textChip.text = "";
    }
}
