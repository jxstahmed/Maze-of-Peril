using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Settings")]
public class GameSettings : ScriptableObject
{

    [Header("Scenes")]
    public int SCENE_MAIN = 0;
    public int SCENE_LEVEL_1 = 1;
    public int SCENE_LEVEL_2 = 2;

    [Header("Values")]
    public float MusicAudioLevel = 1;
    public float FXAudioLevel = 1;

    [Header("Labels")]
    public float LabelTextSpeed = 0.8f;
    public float LabelTextFadeOutTime = 6f;
}
