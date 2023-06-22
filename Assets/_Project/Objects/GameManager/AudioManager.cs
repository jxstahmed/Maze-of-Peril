using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public GameSettings Settings;
    public AudioSource AudioSource;

    [Header("Player")]
    [SerializeField] public CustomClip GameplayClip;


    [Header("Player")]
    [SerializeField] public CustomClip PlayerWalks;
    [SerializeField] public CustomClip PlayerSprints;
    [SerializeField] public CustomClip PlayerGotHit;
    [SerializeField] public CustomClip PlayerDies;

    [Header("Weapon")]
    [SerializeField] public CustomClip PlayerSlashSword1;

    [Header("Enemies")]
    [SerializeField] public CustomClip EnemyWalks;
    [SerializeField] public CustomClip EnemyHits;
    [SerializeField] public CustomClip EnemyDies;

    [Header("Interactalbe")]
    [SerializeField] public CustomClip ObjectNotYetUnlocked;
    [SerializeField] public CustomClip ChestOpens;
    [SerializeField] public CustomClip DoorOpens;

    [SerializeField] public CustomClip BoxIsMoving;
    [SerializeField] public CustomClip KeyIsCollected;

    [SerializeField] public CustomClip PotionIsPickedUp;
    [SerializeField] public CustomClip WeaponIsPickedUp;
    [SerializeField] public CustomClip EnemyIsPickedUp;



    [System.Serializable]
    public class CustomClip
    {
        public AudioClip clip;
        public float volume = 1;
        public float delay = 0;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    public void PlayGamePlay()
    {

    }

    public void PlayFromPosition(CustomClip customClip, Transform transform)
    {
        AudioSource.PlayClipAtPoint(customClip.clip, transform.position, (Settings.FXAudioLevel + customClip.volume) / 2);
    }
}

