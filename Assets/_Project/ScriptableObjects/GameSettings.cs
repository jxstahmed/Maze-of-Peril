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

    [Header("Feedback")]
    public bool PlayerBloodFeedback = true;
    public bool EnemyBloodFeedback = true;
    

    [Header("Labels")]
    [SerializeField] public LabelOptions ObjectiveLabel;
    [SerializeField] public LabelOptions PlayerBloodFeedbackLabel;
    [SerializeField] public LabelOptions EnemyBloodFeedbackLabel;

    //public string LabelTextColor = "#fff";
    //public float LabelTextSpeed = 0.8f;
    //public float LabelTextFadeOutTime = 6f;

    //public string PlayerTextColor = "#fff";
    //public float PlayerTextSpeed = 0.8f;
    //public float PlayerTextFadeOutTime = 3f;

    //public string EnemyTextColor = "#fff";
    //public float EnemyTextSpeed = 0.8f;
    //public float EnemyTextFadeOutTime = 3f;

    [System.Serializable]
    public class LabelOptions
    {
        [SerializeField] public Color text_color;
        [SerializeField] public float text_speed;
        [SerializeField] public float fade_out_time;
        [SerializeField] public float text_size = 6f;
    }
}
