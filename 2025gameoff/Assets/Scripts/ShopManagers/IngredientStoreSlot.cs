using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 商店原料槽位
public class IngredientStoreSlot : MonoBehaviour
{
    [Header("UI References")]
    //public Image ingredientIcon;
    public TextMeshProUGUI ingredientNameText; // 显示名称和价格
    public TMP_InputField quantityInput;
    public Button increaseButton;
    public Button decreaseButton;

    [Header("Settings")]
    public int minQuantity = 0;
    public int maxQuantity = 999;

    private IngredientScriptObjs currentIngredient;
    private int currentQuantity = 0;

    // 初始化槽位
    public void Initialize(IngredientScriptObjs ingredient)
    {
        currentIngredient = ingredient;
        currentQuantity = 0;

        UpdateDisplay();

        // 添加按钮监听
        increaseButton.onClick.RemoveAllListeners();
        decreaseButton.onClick.RemoveAllListeners();
        quantityInput.onValueChanged.RemoveAllListeners();

        increaseButton.onClick.AddListener(IncreaseQuantity);
        decreaseButton.onClick.AddListener(DecreaseQuantity);
        quantityInput.onValueChanged.AddListener(OnQuantityInputChanged);

        // 初始刷新UI
        RefreshUI();
    }

    // 增加数量
    public void IncreaseQuantity()
    {
        if (currentQuantity >= maxQuantity) return;
        AudioManager.Instance.PlayStoreBuyGoods();
        int newQuantity = currentQuantity + 1;
        SetQuantity(newQuantity);
    }

    // 减少数量
    public void DecreaseQuantity()
    {
        if (currentQuantity <= minQuantity) return;
        AudioManager.Instance.PlayStoreBuyGoods();
        int newQuantity = currentQuantity - 1;
        SetQuantity(newQuantity);
    }

    // 设置数量
    public void SetQuantity(int newQuantity)
    {
        newQuantity = Mathf.Clamp(newQuantity, minQuantity, maxQuantity);

        if (newQuantity == currentQuantity) return;

        int quantityDifference = newQuantity - currentQuantity;

        if (quantityDifference > 0)
        {
            // 增加数量
            if (StoreManager.Instance.AddIngredientToCart(currentIngredient, quantityDifference))
            {
                currentQuantity = newQuantity;
                UpdateDisplay();
                RefreshUI();
                IngredientStoreSlotManager.Instance.RefreshAllSlotsUI(); // 更新所有槽位
            }
            else
            {
                // 如果不能增加，重置为0
                ResetToZero();
            }
        }
        else if (quantityDifference < 0)
        {
            // 减少数量
            int removeQuantity = Mathf.Abs(quantityDifference);
            if (StoreManager.Instance.RemoveIngredientFromCart(currentIngredient, removeQuantity))
            {
                currentQuantity = newQuantity;
                UpdateDisplay();
                RefreshUI();
                IngredientStoreSlotManager.Instance.RefreshAllSlotsUI(); // 更新所有槽位
            }
            else
            {
                // 如果不能减少，重置为0
                ResetToZero();
            }
        }
    }

    // 输入框变化回调
    private void OnQuantityInputChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            currentQuantity = 0;
            UpdateDisplay();
            RefreshUI();
            return;
        }

        if (int.TryParse(value, out int newQuantity))
        {
            // 检查输入值是否超出范围
            if (newQuantity < minQuantity || newQuantity > maxQuantity)
            {
                // 如果超出范围，设置为最接近的有效值
                newQuantity = Mathf.Clamp(newQuantity, minQuantity, maxQuantity);
                quantityInput.text = newQuantity.ToString();
            }

            SetQuantity(newQuantity);
        }
        else
        {
            // 输入无效，重置显示
            ResetToZero();
        }
    }

    // 重置数量为0
    private void ResetToZero()
    {
        if (currentQuantity > 0)
        {
            StoreManager.Instance.RemoveIngredientFromCart(currentIngredient, currentQuantity);
        }

        currentQuantity = 0;
        UpdateDisplay();
        RefreshUI();
        IngredientStoreSlotManager.Instance.RefreshAllSlotsUI(); // 更新所有槽位
    }

    // 更新显示
    private void UpdateDisplay()
    {
        if (currentIngredient == null) return;

        // 更新名称和价格信息
        int price = StoreManager.Instance.GetIngredientPrice(currentIngredient);
        string currency = LocalizationManager.Instance.GetText("currency_suffix");
        string name = currentIngredient.GetName();
        ingredientNameText.text = $"{name} - {price}{currency}";

        //// 更新图标
        //if (ingredientIcon != null)
        //{
        //    ingredientIcon.sprite = currentIngredient.icon;
        //}

        // 更新数量显示
        quantityInput.text = currentQuantity.ToString();

    }

    // 刷新UI状态
    public void RefreshUI()
    {
        if (currentIngredient == null) return;

        // 更新按钮状态
        decreaseButton.interactable = currentQuantity > minQuantity;
        increaseButton.interactable = currentQuantity < maxQuantity;
    }


    // 获取当前原料
    public IngredientScriptObjs GetIngredient()
    {
        return currentIngredient;
    }

    // 供Manager调用刷新语言
    public void UpdateLocale()
    {
        UpdateDisplay();
        Debug.LogError("Slot刷新了语言");
    }

    private void OnEnable()
    {
        if (currentIngredient != null)
        {
            UpdateLocale();
        }
    }
}