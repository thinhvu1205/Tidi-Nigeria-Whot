using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Common.Objects
{
    public class Avatar : MonoBehaviour
    {
        [SerializeField] public Image imageAvatar, imageBorder;
        [SerializeField] public Sprite spriteAvatarDefault;
        [SerializeField] public List<Sprite> border;
        [SerializeField] public TextMeshProUGUI textName;

        public int avatarIndex = 0;

        //public bool isMe = false;

        public void Awake()
        {
        }

        public Image GetAvatar() => imageAvatar;
        public void SetAvatar(Sprite sprite) => imageAvatar.sprite = sprite;

        public void LoadAvatar(string avatarIndex, string fbName = "", string fbId = "", string name = "")
        {
            if (string.IsNullOrEmpty(avatarIndex))
            {
                SetSpriteFrame(spriteAvatarDefault);
            }
            else
            {
                int avaIndex = int.Parse(avatarIndex);
                if (avaIndex > 0 && avaIndex <= UIManager.Instance.avatarAtlas.spriteCount)
                {
                    SetSpriteWithIndex(avaIndex);
                }

            }
            int vipLevel = (int)User.userProfile.VipLevel;
            if (vipLevel > 0)
            {
                imageBorder.sprite = border[vipLevel - 1];
            }

            // else
            // {
            //     //http://graph.facebook.com/%fbID%/picture?type=square
            //     //var avtLink = "http://graph.facebook.com/%fbID%/picture?type=square";// Globals.Config.avatar_fb == "" ? "http://graph.facebook.com/%fbID%/picture?type=square" : Globals.Config.avatar_fb;
            //     var avtLink = Config.avatar_fb == ""
            //         ? "http://graph.facebook.com/%fbID%/picture?type=square"
            //         : Config.avatar_fb;
            //     //avtLink = avtLink.Replace("%fbID%", fbId);
            //     //avtLink = avtLink.Replace("%token%", Globals.User.userMain.AccessToken);
            //     Debug.Log("loadAvatar fbId:" + fbId);
            //     if (User.AccessToken == "" || (fbId != User.FacebookID))
            //     {
            //         avtLink = avtLink.Replace("&access_token=%token%", "");
            //     }

            //     if (fbName.Contains("fb."))
            //     {
            //         var idFb = fbName.Substring(3);

            //         avtLink = avtLink.Replace("%fbID%", idFb);
            //     }
            //     else if (fbId != "")
            //     {
            //         avtLink = avtLink.Replace("%fbID%", fbId);
            //     }

            //     avtLink = avtLink.Replace("%token%", Globals.User.AccessToken);
            //     Debug.Log("loadAvatar:" + avtLink);
            //     if (imageAvatar != null)
            //     {
            //         imageAvatar.sprite = await Config.GetRemoteSprite(avtLink);
            //         if (imageAvatar.sprite == null) SetSpriteWithIndex(1); //default
            //     }
            // }

            // if (textName == null) return;
            // if (name.Equals(""))
            // {
            //     textName.gameObject.SetActive(false);
            // }
            // else
            // {
            //     textName.gameObject.SetActive(true);
            //     textName.text = name;
            // }
        }

        public async void loadAvatarAsync(int idAva, string fbName, string fbId = "")
        {
            if (idAva > 0 && idAva <= UIManager.Instance.avatarAtlas.spriteCount)
            {
                SetSpriteWithIndex(idAva);
            }
            else
            {
                //http://graph.facebook.com/%fbID%/picture?type=square
                //Globals.Logging.Log("Globals.Config.avatar_fb    " + Globals.Config.avatar_fb);
                //var avtLink = "http://graph.facebook.com/%fbID%/picture?type=square";// Globals.Config.avatar_fb == "" ? "http://graph.facebook.com/%fbID%/picture?type=square" : Globals.Config.avatar_fb;
                var avtLink = Globals.Config.avatar_fb == ""
                    ? "http://graph.facebook.com/%fbID%/picture?type=square"
                    : Globals.Config.avatar_fb;
                //avtLink = avtLink.Replace("%fbID%", fbId);
                //avtLink =avtLink avtLink.Replace("%token%", Globals.User.userMain.AccessToken);

                if (Globals.User.AccessToken.Equals("") || (!fbId.Equals(Globals.User.FacebookID)))
                {
                    avtLink = avtLink.Replace("&access_token=%token%", "");
                }

                if (fbName.Contains("fb."))
                {
                    var idFb = fbName.Substring(3);
                    //idFb = "42625900944101";
                    avtLink = avtLink.Replace("%fbID%", idFb);
                }
                else if (!fbId.Equals(""))
                {
                    avtLink = avtLink.Replace("%fbID%", fbId);
                }

                if (fbId == Globals.User.FacebookID)
                {
                    avtLink = avtLink.Replace("%token%", Globals.User.AccessToken);
                }

                Debug.Log("loadAvatarAsync avtLink  " + avtLink);
                if (this != null && imageAvatar != null)
                {
                    imageAvatar.sprite = await Globals.Config.GetRemoteSprite(avtLink);
                    if (imageAvatar.sprite == null) SetSpriteWithIndex(1); //default
                }
            }
        }

        public void setVip(int vip)
        {
            //        v1 = VIP 0 - 1 - 2
            //v2 = VIP 3 - 4
            //v3 = VIP 5 - 6
            //v4 = VIP 7 - 8
            //v5 = VIP 9 - 10
            if (imageBorder == null) return;
            var index = 0;
            if (vip <= 2)
            {
                index = 0;
            }
            else if (vip <= 4)
            {
                index = 1;
            }
            else if (vip <= 6)
            {
                index = 2;
            }
            else if (vip <= 8)
            {
                index = 3;
            }
            else
            {
                index = 4;
            }

            imageBorder.sprite = border[index];
        }

        public void setDefault()
        {
            imageAvatar.sprite = spriteAvatarDefault;
        }

        public void SetSpriteFrame(Sprite sprite)
        {
            if (imageAvatar == null) return;
            imageAvatar.sprite = sprite;
        }

        public void effectShowAvatar(Sprite sprite) //Keang
        {
            imageAvatar.sprite = sprite;
            CanvasGroup cvGroupAvt = imageAvatar.GetComponent<CanvasGroup>();
            cvGroupAvt.alpha = 0;
            cvGroupAvt.DOFade(1, 1.0f);
        }

        public void SetSpriteWithIndex(int avatarIndex)
        {
            Sprite avatarSprite = UIManager.Instance.avatarAtlas.GetSprite("avatar_" + avatarIndex);
            if (avatarSprite == null)
                avatarSprite = UIManager.Instance.GetAvatarDefault();
            SetSpriteFrame(avatarSprite);
            this.avatarIndex = avatarIndex;
        }

        public void setDark(bool isDark)
        {
            imageAvatar.color = isDark ? Color.gray : Color.white;
        }

        public async UniTask setSpriteWithID2(int idAva)
        {
            Debug.Log("-=-= before yield");
            await UniTask.Yield(); // Chờ 1 frame hoặc move sang async context
            SetSpriteFrame(UIManager.Instance.avatarAtlas.GetSprite("avatar_" + idAva));
            avatarIndex = idAva;
        }

        public async UniTask loadAvatarAsync2(int idAva, string fbName, string fbId = "")
        {
            if (idAva > 0 && idAva <= UIManager.Instance.avatarAtlas.spriteCount)
            {
                await setSpriteWithID2(idAva);
            }
            else
            {
                //http://graph.facebook.com/%fbID%/picture?type=square
                //Globals.Logging.Log("Globals.Config.avatar_fb    " + Globals.Config.avatar_fb);
                //var avtLink = "http://graph.facebook.com/%fbID%/picture?type=square";// Globals.Config.avatar_fb == "" ? "http://graph.facebook.com/%fbID%/picture?type=square" : Globals.Config.avatar_fb;
                var avtLink = Globals.Config.avatar_fb == ""
                    ? "http://graph.facebook.com/%fbID%/picture?type=square"
                    : Globals.Config.avatar_fb;
                //avtLink = avtLink.Replace("%fbID%", fbId);
                //avtLink =avtLink avtLink.Replace("%token%", Globals.User.userMain.AccessToken);

                if (Globals.User.AccessToken == "" || (fbId != Globals.User.FacebookID))
                {
                    avtLink = avtLink.Replace("&access_token=%token%", "");
                }

                if (fbName.Contains("fb."))
                {
                    var idFb = fbName.Substring(3);
                    //idFb = "42625900944101";
                    avtLink = avtLink.Replace("%fbID%", idFb);
                }
                else if (fbId != "")
                {
                    avtLink = avtLink.Replace("%fbID%", fbId);
                }

                if (fbId == Globals.User.FacebookID)
                {
                    avtLink = avtLink.Replace("%token%", Globals.User.AccessToken);
                }

                if (imageAvatar != null)
                {
                    imageAvatar.sprite = await Globals.Config.GetRemoteSprite(avtLink);
                    if (imageAvatar.sprite == null) await setSpriteWithID2(1); //default
                }
            }
        }
    }
}