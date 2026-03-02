using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent, ExecuteInEditMode, RequireComponent(typeof(RectTransform))]
public abstract class BasePool : MonoBehaviour
{
    [SerializeField] private GameObject m_PfCell;
    [SerializeField] public Vector2 m_SpacingV2;
    [SerializeField] public float m_PaddingTop, m_PaddingBot, m_PaddingLeft, m_PaddingRight;
    protected List<PoolInfo> _ControlPIs = new();
    protected List<PoolObj> _DataPOs = new();
    public ScrollRect _DataSR;
    protected ICallFunc.Func4<RectTransform, PoolInfo, int> _OnApplyDataCb = (aRT, dataPI, index) => { };
    protected bool _IsCompleteCalculate, _IsChangeCellDimensionsInApplyDataCb;
    private float _BaseCellWidth, _BaseCellHeight;

    public virtual void SetControlInfo(List<PoolInfo> _infoPIs, int _startFocusIndex = 0, float _startFocusDuration = 0)
    {   // _startFocusIndex<0: content remains position
        if (_infoPIs == null || _infoPIs.Count <= 0) return;
        _CheckInitialize();
        _DataSR.StopMovement();
        foreach (PoolObj aPO in _DataPOs) aPO.PutBackToPool();
        for (int i = 0; i < _infoPIs.Count; i++)
        {
            PoolInfo aPI = _infoPIs[i];
            aPI.Id = i;
            if (aPI.CellWidth <= 0) aPI.SetCellWidth(_BaseCellWidth).UpdateOldWidth();
            if (aPI.CellHeight <= 0) aPI.SetCellHeight(_BaseCellHeight).UpdateOldHeight();
        }
        _ControlPIs = _infoPIs;
        if (_IsChangeCellDimensionsInApplyDataCb) _RunAllApplyDataCbs();
        _CalculateCellLocalV2sAndContentSizeDelta();
        if (_startFocusIndex >= 0 && _startFocusIndex < _ControlPIs.Count) ScrollToItem(_startFocusIndex, _startFocusDuration);
    }
    public PoolInfo GetInfo(int _id) { return _id >= 0 && _id < _ControlPIs.Count ? _ControlPIs[_id] : null; }
    public virtual void RefreshUI(int _focusIndex = -1) { }
    public BasePool SetApplyDataCb(ICallFunc.Func4<RectTransform, PoolInfo, int> _func, bool _isChangeCellDimensions = false)
    {
        if (_func == null || m_PfCell == null)
        {
            _OnApplyDataCb = (aRT, dataPI, index) => { };
            _IsChangeCellDimensionsInApplyDataCb = false;
            return null;
        }
        _CheckInitialize();
        _IsChangeCellDimensionsInApplyDataCb = _isChangeCellDimensions;
        _OnApplyDataCb = (aRT, dataPI, index) => { _func(aRT, dataPI, index); };
        return this;
    }
    public virtual void ScrollToItem(int _index, float _duration = 0, Ease _easeE = Ease.Linear, Action _OnEndCb = null) { }
    protected virtual void _CalculateCellLocalV2sAndContentSizeDelta()
    { if (_DataSR.viewport.rect.width <= 0 || _DataSR.viewport.rect.height <= 0) Canvas.ForceUpdateCanvases(); }
    protected void _CheckAndSetUIPoolObject(PoolInfo _aPI, Vector2 _localV2)
    {
        foreach (PoolObj aPO in _DataPOs) if (!aPO.IsUnused && aPO.Id == _aPI.Id) return;
        PoolObj foundPO = null;
        foreach (PoolObj aPO in _DataPOs) if (aPO.IsUnused) { foundPO = aPO; break; }
        if (foundPO == null)
        {
            foundPO = new() { DataRT = Instantiate(m_PfCell, _DataSR.content).GetComponent<RectTransform>() };
            _Normalize(foundPO.DataRT);
            _DataPOs.Add(foundPO);
        }
        foundPO.Id = _aPI.Id;
        foundPO.IsUnused = false;
        foundPO.DataRT.name = "item " + _aPI.Id;
        foundPO.DataRT.gameObject.SetActive(true);
        _OnApplyDataCb(foundPO.DataRT, _aPI, _aPI.Id);
        foundPO.DataRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _aPI.CellWidth);
        foundPO.DataRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _aPI.CellHeight);
        foundPO.DataRT.localPosition = _localV2;
        foundPO.DataRT.SetAsLastSibling();
    }
    protected virtual void _HandleOnScroll() { }
    protected void _RunAllApplyDataCbs()
    {
        RectTransform aRT = Instantiate(m_PfCell, _DataSR.viewport).GetComponent<RectTransform>();
        foreach (PoolInfo aPI in _ControlPIs) _OnApplyDataCb(aRT, aPI, aPI.Id);
        Destroy(aRT.gameObject);
    }
    private void _Normalize(RectTransform _aRT)
    {
        _aRT.anchorMin = Vector2.up;
        _aRT.anchorMax = Vector2.up;
        _aRT.pivot = Vector2.up;
    }
    private void _CheckInitialize()
    {
        if (m_PfCell != null)
        {
            RectTransform cellRT = m_PfCell.GetComponent<RectTransform>();
            _BaseCellWidth = cellRT.rect.width;
            _BaseCellHeight = cellRT.rect.height;
        }
        _Normalize(GetComponent<RectTransform>());
        if (_DataSR != null) return;
        _DataSR = GetComponentInParent<ScrollRect>();
        _DataSR.onValueChanged.AddListener(aV2 => _HandleOnScroll());
        _DataSR.viewport.pivot = Vector2.up;
        if (_DataSR.horizontalScrollbar != null && _DataSR.horizontalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport)
            _DataSR.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        if (_DataSR.verticalScrollbar != null && _DataSR.verticalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport)
            _DataSR.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
    }

    private void Awake()
    {
        _CheckInitialize();
    }
}
public class PoolObj
{
    public RectTransform DataRT;
    public int Id;
    public bool IsUnused;

    public void PutBackToPool()
    {
        Id = -1;
        IsUnused = true;
        DataRT.gameObject.SetActive(false);
    }
}
public class PoolInfo
{
    public object Data;
    public int Id;
    public float LocalXLeft, LocalXRight, LocalYTop, LocalYBot;
    public float OldCellWidth { get; private set; }
    public float OldCellHeight { get; private set; }
    public float CellWidth { get; private set; }
    public float CellHeight { get; private set; }

    public PoolInfo UpdateOldWidth() { OldCellWidth = CellWidth; return this; }
    public PoolInfo UpdateOldHeight() { OldCellHeight = CellHeight; return this; }
    public PoolInfo SetCellWidth(float _width)
    {
        if (CellWidth != _width)
        {
            OldCellWidth = CellWidth;
            CellWidth = _width;
        }
        return this;
    }
    public PoolInfo SetCellHeight(float _height)
    {
        if (CellHeight != _height)
        {
            OldCellHeight = CellHeight;
            CellHeight = _height;
        }
        return this;
    }
}