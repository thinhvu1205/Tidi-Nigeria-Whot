using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class VerticalPool : BasePool
{
    public override void ScrollToItem(int _index, float _duration = 0, Ease _easeE = Ease.Linear, Action _OnEndCb = null)
    {
        base.ScrollToItem(_index, _duration, _easeE, _OnEndCb);
        _DataSR.StopMovement();
        float targetY = Mathf.Min(-GetInfo(_index).LocalYTop, _DataSR.content.sizeDelta.y - _DataSR.viewport.rect.height);
        _DataSR.content.DOLocalMoveY(targetY, _duration).SetEase(_easeE).OnComplete(() => _OnEndCb?.Invoke());
    }
    public override void RefreshUI(int _focusIndex = -1)
    {
        base.RefreshUI(_focusIndex);
        if (_IsChangeCellDimensionsInApplyDataCb)
        {
            if (_focusIndex >= 0 && _focusIndex < _ControlPIs.Count)
            {
                _RunAllApplyDataCbs();
                _CalculateCellLocalV2sAndContentSizeDelta();
                ScrollToItem(_focusIndex);
            }
            else
            {
                PoolObj topMostPO = null;
                foreach (PoolObj aPO in _DataPOs)
                {
                    if (aPO.IsUnused) continue;
                    if (topMostPO == null)
                    {
                        topMostPO = aPO;
                        continue;
                    }
                    if (aPO.Id < topMostPO.Id) topMostPO = aPO;
                }
                PoolInfo topMostPI = GetInfo(topMostPO.Id);
                if (_DataSR.content.localPosition.y + topMostPI.LocalYTop > 0)
                {
                    PoolInfo nextPI = GetInfo(topMostPO.Id + 1);
                    if (nextPI != null) topMostPI = nextPI;
                }
                float offsetYFromTopViewport = _DataSR.content.localPosition.y + topMostPI.LocalYTop;
                _RunAllApplyDataCbs();
                _CalculateCellLocalV2sAndContentSizeDelta();
                _DataSR.content.localPosition = new(0, offsetYFromTopViewport - topMostPI.LocalYTop);
            }
            foreach (PoolObj aPO in _DataPOs) aPO.IsUnused = true;
        }
        else foreach (PoolObj aPO in _DataPOs) if (!aPO.IsUnused) _OnApplyDataCb(aPO.DataRT, GetInfo(aPO.Id), aPO.Id);
    }
    protected override void _CalculateCellLocalV2sAndContentSizeDelta()
    {
        base._CalculateCellLocalV2sAndContentSizeDelta();
        _IsCompleteCalculate = false;
        float localYTop = -m_PaddingTop;
        for (int i = 0; i < _ControlPIs.Count; i++)
        {
            PoolInfo aPI = GetInfo(i);
            float cellHeight = aPI.CellHeight;
            aPI.LocalYTop = localYTop;
            aPI.LocalYBot = aPI.LocalYTop - cellHeight;
            localYTop -= cellHeight + m_SpacingV2.y;
        }
        _DataSR.content.sizeDelta = new(_DataSR.viewport.rect.width, Mathf.Abs(localYTop + m_SpacingV2.y - m_PaddingBot));
        _IsCompleteCalculate = true;
        _HandleOnScroll();
    }
    protected override void _HandleOnScroll()
    {
        base._HandleOnScroll();
        Debug.Log("HANDLE ON SCROLL 1");
        if (!_IsCompleteCalculate) return;
        Debug.Log("HANDLE ON SCROLL 2");
        float topViewportY = -_DataSR.content.localPosition.y, bottomViewportY = topViewportY - _DataSR.viewport.rect.height;
        foreach (PoolObj aPO in _DataPOs)
        {
            if (aPO.IsUnused) continue;
            PoolInfo aPI = GetInfo(aPO.Id);
            if (aPI != null)
            {
                if (aPI.LocalYBot >= topViewportY || aPI.LocalYTop <= bottomViewportY) aPO.PutBackToPool();
            }
        }
        int countPoolInfo = _ControlPIs.Count;
        for (int i = 0; i < countPoolInfo; i++)
        {
            PoolInfo aPI = _ControlPIs[i];
            if (aPI.LocalYBot >= topViewportY || aPI.LocalYTop <= bottomViewportY) continue;
            _CheckAndSetUIPoolObject(aPI, new(0, aPI.LocalYTop));
        }
    }
}
