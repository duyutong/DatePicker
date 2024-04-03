using D.Unity3dTools;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SimpleLoopScrollView : MonoBehaviour
{
    public Transform view;
    public Transform content;
    public SimpleLoopItem item;


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
    private void InitSelf()
    {
        if (isInit) return;
        isInit = true;

        rectTransform = GetComponent<RectTransform>();
        RectTransform itemRect = item.GetComponent<RectTransform>();
        Vector2 rectSize = rectTransform.sizeDelta;
        Vector2 itemRectSize = itemRect.sizeDelta;
        maxCount = Mathf.CeilToInt(rectSize.y / itemRectSize.y) + 2;

        Image img = gameObject.GetOrAddComponent<Image>();
        img.color = new Color(0, 0, 0, 0);

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
        // 设置 UI 元素的 anchoredPosition
        Vector2 offsetVec = localPoint - startPos;
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
