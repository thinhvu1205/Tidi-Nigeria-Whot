using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Avatar = Common.Objects.Avatar;

public class PlayerProfileInGameView : BaseView
{
    [SerializeField] private TextMeshProUGUI nameText, idText;
    [SerializeField] private Image[] listImageStar;
    [SerializeField] private Sprite[] listSpriteStar; // 0: Full, 1: Half, 2: Empty
    [SerializeField] private Avatar avatar;
    private PlayerProfileInGamePresenter playerProfileInGamePresenter;
    private string playerId;

    protected override void Awake()
    {
        base.Awake();
        playerProfileInGamePresenter = new PlayerProfileInGamePresenter();
        playerProfileInGamePresenter.Init(this);
    }

    public void SetInfo(string name, string id, string sId, int vipLevel, string avatarId)
    {
        playerId = id;
        nameText.text = name;
        idText.text = "ID: " + sId;
        avatar.LoadAvatar(avatarId, vipLevel);
        SetVipStars(vipLevel);
    }

    private void SetVipStars(int vip)
    {
        // Mỗi cấp VIP = 0.5 sao
        float starValue = vip * 0.5f;

        for (int i = 0; i < listImageStar.Length; i++)
        {
            if (starValue >= i + 1)
            {
                // Sao đầy
                listImageStar[i].sprite = listSpriteStar[0];
            }
            else if (starValue > i)
            {
                // Sao nửa
                listImageStar[i].sprite = listSpriteStar[1];
            }
            else
            {
                // Sao trống
                listImageStar[i].sprite = listSpriteStar[2];
            }
        }
    }

    public void OnClickButtonEmoji(int emojiId)
    {
        _ = playerProfileInGamePresenter.SendEmoji(User.userProfile.UserId, playerId, emojiId.ToString());
        Hide();
    }
}
