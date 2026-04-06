using UnityEngine;

public class PlayerInventoryController : MonoBehaviour
{
    public static PlayerInventoryController Instance;

    public int coins { get; private set; } = 0;

    public delegate int d_CoinChange(int oldValue);

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public void ChangeCoins(d_CoinChange changeFunction)
    {
        // Use the provided function to alter the current coins
        coins = changeFunction(coins);
        Debug.Log(coins);
    }

    private void OnDestroy()
    {
        if(Instance == this)
            Instance = null;
    }
}
