using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class StoreDisplayManager : MonoBehaviour
{
    [Header("3D场景引用")]
    public List<StoreShelf> shelves;//4个盒子

    [Header("2dUI引用")]
    public Button refreshButton;//刷新按钮
    public TextMeshProUGUI refreshCostText;//刷新所需价格的显示文本
    public Button buyMicrowaveButton;//买微波炉按钮

    public static StoreDisplayManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (refreshButton != null)
            refreshButton.onClick.AddListener(OnRefreshClicked);

        if (buyMicrowaveButton != null)
            buyMicrowaveButton.onClick.AddListener(OnBuyMicrowaveClicked);

        //初始刷新
        RefreshShelves();
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

        //更新刷新价格文本
        if (refreshCostText != null)
        {
            int cost = StoreManager.Instance.GetRefreshCost();
            refreshCostText.text = $"刷新 ({cost} G)";
        }
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
        //更新一下金币显示？
    }
}