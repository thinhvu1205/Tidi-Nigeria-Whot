using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Globals;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Avatar = Common.Objects.Avatar;

public class ProfileView : BaseView
{
    [SerializeField] private TextMeshProUGUI nameText, idText, chipText;
    [SerializeField] private Image avatarImage;
    [SerializeField] private Button changePasswordButton, changeNameButton;
    [SerializeField] private Sprite[] listAvatarSprite;
    [SerializeField] private GameObject avatarPrefab;
    [SerializeField] private Transform avatarContainer;
    [SerializeField] private Avatar avatar;
    [SerializeField] private Image[] listImageStar;
    [SerializeField] private Sprite[] listSpriteStar; // 0: Full, 1: Half, 2: Empty
    private ProfilePresenter profilePresenter;
    

    protected override void Awake()
    {
        base.Awake();
        UpdateProfileVisuals();
        InitListAvatar();
        profilePresenter = new ProfilePresenter();
        profilePresenter.Init(this);
    }

    protected override void Start()
    {
        base.Start();

    }

    protected override void OnEnable()
    {
        base.OnEnable();
        User.OnProfileUpdated += UpdateProfileVisuals;

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        User.OnProfileUpdated -= UpdateProfileVisuals;
    }

    private void UpdateProfileVisuals()
    {
        if (User.UserAccount != null)
        {
            int vipLevel = (int)User.UserAccount.Profile.Vip;
            nameText.text = User.UserAccount.Profile.DisplayName;
            idText.text = "ID: " + User.UserAccount.Profile.UserId;
            chipText.text = Utility.FormatNumber(User.UserAccount.Profile.Balance);
            if (User.UserAccount.IsGuest)
            {
                changeNameButton.gameObject.SetActive(true);
                changePasswordButton.gameObject.SetActive(false);
            }
            else
            {
                changeNameButton.gameObject.SetActive(false);
                changePasswordButton.gameObject.SetActive(true);
            }
            avatar.LoadAvatar(User.UserAccount.Profile.AvatarId.ToString(), User.UserAccount.Profile.Vip);
            SetVipStars(vipLevel);
        }
    }

    private void InitListAvatar()
    {
        SpriteAtlas spriteAtlas = UIManager.Instance.avatarAtlas;
        for (int i = 1; i <= spriteAtlas.spriteCount; i++)
        {
            int index = i;
            Avatar avatar = Instantiate(avatarPrefab, avatarContainer).GetComponent<Avatar>();
            avatar.SetAvatar(spriteAtlas.GetSprite($"avatar_{i}"));
            avatar.AddComponent<Button>();
            avatar.GetComponent<Button>().onClick.AddListener(() =>
            {
                _ = OnUpdateAvatar(index.ToString());
            });

        }
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

    private async UniTask OnUpdateAvatar(string name)
    {
        // await ShowToast(name);
        await profilePresenter.OnUpdateAvatar(name);
    }

    private bool IsDefaultName(string name)
    {
        return name.Contains("CGPD.");
    }

    #region Buttons
    public void OnClickChangePassword()
    {
        Hide();
        UIManager.Instance.OpenChangePassword();
    }

    public void OnClickChangeName()
    {
        Hide();
        UIManager.Instance.OpenChangeName();
    }

    public void OnClickSendGift()
    {
        Hide();
        UIManager.Instance.OpenSendGift();
    }

   

    #endregion

}
