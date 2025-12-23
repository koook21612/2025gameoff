using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class StoreDisplayManager : MonoBehaviour
{
    [Header("3D场景引用")]
    public List<StoreShelf> shelves;//4个盒子

    [Header("2dUI引用")]
    public TextMeshProUGUI CostText;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI DescriptionText;

    public GameObject itemInfoPanel;

    public GameObject refreshButton;//刷新按钮
    public GameObject buyMicrowaveButton;//买微波炉按钮
    private StoreShelf currentFocusedShelf;

    public static StoreDisplayManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 初始化时隐藏商品信息面板
        if (itemInfoPanel != null)
            itemInfoPanel.SetActive(false);
    }

    //将数据同步到3D场景
    public void RefreshShelves()
    {
        if (StoreManager.Instance == null) return;

        //获取数据
        List<EquipmentDataSO> items = StoreManager.Instance.currentShelfEquipments;

        //分配给货架
        for (int i = 0; i < shelves.Count; i++)
        {
            if (i < items.Count)
            {
                shelves[i].Setup(items[i]);
            }
            else
            {
                shelves[i].Setup(null);
            }
        }

        ////更新刷新价格文本
        //if (refreshCostText != null)
        //{
        //    int cost = StoreManager.Instance.GetRefreshCost();
        //    refreshCostText.text = $"刷新 ({cost} G)";
        //}
    }
    public void ShowCantBuy()
    {
        if (itemInfoPanel != null)
            itemInfoPanel.SetActive(true);
        DescriptionText.text = LocalizationManager.Instance.GetText("cant_buy");
    }
    public void ShowItemInfo(StoreShelf shelf)
    {
        if (shelf == null) return;

        currentFocusedShelf = shelf;

        if (itemInfoPanel != null)
            itemInfoPanel.SetActive(true);

        // 更新UI文本
        if (NameText != null)
            NameText.text = shelf._data.GetName();

        if (CostText != null)
            if (shelf._data.equipmentName == "refresh")
            {
                NameText.text = shelf._data.GetName();
                CostText.text = $"{StoreManager.Instance.GetRefreshCost()} G";
            }
            else if (shelf._data.equipmentName == "newMicrowave") {
                NameText.text = shelf._data.GetName();
                CostText.text = $"{shelf._data.equipmentPrice} G";
            }
            else
            {
                int displayPrice = shelf._data.equipmentPrice;
                if (InnerGameManager.Instance.Supplier)
                {
                    displayPrice = CalculateDiscountedPrice(shelf._data.equipmentPrice);
                }
                CostText.text = $"{displayPrice} G";
            }

        if (DescriptionText != null)
            DescriptionText.text = shelf._data.GetDescription();
    }

    private int CalculateDiscountedPrice(int originalPrice)
    {
        float discountedPrice = originalPrice * 0.9f;
        return Mathf.CeilToInt(discountedPrice);
    }

    public void HideItemInfo()
    {
        if (itemInfoPanel != null)
            itemInfoPanel.SetActive(false);

        ClearItemInfo();
    }
    private void ClearItemInfo()
    {
        currentFocusedShelf = null;
        if (NameText != null) NameText.text = "";
        if (CostText != null) CostText.text = "";
        if (DescriptionText != null) DescriptionText.text = "";
    }

    //UI按钮点击逻辑
    public void OnRefreshClicked()
    {
        StoreManager.Instance.TryRefreshShelf();
        RefreshShelves();
    }

    public void OnBuyMicrowaveClicked()
    {
        StoreManager.Instance.BuyMicrowave();
    }
}