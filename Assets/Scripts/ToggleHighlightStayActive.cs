using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleHighlightStayActive : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private Image image;
    [SerializeField] private List<Sprite> listToggleSprite;
    [SerializeField] private TextMeshProUGUI textHeader;
    [SerializeField] private TextMeshProUGUI textCount;
    public Color activeColor;
    public Color inactiveColor;
    private void Reset()
    {
        toggle = GetComponent<Toggle>();
    }

    private void Awake()
    {
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
        OnToggleValueChanged(toggle.isOn);
    }

    private void OnDestroy() {
        toggle.onValueChanged.RemoveListener(OnToggleValueChanged); 
    }

    private void OnToggleValueChanged(bool isOn)
    {
        if (image == null) return;
        image.sprite = toggle.isOn ? listToggleSprite[0] : listToggleSprite[1];
        if (textHeader != null)
        {
            textHeader.color = isOn ? activeColor : inactiveColor;
        }
        if (textCount != null)
        {
            textCount.color = isOn ? activeColor : inactiveColor;
        }
    }
}