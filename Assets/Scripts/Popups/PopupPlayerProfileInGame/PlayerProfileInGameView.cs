using System.Collections.Generic;
using System.Threading.Tasks;
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
    [SerializeField] private Button buttonAddFriend;
    private PlayerProfileInGamePresenter playerProfileInGamePresenter;
    private string playerId;

    protected override void Awake()
    {
        base.Awake();
        playerProfileInGamePresenter = new PlayerProfileInGamePresenter();
        playerProfileInGamePresenter.Init(this);
        buttonAddFriend.onClick.AddListener(async () => await OnClickSendFriendRequest());
    }

    public void SetInfo(string name, string id, string sId, int vipLevel, string avatarId)
    {
        playerId = id;
        nameText.text = name;
        idText.text = "ID: " + sId;
        avatar.LoadAvatar(avatarId, vipLevel);
        SetVipStars(vipLevel);

        bool isMe = playerId == User.Profile.UserId;
        buttonAddFriend.gameObject.SetActive(!isMe && !FriendManager.Instance.IsFriend(playerId));
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

    public async Task OnClickSendFriendRequest()
    {
        List<string> userId = new()
        {
            playerId  
        };
        await playerProfileInGamePresenter.SendFriendRequest(userId);
        buttonAddFriend.gameObject.SetActive(false);
    }

    public void OnClickButtonEmoji(int emojiId)
    {
        _ = playerProfileInGamePresenter.SendEmoji(User.Profile.UserId, playerId, emojiId.ToString());
        Hide();
    }
}
