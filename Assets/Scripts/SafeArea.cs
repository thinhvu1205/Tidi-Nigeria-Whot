using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeArea : MonoBehaviour
{
    public static SafeArea Instance { get; private set; }
    private RectTransform rectTransform;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        rectTransform = GetComponent<RectTransform>();
        ChangeOrient();
    }

    private void Update()
    {
        ChangeOrient();
        enabled = false;
    }
    public void ChangeOrient()
    {
        if (Screen.orientation == ScreenOrientation.Portrait)
        {
            Rect safeAreaRect = Screen.safeArea;

            var minAnchor = safeAreaRect.position;
            var maxAnchor = minAnchor + safeAreaRect.size;
            minAnchor.x /= Screen.width;
            minAnchor.y /= Screen.height;
            maxAnchor.x /= Screen.width;
            maxAnchor.y /= Screen.height;

            rectTransform.anchorMin = minAnchor;
            rectTransform.anchorMax = maxAnchor;
        }
        else if (Screen.orientation == ScreenOrientation.LandscapeLeft)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
        }
    }
}
