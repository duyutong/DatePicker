using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SimpleLoopItem : MonoBehaviour
{
    public int infoIndex = 0;
    public Toggle toggle;
    public Text txtValue;

    private Action<int> onSelect;
    private Action<int, SimpleLoopItem> onRefresh;
    public void InitSelf(int _infoIndex, Action<int, SimpleLoopItem> _call, Action<int> _selectCall)
    {
        onRefresh = _call;
        infoIndex = _infoIndex;
        onRefresh?.Invoke(infoIndex, this);
        onSelect = _selectCall;

        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }
    public void RefreshSelf(int _infoIndex)
    {
        infoIndex = _infoIndex;
        onRefresh?.Invoke(infoIndex, this);
    }
    public void OnMoveByOffset(Vector3 offsetVec)
    {
        transform.localPosition += offsetVec;
    }
    public void OnToggleValueChanged(bool isOn)
    {
        if (!isOn) return;
        onSelect?.Invoke(infoIndex);
    }
}
