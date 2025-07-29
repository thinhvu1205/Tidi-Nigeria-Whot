using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Popups
{
    public class Progressing : Singleton<Progressing>
    {
        [SerializeField] private Image iconBorder;
        [SerializeField] private Image iconHeart;
        private Tween heartTween;
        private bool isAnimating;

        protected override void Awake()
        {
            base.Awake();
            isAnimating = false;
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            isAnimating = true;
            StartHeartAnimation();
        }

        private void OnDisable()
        {
            isAnimating = false;
            StopHeartAnimation();
        }

        private void Update()
        {
            if (!isAnimating) return;
            iconBorder.transform.Rotate(Vector3.forward, 360 * Time.deltaTime);
        }
        
        private void StartHeartAnimation()
        {
            // Scale co dãn qua lại 0 -> 1 liên tục
            heartTween = iconHeart.transform.DOScaleX(0, 0.35f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        private void StopHeartAnimation()
        {
            if (heartTween != null && heartTween.IsActive())
            {
                heartTween.Kill();
                iconHeart.transform.localScale = Vector3.one; // Reset scale về mặc định
            }
        }
    }
}