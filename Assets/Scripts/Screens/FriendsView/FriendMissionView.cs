using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FriendMissionView : BaseView
{
    [SerializeField] private int pageIndex = 0;

    [SerializeField] private ToggleGroup toggleGroup;
    [SerializeField] private List<Toggle> tabs = new();
    [SerializeField] private List<CanvasGroup> pages = new();
    public UnityEvent<int> OnPageIndexChanged;

    private void OnValidate()
    {
        OpenPage(pageIndex);
        tabs[pageIndex].SetIsOnWithoutNotify(true);
    }

    protected override void Awake()
    {
        base.Awake();
        foreach(Toggle toggle in tabs)
        {
            toggle.onValueChanged.AddListener(CheckForTab);
            toggle.group = toggleGroup;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        foreach(Toggle toggle in tabs)
        {
            toggle.onValueChanged.RemoveListener(CheckForTab);
        }
    }

    public override void OnClickCloseButton()
    {
        Hide(false);
    }

    #region Tabs

    private void CheckForTab(bool value)
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            if (!tabs[i].isOn) continue;
            pageIndex = i;
        }
        OpenPage(pageIndex);
    }

    private void OpenPage(int index)
    {
        EnsureIndexIsInRange(index);

        for (int i = 0; i < pages.Count; i++)
        {
            bool isActivePage = i == pageIndex;
            pages[i].alpha = isActivePage ? 1.0f : 0f;
            pages[i].interactable = isActivePage;
            pages[i].blocksRaycasts = isActivePage;
        }

        if (Application.isPlaying)
        {
            OnPageIndexChanged?.Invoke(pageIndex);
        }
    }

    private void EnsureIndexIsInRange(int index)
    {
        if (tabs.Count == 0 || pages.Count == 0)
        {
            return;
        }
        pageIndex = Mathf.Clamp(index, 0, pages.Count - 1);
    }

    public void JumpToPage(int index)
    {
        EnsureIndexIsInRange(index);
        tabs[pageIndex].isOn = true;
    }
    #endregion
}
