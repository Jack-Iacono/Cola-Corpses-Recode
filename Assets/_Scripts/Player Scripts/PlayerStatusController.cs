using UnityEngine;
using static IDamageable;

public class PlayerStatusController : StatusSystem
{
    protected override void Awake()
    {
        // Initialize the status and health systems
        base.Awake();

        // Add in the necessary hurt sounds onto the player
        AudioManager.AddAudioSources(AudioManager.SoundType.p_Hurt, 5, gameObject);
    }

    public override void ChangeHealth(d_ChangeHealth change)
    {
        float oldHealth = health;
        base.ChangeHealth(change);

        if(health < oldHealth)
        {
            // Play the hurt sound
            AudioManager.Play(AudioManager.SoundType.p_Hurt, gameObject);
        }
        InvokeOnHealthChange();
    }
    public override void ResetHealth()
    {
        base.ResetHealth();
        InvokeOnHealthChange();
    }
    protected override void HealthEmpty()
    {
        Debug.Log("Health Empty");
        InvokeOnHealthEmpty();
    }

    public override DamageableType GetDamageableType()
    {
        return IDamageable.DamageableType.PLAYER;
    }
}
