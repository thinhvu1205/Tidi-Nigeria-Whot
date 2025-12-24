using Globals;
using Proto;
using TMPro;
using UnityEngine;

public class LuckyNumberHistoryItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textId, textDrawTime, textChosenNumbers, textStatus, textReward, textNote;
    [SerializeField] private GameObject imageNote;
    [SerializeField] private Sprite[] listSpriteNote; // 0: free, 1: 3, 2: 4, 3:5, 4:6 matched numbers

    public void Init(LotteryTicket ticket)
    {
        textId.text = ticket.Id.ToString();
        textDrawTime.text = Utility.ConvertUnixTimeToDDMMYYYYHHMM(ticket.DrawTimeUnix);
        textChosenNumbers.text = string.Join(", ", ticket.Numbers);
        textStatus.text = ticket.Status switch
        {
            LotteryTicketStatus.Win => "Win",
            LotteryTicketStatus.Lose => "Lose",
            LotteryTicketStatus.Waiting => "Waiting",
            _ => "Unknown"
        };
        textNote.text = ticket.Status switch
        {
            LotteryTicketStatus.Lose => "Lose",
            LotteryTicketStatus.Waiting => "Waiting",
            _ => "Unknown"
        };
        textReward.text = Utility.FormatNumber(ticket.Reward);
        switch(ticket.MatchedCount)
        {
            case 3:
                imageNote.GetComponent<SpriteRenderer>().sprite = listSpriteNote[0];
                break;
            case 4:
                imageNote.GetComponent<SpriteRenderer>().sprite = listSpriteNote[1];
                break;
            case 5:
                imageNote.GetComponent<SpriteRenderer>().sprite = listSpriteNote[2];
                break;
            case 6:
                imageNote.GetComponent<SpriteRenderer>().sprite = listSpriteNote[3];
                break;
            default:
                imageNote.SetActive(false);
                textNote.gameObject.SetActive(true);
                break;
        }
    }
}
