using Cysharp.Threading.Tasks;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Avatar = Common.Objects.Avatar;
public class FriendInviteItem : MonoBehaviour
{
    [SerializeField] private Avatar avatar;
    [SerializeField] private TextMeshProUGUI textInfo, textIntimacy;
    [SerializeField] private Image imageIntimacy;
    [SerializeField] private Button btnInvite;
    [SerializeField] private Sprite[] listIntimacyImage;
    
    public void Setup(FriendItem friendItem, int targetTier = 1)
    {
        avatar.LoadAvatar(friendItem.AvatarId, friendItem.VipLevel);
        textInfo.text = $"{friendItem.Username} \nID: {friendItem.Sid} \nVip Level: {friendItem.VipLevel}";
        textIntimacy.text = $"{friendItem.IntimacyPoint}";

        switch (friendItem.IntimacyStatus)
        {
            case IntimacyStatus.Normal:
                imageIntimacy.sprite = listIntimacyImage[0];
                break;
            case IntimacyStatus.Frozen:
                imageIntimacy.sprite = listIntimacyImage[1];
                break;
            case IntimacyStatus.Reduced:
                imageIntimacy.sprite = listIntimacyImage[2];
                break;
            default:
                imageIntimacy.sprite = listIntimacyImage[0];
                break;
        }
        btnInvite.onClick.AddListener(() =>
        {
            _ = OnClickInvite(targetTier, friendItem.UserId);
        });
    }

    private async UniTask OnClickInvite(int targetTier = 1 , string friendID = "")
    {
        var response = await DataSender.SendRequestTierUpgrade(targetTier, friendID);
        if (response is { Ok: true })
        {
            UIManager.Instance.ShowAlertDialog("You have successfully submitted your friend level upgrade request.");
            Destroy(gameObject);
        }
        else
        {
            UIManager.Instance.ShowAlertDialog("The upgrade failed. Try again later.");
        }
    }
    
}