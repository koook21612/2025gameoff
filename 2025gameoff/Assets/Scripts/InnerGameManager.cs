using UnityEngine;

public class InnerGameManager : MonoBehaviour
{
    public bool isPlaying = false;

    private int currentGold = 50; // 初始金币
    private int currentReputation = 3; // 初始声望
    private int maxReputation = 3; // 声望上限
    private int completedCustomers = 0; // 完成的顾客数量

    // 微波炉升级相关
    [Header("微波炉升级")]
    public int MicrowavesCount = 1; // 微波炉数量

    public static InnerGameManager instance;
    private void Awake() {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //游戏开始
    public void GameStart()
    {
        currentGold = GameManager.instance.currentGold;
        currentReputation = GameManager.instance.currentReputation;
        maxReputation = GameManager.instance.maxReputation;
        completedCustomers = 0;

        EnterStore();
    }

    // 游戏结束
    private void GameOver()
    {
        Debug.Log("游戏结束！声望降为0，经营失败");
    }

    // 进入商店
    public void EnterStore()
    {
        isPlaying = false;
    }

    // 新的一天开始
    public void StartNewDay()
    {
        isPlaying = true;
    }

    // === 金币操作 ===
    public void AddGold(int amount)
    {
        currentGold += amount;
    }

    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            return true;
        }
        return false;
    }

    // === 声望操作 ===
    public void LoseReputation()
    {
        currentReputation = Mathf.Max(0, currentReputation - 1);

        // 检查游戏结束
        if (currentReputation <= 0)
        {
            GameOver();
        }
    }

    public void CompleteCustomer()
    {
        completedCustomers++;

        // 每完成10个顾客恢复1点声望
        if (completedCustomers % 10 == 0)
        {
            if (currentReputation < maxReputation)
            {
                currentReputation++;
            }
            else
            {
                int tipReward = Mathf.RoundToInt(currentGold * 0.05f);
                AddGold(tipReward);
                GameManager.instance.AddTalentPoint(1);
                Debug.Log($"声望已满，获得小费: {tipReward}和1天赋点");
            }
        }
    }

    // === 微波炉升级 ===
    public void Buy3DPrinterModule()
    {
    }

    public void BuyHighPowerModule()
    {

    }

    public void BuyCoolingModule()
    {

    }

    public void BuyNewMicrowave()
    {
        MicrowavesCount++;
    }

}
