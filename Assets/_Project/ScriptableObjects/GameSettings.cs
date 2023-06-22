using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Settings")]
public class GameSettings : ScriptableObject
{
    
    [Header("Values")]
    public float GameplayAudioLevel = 1;
    public float FXAudioLevel = 1;
}
