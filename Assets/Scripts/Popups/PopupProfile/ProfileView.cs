using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        if (User.userProfile != null)
        {
            nameText.text = User.userProfile.DisplayName;
            idText.text = "ID: " + User.userProfile.UserSid;
            chipText.text = User.userProfile.AccountChip.ToString();
            avatar.LoadAvatar(User.userProfile.AvatarId);
            if (IsDefaultName(User.userProfile.DisplayName))
            {
                changeNameButton.gameObject.SetActive(true);
                changePasswordButton.gameObject.SetActive(false);
            }
            else
            {
                changeNameButton.gameObject.SetActive(false);
                changePasswordButton.gameObject.SetActive(true);
            }
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

    private async Task OnUpdateAvatar(string name)
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

   

    #endregion

}
