using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static ModifierUtil;

public class PlayerUIController : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text coinText;

    private void Start()
    {
        PlayerController.Instance.statusController.OnHealthChange += OnPlayerHealthChange;
        PlayerController.Instance.statusController.OnFlavorStatusChanged += OnPlayerFlavorStatusChanged;
        PlayerController.Instance.inventoryController.OnCoinCountChanged += OnCoinCountChanged;

        SetHealthText(PlayerController.Instance.statusController.GetHealth());
        SetCoinText(PlayerController.Instance.inventoryController.coins);
    }

    private void OnPlayerFlavorStatusChanged()
    {
        PlayerStatusController statusController = PlayerController.Instance.statusController;
        Dictionary<Flavor, Timer> timers = statusController.GetFlavorTimers();
        string test = string.Empty;
        foreach (Flavor flavor in timers.Keys)
        {
            test += flavor.ToString() + ": " + Mathf.FloorToInt(timers[flavor].GetCurrentTime()) + "/" + timers[flavor].GetRepeats().ToString() + "\n";
        }
        statusText.text = test;
    }

    private void OnPlayerHealthChange(float newHealth)
    {
        SetHealthText(newHealth);
    }
    private void OnCoinCountChanged(int oldValue, int newValue)
    {
        SetCoinText(newValue);
    }

    public void SetHealthText(float health)
    {
        healthText.text = "Health: " + Mathf.FloorToInt(health).ToString();
    }
    public void SetCoinText(int coins)
    {
        coinText.text = "Coins: " + coins.ToString();
    }
}
