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
    public void ShowItemInfo(StoreShelf shelf)
    {
        if (shelf == null) return;

        currentFocusedShelf = shelf;

        if (itemInfoPanel != null)
            itemInfoPanel.SetActive(true);

        // 更新UI文本
        if (NameText != null)
            NameText.text = shelf._data.equipmentName;

        if(shelf._data.equipmentName == "refresh")
        {
            CostText.text = $"{StoreManager.Instance.GetRefreshCost()} G";
        }
        if (CostText != null)
            CostText.text = $"{shelf._data.equipmentPrice} G";

        if (DescriptionText != null)
            DescriptionText.text = shelf._data.description;
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
    private void OnRefreshClicked()
    {
        StoreManager.Instance.TryRefreshShelf();
        RefreshShelves();
    }

    private void OnBuyMicrowaveClicked()
    {
        StoreManager.Instance.BuyMicrowave();
        buyMicrowaveButton.SetActive(false);
    }
}