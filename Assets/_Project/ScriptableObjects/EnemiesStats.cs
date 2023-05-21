using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemies Stats")]
public class EnemiesStats : ScriptableObject
{
    [Header("Generic")]
    public string Name;
    public int Rank;

    [Header("Stats")]
    public float Health;
    public float Damage = 20f;

    [Header("Follow")]
    public float FollowSpeed = 0.75f;
    public float MaxSightRadius = 6f;
    [Tooltip("Start chasing the player after locking a sighting into him for x seconds")]
    public float FollowAfterLockedSightingForSeconds = 2f;

    [Header("Patrol")]
    public float PatrolSpeed = 0.95f;
    public float PatrolRadius = 25;


    public float StaminaRegenerationRate;
    public float HealthRegenerationRate;
    public float SpeedStaminaReductionRate;


    [Header("Values")]
    public float IncrementEverySeconds = 1;
    public float RegenerateHealthCooldownWhenHit = 2f;
    public float RegenerateStaminaCooldownWhenRun = 2f;
    public float AttackCooldown = 2f;
}
