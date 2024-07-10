using D.Unity3dTools;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum EScrollSibling
{
    First,
    Center,
    Last,
}
public class SimpleLoopScrollView : MonoBehaviour
{
    public Transform view;
    public Transform content;
    public SimpleLoopItem item;
    public bool isLoop;

    private bool isInit;
    private int maxCount;
    private int itemCount;
    private int infoCount;
    private Vector2 topPos;
    private Vector2 botPos;
    private Vector2 itemRectSize;
    private int minInfoIndex;
    private int maxInfoIndex;
    private List<SimpleLoopItem> itemList = new List<SimpleLoopItem>();
    private RectTransform rectTransform;
    private EventTrigger eventTrigger;
    private Vector2 startPos;
    private bool isPointerEnter;
    // Start is called before the first frame update
    void Start()
    {
        //isInit = false;
        //InitSelf();
    }
    public void InitData(int count, Action<int, SimpleLoopItem> action, Action<int> _selectCall)
    {
        InitSelf();

        infoCount = count;
        content.RemoveAllChildren();
        itemCount = Mathf.Min(maxCount, infoCount);
        RectTransform itemRect = item.GetComponent<RectTransform>();
        itemRectSize = itemRect.sizeDelta;
        itemList.Clear();
        for (int i = 0; i < itemCount; i++)
        {
            int index = i;
            SimpleLoopItem child = Instantiate(item);
            child.transform.SetParent(content);
            child.transform.localPosition = itemRectSize.y * (index - 1) * Vector2.down;
            child.transform.localScale = Vector3.one;
            int infoIndex = index == 0 ? infoCount - 1 : index - 1;
            child.InitSelf(infoIndex, action, _selectCall);

            itemList.Add(child);
        }

        topPos = content.GetChild(0).localPosition;
        botPos = content.GetChild(itemCount - 1).localPosition;

        minInfoIndex = 0;
        maxInfoIndex = itemCount - 1;
    }
    private bool CheckIsScrollToBot()
    {
        if (isLoop) return false;

        //计算底部位置
        Vector2 rectSize = rectTransform.rect.size;
        float botPosY = -1 * rectSize.y + itemRectSize.y;

        //查找infoIndex = infoCount-1的item
        SimpleLoopItem currLastItem = null;
        for (int i = 0; i < itemCount; i++)
        {
            SimpleLoopItem child = itemList[i];
            if (child.infoIndex != infoCount - 1) continue;
            currLastItem = child;
        }
        if (currLastItem == null) return false;

        float checkPosY = currLastItem.transform.localPosition.y;
        //如果item的Y大于等于底部位置
        if (checkPosY >= botPosY && checkPosY <= 0) return true;

        return false;
    }
    private bool CheckIsScrollToTop()
    {
        if (isLoop) return false;

        Vector2 rectSize = rectTransform.rect.size;
        float botPosY = -1 * rectSize.y + itemRectSize.y;

        SimpleLoopItem currFirstItem = null;
        for (int i = 0; i < itemCount; i++)
        {
            SimpleLoopItem child = itemList[i];
            if (child.infoIndex != 0) continue;
            currFirstItem = child;
        }

        if (currFirstItem == null) return false;

        float checkPosY = currFirstItem.transform.localPosition.y;
        if (checkPosY <= 0 && checkPosY >= botPosY) return true;

        return false;
    }
    public void ScrollToInfoIndex(int _infoIndex, EScrollSibling _eSibling)
    {
        if (infoCount < maxCount) return;

        Vector2 rectSize = rectTransform.rect.size;

        Vector2 _topPos = Vector2.zero;
        Vector2 _botPos = (-1 * rectSize.y + itemRectSize.y) * Vector2.up;
        Vector2 cenPos = 0.5f * (_topPos + _botPos);
        //判断是否原本就处在范围内
        SimpleLoopItem checkItem = GetItemByInfoIndex(_infoIndex);
        if (checkItem != null)
        {
            Vector2 checkLocPos = checkItem.transform.localPosition;
            if (checkLocPos.y <= _topPos.y && checkLocPos.y >= _botPos.y) return;
        }

        //重置item位置
        Vector2 firstLocPos = Vector2.zero;
        int firstInfoIndex = 0;
        if (_eSibling == EScrollSibling.First)
        {
            firstLocPos = (_topPos.y + itemRectSize.y) * Vector2.up;
            firstInfoIndex = _infoIndex - 1;//如果小于0，暂且保留负数，赋值的时候再做处理
        }
        if (_eSibling == EScrollSibling.Last)
        {
            float posY = _botPos.y + itemRectSize.y * (itemCount - 2);
            firstLocPos = posY * Vector2.up;
            firstInfoIndex = _infoIndex - Mathf.CeilToInt(rectSize.y / itemRectSize.y);
        }
        if (_eSibling == EScrollSibling.Center)
        {
            firstInfoIndex = _infoIndex - (Mathf.CeilToInt(itemCount * 0.5f) - 2) - 1;
            int lastCount = _infoIndex - firstInfoIndex;
            float posY = cenPos.y + itemRectSize.y * lastCount;
            firstLocPos = posY * Vector2.up;
        }


        minInfoIndex = infoCount;
        //maxCount = -1;
        for (int i = 0; i < itemCount; i++)
        {
            int infoIndex = firstInfoIndex + i;
            if (infoIndex >= infoCount) infoIndex -= infoCount;
            else if (infoIndex < 0) infoIndex += infoCount;

            SimpleLoopItem child = itemList[i];
            child.transform.localPosition = firstLocPos + i * itemRectSize.y * Vector2.down;
            child.RefreshSelf(infoIndex);

            minInfoIndex = infoIndex < minInfoIndex ? infoIndex : minInfoIndex;
            maxInfoIndex = infoIndex > maxInfoIndex ? infoIndex : maxInfoIndex;
        }

    }
    private SimpleLoopItem GetItemByInfoIndex(int _infoIndex)
    {
        foreach (SimpleLoopItem _item in itemList)
        {
            if (_item.infoIndex == _infoIndex) return _item;
        }
        return null;
    }
    private void InitSelf()
    {
        if (isInit) return;
        isInit = true;

        rectTransform = content.GetComponent<RectTransform>();
        RectTransform itemRect = item.GetComponent<RectTransform>();

        //确保计算条件，强制设定UI的对齐方式
        rectTransform.pivot = itemRect.pivot = Vector2.up;
        rectTransform.anchorMax = Vector2.up;
        rectTransform.anchorMin = Vector2.up;
        itemRect.anchorMax = Vector2.up;
        itemRect.anchorMin = Vector2.up;

        Vector2 rectSize = rectTransform.sizeDelta;
        Vector2 itemRectSize = itemRect.sizeDelta;
        maxCount = Mathf.CeilToInt(rectSize.y / itemRectSize.y) + 2;

        eventTrigger = gameObject.GetOrAddComponent<EventTrigger>();
        eventTrigger.RemoveAllEventListener();
        eventTrigger.AddTrggerEventListener(EventTriggerType.BeginDrag, OnBeginDrag);
        eventTrigger.AddTrggerEventListener(EventTriggerType.Drag, OnDrag);
        eventTrigger.AddTrggerEventListener(EventTriggerType.EndDrag, OnEndDrag);
        eventTrigger.AddTrggerEventListener(EventTriggerType.PointerEnter, (_data) => { isPointerEnter = true; });
        eventTrigger.AddTrggerEventListener(EventTriggerType.PointerExit, (_data) => { isPointerEnter = false; });
    }
    private void Update()
    {
        if (isPointerEnter) OnCheckScrollWheel();
    }
    private void OnCheckScrollWheel()
    {
        // 检测鼠标滚轮的滚动
        float scrollWheel = Mouse.current.scroll.ReadValue().y;
        MoveScroll(scrollWheel * Vector2.up);
    }
    private void OnEndDrag(PointerEventData eventData)
    {
        if (infoCount < maxCount) return;
        startPos = Vector2.zero;
    }

    private void OnDrag(PointerEventData eventData)
    {
        if (infoCount < maxCount) return;
        // 将屏幕坐标转换为 Canvas 空间坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
        Vector2 offsetVec = localPoint - startPos;

        //判断边界
        bool isScrollToBot = CheckIsScrollToBot();
        bool isScrollToTop = CheckIsScrollToTop();
        if (offsetVec.y < 0) if (isScrollToTop) return;
        else if (offsetVec.y > 0) if (isScrollToBot) return;

        startPos = localPoint;
        offsetVec = new Vector2(0, offsetVec.y);
        MoveScroll(offsetVec);
    }

    private void MoveScroll(Vector2 offsetVec)
    {
        foreach (SimpleLoopItem _item in itemList) _item.OnMoveByOffset(offsetVec);

        foreach (SimpleLoopItem _item in itemList)
        {
            if (_item.transform.localPosition.y > topPos.y)
            {
                minInfoIndex++;
                maxInfoIndex++;
                if (maxInfoIndex == infoCount) maxInfoIndex = 0;
                if (minInfoIndex == infoCount) minInfoIndex = 0;

                Vector3 lastChildPos = content.GetChild(itemCount - 1).localPosition;
                Vector3 newPos = lastChildPos - new Vector3(0, itemRectSize.y);
                _item.transform.localPosition = newPos;
                _item.transform.SetAsLastSibling();
                _item.RefreshSelf(maxInfoIndex);
            }
            if (_item.transform.localPosition.y < botPos.y)
            {
                minInfoIndex--;
                maxInfoIndex--;
                if (minInfoIndex < 0) minInfoIndex = infoCount - 1;
                if (maxInfoIndex < 0) maxInfoIndex = infoCount - 1;

                Vector3 firstChildPos = content.GetChild(0).localPosition;
                Vector3 newPos = firstChildPos + new Vector3(0, itemRectSize.y);
                _item.transform.localPosition = newPos;
                _item.transform.SetAsFirstSibling();
                _item.RefreshSelf(minInfoIndex);
            }
        }
    }

    private void OnBeginDrag(PointerEventData eventData)
    {
        if (infoCount < maxCount) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
        startPos = localPoint;
    }


}
