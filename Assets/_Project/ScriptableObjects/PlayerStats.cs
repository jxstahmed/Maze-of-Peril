using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Stats")]
public class PlayerStats : ScriptableObject
{
    [Header("Generic")]
    public int CurrentLevel;
    
    [Header("Stats")]
    public float Health;
    public float Stamina;
    public float Speed;

    public float StaminaRegenerationRate;
    public float HealthRegenerationRate;
    public float SpeedStaminaReductionRate;

    [Header("Multiplier")]
    public float WalkFactor;
    public float RunFactor;
    public float DamageFactor;

    [Header("Values")]
    public float IncrementEverySeconds = 1;
    public float RegenerateHealthCooldownWhenHit = 2f;
    public float RegenerateStaminaCooldownWhenRun = 2f;

    [Header("Weapons")]

    public string WeaponId;
}
