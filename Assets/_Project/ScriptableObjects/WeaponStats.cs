using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon Stats")]
public class WeaponStats : ScriptableObject
{

    public GameObject prefab;
    public string ID;
    public int SwingType;
    public float Damage;
    public float StaminaReductionRate;
    public float ComboInbetweenTime;
    public int MaxCombos;

    [Header("Prefab")]
    public Vector2 Position;

    [Header("Tracker")]
    public int HitsCount;
    public int KilledEnemies;
    public int HighestComboCount;
    public int CombosCount;
}
