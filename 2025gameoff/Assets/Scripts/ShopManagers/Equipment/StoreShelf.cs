using UnityEngine;
using TMPro;

public class StoreShelf : MonoBehaviour
{
    [Header("3D组件引用")]
    public MeshRenderer modelRenderer;//模型
    public GameObject soldOutVisual;//售罄时的视觉表现

    public EquipmentDataSO _data;
    private bool _isSold;

    //初始化货架显示
    public void Setup(EquipmentDataSO data)
    {
        _data = data;
        _isSold = false;

        if (_data == null)
        {
            //没货，隐藏
            modelRenderer.gameObject.SetActive(false);
            soldOutVisual.SetActive(false);
            return;
        }

        // 有货
        modelRenderer.gameObject.SetActive(true);
        if (soldOutVisual != null) soldOutVisual.SetActive(true);

        //换贴图
        if (_data.equipmentTexture != null)
        {
            // 假设贴图在第一个材质球上
            modelRenderer.material.mainTexture = _data.equipmentTexture;
        }
    }

    //购买逻辑
    public void TryBuy()
    {
        if (_isSold || _data == null) return;
        if(_data.equipmentName == "refresh")
        {
            return;
        }
        //调用后台逻辑买东西
        if (StoreManager.Instance.BuyEquipment(_data))
        {
            Debug.Log("装备购买成功");
            SetSoldOut();
        }
        else
        {
            Debug.Log("金币不足");
        }
    }

    private void SetSoldOut()
    {
        _isSold = true;
        //视觉反馈
        modelRenderer.gameObject.SetActive(false);
        if (soldOutVisual != null) soldOutVisual.SetActive(false);
    }
}