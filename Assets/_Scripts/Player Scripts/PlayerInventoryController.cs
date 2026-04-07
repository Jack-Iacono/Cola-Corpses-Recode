using UnityEngine;

public class PlayerInventoryController : MonoBehaviour
{
    public static PlayerInventoryController Instance;

    public int coins { get; private set; } = 0;
    private Vector2 coinLimits = new Vector2(0, 20);

    public delegate int d_CoinChange(int oldValue);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Add in the necessary hurt sounds onto the player
            AudioManager.AddAudioSources(AudioManager.SoundType.i_CoinCollect, 3, gameObject);
        }
        else
            Destroy(this);
    }

    public bool ChangeCoins(d_CoinChange changeFunction)
    {
        // Use the provided function to alter the current coins
        int old = coins;
        coins = (int)Mathf.Clamp(changeFunction(coins), coinLimits.x, coinLimits.y);

        if(old < coins)
            AudioManager.Play(AudioManager.SoundType.i_CoinCollect, gameObject);

        // If there was no change in the coin count, this means there was an issue doing the action and it should be rejected
        if (coins == old)
            return false;
        else
            return true;
    }

    private void OnDestroy()
    {
        if(Instance == this)
            Instance = null;
    }
}
