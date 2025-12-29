using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class BetterDropDown : MonoBehaviour, ISelectHandler
{
    private ScrollRect scrollRect;
    private float scrollPosition = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      scrollRect = GetComponentInParent<ScrollRect>(true);
        int childCount = scrollRect.content.transform.childCount - 1;
        int childIndex = transform.GetSiblingIndex();

        childIndex = childIndex < ((float)childCount / 2) ? childIndex - 1 : childIndex;

        scrollPosition = 1 - ((float)childIndex / childCount);


    }

    public void OnSelect(BaseEventData eventData)
    {
        if (scrollRect)
        scrollRect.verticalScrollbar.value = scrollPosition;
    }
}
