
using UnityEngine;
using UnityEngine.UI;

public class BlackjackPlayerView : BasePlayerView
{
    [SerializeField] private Image imageLight;

    public override void SetCurrentTurn(bool isTurn, float _timeTurn = 0f, bool _isMe = false, float timeVibrate = 5f)
    {
        base.SetCurrentTurn(isTurn, _timeTurn, _isMe, timeVibrate);
        imageLight.gameObject.SetActive(isTurn);
    }

    public override void HideCountDown()
    {
        base.HideCountDown();
        imageLight.gameObject.SetActive(false);

    }
}
