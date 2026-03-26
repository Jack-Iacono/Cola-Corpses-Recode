using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static ModifierUtil;

public class PlayerUIController : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text statusText;

    private void Start()
    {
        PlayerController.Instance.statusController.OnHealthChange += OnPlayerHealthChange;
        //PlayerController.Instance.statusController.OnFlavorStatusChanged += OnPlayerFlavorStatusChanged;
        SetHealthText(PlayerController.Instance.statusController.GetHealth());
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

    public void SetHealthText(float health)
    {
        healthText.text = Mathf.FloorToInt(health).ToString();
    }
}
