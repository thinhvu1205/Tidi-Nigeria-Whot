using Common.Pool;
using DG.Tweening;
using Globals;
using Spine.Unity;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Games.Card
{
    /// <summary>
    /// Card - Manages individual card display and animations
    /// Handles card data, visual effects, and game-specific features
    /// </summary>
    public class CardModel : MonoBehaviour, IPoolable
    {
        [Header("=== CARD TYPE CONSTANTS ===")]
        private static readonly System.Collections.Generic.Dictionary<int, string> CARD_TYPE = new System.Collections.Generic.Dictionary<int, string>
        {
            { 1, "co" },
            { 2, "ro" },
            { 3, "tep" },
            { 4, "bich" }
        };

        [Header("=== UI COMPONENTS ===")]
        [Tooltip("Card background image")]
        [SerializeField] public Image imgBackground;
    
        [Tooltip("Star effect for Pusoy game")]
        [SerializeField] public Image starPusoy;
    
        [Tooltip("Blue background for hidden cards")]
        [SerializeField] public Image blueBg;
    
        [Tooltip("Dark background for shadow effect")]
        [SerializeField] public Image darkBg;
    
        [Tooltip("Card value display")]
        [SerializeField] public Image value;
    
        [Tooltip("Small icon display")]
        [SerializeField] public Image icon_small;
    
        [Tooltip("Large icon display")]
        [SerializeField] public Image icon_large;
    
        [Tooltip("Card border image")]
        [SerializeField] public Image cardBorder;

        [Header("=== SPRITE ASSETS ===")]
        [Tooltip("Sprite atlas for Pusoy card images")]
        [SerializeField] public SpriteAtlas imageSpritesPusoy;

        [Header("=== ANIMATIONS ===")]
        [Tooltip("Spine animation for sparkle effect")]
        [SerializeField] public SkeletonGraphic animationSparkle;

        [Header("=== CARD DATA ===")]
        [Tooltip("Card data containing rank and suit")]
        public CardData data;

        [Header("=== ANIMATION SEQUENCES ===")]
        [Tooltip("Sparkle animation sequence")]
        private Sequence tweenLapLanh;

        /// <summary>
        /// Card data structure
        /// </summary>
        [System.Serializable]
        public class CardData
        {
            public int rank;
            public int suit;
        
            public CardData(int rank, int suit)
            {
                this.rank = rank;
                this.suit = suit;
            }
        }

        private void Start()
        {
            Initialize();
        }

        /// <summary>
        /// Initialize card component
        /// </summary>
        private void Initialize()
        {
            tweenLapLanh = null;
        }

        /// <summary>
        /// Sets card data and updates visual display
        /// </summary>
        /// <param name="value">Card rank (1-14)</param>
        /// <param name="type">Card suit (1-4)</param>
        public void SetData(int value, int type)
        {
            data = new CardData(value, type);
            icon_large.gameObject.SetActive(true);
            icon_small.gameObject.SetActive(true);
            this.value.gameObject.SetActive(true);
            // Set color based on suit (red for hearts/diamonds, black for clubs/spades)
            Color textColor = (type == 1 || type == 2) ? new Color(211f/255f, 6f/255f, 25f/255f) : Color.black;
            this.value.color = textColor;
        
            // Handle Ace (rank 14 becomes 1)
            int rank = value;
            if (rank == 14)
            {
                rank = 1;
            }
        
            // Set card sprites
            this.value.sprite = imageSpritesPusoy.GetSprite($"card_{rank}");
            this.icon_small.sprite = imageSpritesPusoy.GetSprite($"card_{CARD_TYPE[type]}_small");
            this.icon_large.sprite = imageSpritesPusoy.GetSprite($"card{(rank > 10 ? "_" + rank : "")}_{CARD_TYPE[type]}");
        }

        /// <summary>
        /// Hides card (shows red background)
        /// </summary>
        public void HideCard()
        {
            blueBg.gameObject.SetActive(true);
            icon_large.gameObject.SetActive(false);
            icon_small.gameObject.SetActive(false);
            value.gameObject.SetActive(false);
        }

        /// <summary>
        /// Shows card (hides red background)
        /// </summary>
        public void ShowCard()
        {
            blueBg.gameObject.SetActive(false);
            icon_large.gameObject.SetActive(true);
            icon_small.gameObject.SetActive(true);
            value.gameObject.SetActive(true);
        }

        /// <summary>
        /// Checks if card is visible
        /// </summary>
        /// <returns>True if card is shown</returns>
        public bool IsShow()
        {
            return !blueBg.gameObject.activeInHierarchy;
        }

        /// <summary>
        /// Animates hiding card with flip effect
        /// </summary>
        public void HideCardAnimation()
        {
            float cardScaleX = transform.localScale.x;
        
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOScaleX(0, 0.1f))
                .AppendCallback(() => {
                    blueBg.gameObject.SetActive(true);
                })
                .Append(transform.DOScaleX(cardScaleX, 0.3f));
            sequence.Play();
        }

        /// <summary>
        /// Animates showing card with flip effect
        /// </summary>
        public void ShowCardAnimation()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOScaleX(0, 0.1f))
                .AppendCallback(() => {
                    blueBg.gameObject.SetActive(false);
                })
                .Append(transform.DOScaleX(transform.localScale.x, 0.3f));
            sequence.Play();
        }

        /// <summary>
        /// Shows shadow effect on card
        /// </summary>
        public void ShowShadowCard()
        {
            darkBg.gameObject.SetActive(true);
        }

        /// <summary>
        /// Hides shadow effect on card
        /// </summary>
        public void HideShadowCard()
        {
            darkBg?.gameObject.SetActive(false);
        }

        /// <summary>
        /// Sets dark mode for card
        /// </summary>
        /// <param name="isDark">Dark mode enabled</param>
        public void SetDark(bool isDark)
        {
            darkBg.gameObject.SetActive(isDark);
        }

        /// <summary>
        /// Shows/hides card border
        /// </summary>
        /// <param name="isBorder">Show border</param>
        public void SetBorder(bool isBorder)
        {
            cardBorder.gameObject.SetActive(isBorder);
        }

        /// <summary>
        /// Sets card background color
        /// </summary>
        /// <param name="color">Color to set</param>
        public void SetColorCard(Color color)
        {
            imgBackground.color = color;
        }

        /// <summary>
        /// Sets card opacity
        /// </summary>
        /// <param name="opacity">Opacity value (0-1)</param>
        public void SetOpacity(float opacity)
        {
            Color color = imgBackground.color;
            color.a = opacity;
            imgBackground.color = color;
        }

        /// <summary>
        /// Checks if card is in dark mode
        /// </summary>
        /// <returns>True if dark mode</returns>
        public bool IsDark()
        {
            return darkBg.gameObject.activeInHierarchy;
        }

        /// <summary>
        /// Stops sparkle animation
        /// </summary>
        public void HideSparkleAnimation()
        {
            animationSparkle.gameObject.SetActive(false);
        }

        /// <summary>
        /// Shows sparkle animation effect
        /// </summary>
        public void ShowSparkleAnimation()
        {
            if (darkBg.gameObject.activeInHierarchy)
            {
                return;
            }
        
            // Kill existing animation
            if (tweenLapLanh != null)
            {
                tweenLapLanh.Kill();
            }
            tweenLapLanh = DOTween.Sequence();
            tweenLapLanh.AppendCallback(() =>
            {
                Utility.PlayAnimation(animationSparkle, "animation", true);
            });
            // .AppendInterval(1f)
            // .AppendCallback(() => {
            //     HideSparkleAnimation();
            //     SetBorder(true);
            // });
            tweenLapLanh.Play();
        }

        /// <summary>
        /// Shows star effect for Pusoy game
        /// </summary>
        public void ShowStarPusoy()
        {
            starPusoy.gameObject.SetActive(true);
            starPusoy.transform.DOKill();
        
            // Set initial state
            starPusoy.transform.localScale = Vector3.one * 10f;
            starPusoy.color = new Color(starPusoy.color.r, starPusoy.color.g, starPusoy.color.b, 0f);
            starPusoy.transform.rotation = Quaternion.Euler(0, 0, 180f);
        
            // Animate star
            Sequence starSequence = DOTween.Sequence();
            starSequence.Join(starPusoy.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic))
                .Join(starPusoy.transform.DORotate(Vector3.zero, 0.5f).SetEase(Ease.OutCubic))
                .Join(starPusoy.DOFade(1f, 0.3f));
            starSequence.Play();
        }

        /// <summary>
        /// Hides star effect
        /// </summary>
        public void HideStarPusoy()
        {
            starPusoy.gameObject.SetActive(false);
        }

        /// <summary>
        /// Sets star color
        /// </summary>
        /// <param name="color">Color to set</param>
        public void SetStarPusoyColor(Color color)
        {
            starPusoy.color = color;
        }

        /// <summary>
        /// Gets card rank
        /// </summary>
        /// <returns>Card rank</returns>
        public int GetRank()
        {
            return data?.rank ?? 0;
        }

        /// <summary>
        /// Gets card suit
        /// </summary>
        /// <returns>Card suit</returns>
        public int GetSuit()
        {
            return data?.suit ?? 0;
        }

        /// <summary>
        /// Gets card data
        /// </summary>
        /// <returns>Card data</returns>
        public CardData GetData()
        {
            return data;
        }

        /// <summary>
        /// Checks if card is an Ace
        /// </summary>
        /// <returns>True if Ace</returns>
        public bool IsAce()
        {
            return data?.rank == 1 || data?.rank == 14;
        }

        /// <summary>
        /// Checks if card is a face card (J, Q, K)
        /// </summary>
        /// <returns>True if face card</returns>
        public bool IsFaceCard()
        {
            int rank = data?.rank ?? 0;
            return rank >= 11 && rank <= 13;
        }

        /// <summary>
        /// Gets card display name
        /// </summary>
        /// <returns>Card name string</returns>
        public string GetCardName()
        {
            if (data == null) return "";
        
            string rankName = "";
            switch (data.rank)
            {
                case 1: rankName = "A"; break;
                case 11: rankName = "J"; break;
                case 12: rankName = "Q"; break;
                case 13: rankName = "K"; break;
                default: rankName = data.rank.ToString(); break;
            }
        
            string suitName = CARD_TYPE.ContainsKey(data.suit) ? CARD_TYPE[data.suit] : "";
            return $"{rankName} {suitName}";
        }

        /// <summary>
        /// Cleanup when destroyed
        /// </summary>
        private void OnDestroy()
        {
            if (tweenLapLanh != null)
            {
                tweenLapLanh.Kill();
                tweenLapLanh = null;
            }
        }

        public void OnGetFromPool()
        {
            HideShadowCard();
        }

        public void OnReturnToPool()
        {
            
        }
    }
} 