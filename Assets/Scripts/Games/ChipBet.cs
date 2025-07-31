using System.Collections.Generic;
using Common.Pool;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Games
{
    public class ChipBet : MonoBehaviour, IPoolable
    {
        [SerializeField]
        List<Sprite> sprChips = new List<Sprite>();

        Image imgChip;
    
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    
        public void init(int value = 0, float scale = 1.0f)
        {
            imgChip = GetComponent<Image>();
            if (value == 0)
            {
                imgChip.sprite = getRandomSpr();
            }
            transform.localScale = new Vector2(scale,scale);
        }
    
        private Sprite getRandomSpr()
        {
            int randomIndex = Random.Range(0, sprChips.Count);
            return sprChips[randomIndex];
        }
   
        public void move(Vector2 pos, float time = 0.3f, System.Action cb = null, bool isEffect = true)
        {
            if (isEffect)
            {
                DOTween.Sequence()
                    .Append(transform.DOLocalMove(pos, time).SetEase(Ease.InSine))
                    .AppendCallback(() =>
                    {
                        if (cb != null)
                        {
                            cb.Invoke();
                        }
                    });
            }
            else
            {
                transform.localPosition = pos;
            }
        
        }

        public void OnGetFromPool()
        {
       
        }

        public void OnReturnToPool()
        {
        
        }
    }
}
