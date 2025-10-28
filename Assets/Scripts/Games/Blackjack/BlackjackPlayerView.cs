
using UnityEngine;
using UnityEngine.UI;

public class BlackjackPlayerView : BasePlayerView
{
    [SerializeField] private Image imageLight;

    public override void SetCurrentTurn(bool isTurn, float _timeTurn = 0f, float _totalTimeTurn = 0, bool _isMe = false, float timeVibrate = 5f)
    {
        base.SetCurrentTurn(isTurn, _timeTurn,  _totalTimeTurn , _isMe, timeVibrate);
        imageLight.gameObject.SetActive(isTurn);
    }

    public override void HideCountDown()
    {
        base.HideCountDown();
        imageLight.gameObject.SetActive(false);

    }
}
