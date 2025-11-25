using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.Progress;



public class Interactable : MonoBehaviour
{
    public Item item;
    public UnityEvent onInteract;
    [Header("交互设置")]
    public bool isInstantInteract = false;//如果勾选，点击后直接触发事件，不移动相机

    public StoreShelf storeShelf; // 关联的商品货架
    // 新的交互方法，专门用于购买
    public void TryBuyItem()
    {
        if (storeShelf != null)
        {
            storeShelf.TryBuy();
        }
    }
}
