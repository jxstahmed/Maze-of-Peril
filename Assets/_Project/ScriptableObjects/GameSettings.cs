using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Game")]
    public List<LevelOption> LevelsOptions; 

    [Header("Scenes")]
    public int SCENE_MAIN = 0;
    public int SCENE_LEVEL_1 = 1;
    public int SCENE_LEVEL_2 = 2;
    public int SCENE_LEVEL_3 = 3;

    public string LEVEL_1_END_ID = "";
    public string LEVEL_2_END_ID = "";
    public string LEVEL_3_END_ID = "";

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
    [System.Serializable]
    public class LevelOption
    {
        public LevelOption()
        {

        }

        public LevelOption(int level_n)
        {
            level_number = level_n;
        }


        [SerializeField] public int level_number;

        [SerializeField] public float player_health_original = 50f;
        [SerializeField] public float player_health = 50f;
        [SerializeField] public float player_stamina = 100f;
        [SerializeField] public float player_stamina_original = 100f;

        [SerializeField] public float enemy_speed_factor = 1f;
        [SerializeField] public float enemy_damage_factor = 1f;

        [SerializeField] public float player_stamina_increase_factor = 1f;
        [SerializeField] public float player_blood_increase_factor = 1f;
        [SerializeField] public float player_speed_factor = 1f;
        [SerializeField] public float player_speed_reduction_rate_factor = 1f;

        [SerializeField] public float weapon_damage_factor = 1f;
        [Tooltip("Stamina when weapon is utilized, we can increase or decrease the rate by level")]
        [SerializeField] public float weapon_reduction_factor = 1f;
    }
}
