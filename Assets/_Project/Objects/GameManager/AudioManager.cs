using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private AudioSource audioSource;

    [Header("Attachments")]
    [SerializeField] public GameSettings Settings;

    [Header("Timer Rates")]
    [SerializeField] public float PlayerMovementRate = 0.3f;
    [SerializeField] public float EnemyMovementRate = 0.3f;
    [SerializeField] public float BoxMovementRate = 0.3f;
    
    [Header("Overall")]
    [SerializeField] public CustomClip MenuClip;
    [SerializeField] public CustomClip GameplayClip;

    [Header("Player")]
    [SerializeField] public List<CustomClip> PlayerWalks;
    [SerializeField] public List<CustomClip> PlayerSprints;
    [SerializeField] public CustomClip PlayerGotHit;
    [SerializeField] public CustomClip PlayerDies;

    [Header("Weapon")]
    [SerializeField] public List<CustomClip> PlayerSlashSword1;

    [Header("Enemies")]
    [SerializeField] public List<CustomClip> EnemyWalks;
    [SerializeField] public CustomClip EnemyAttacks;
    [SerializeField] public CustomClip EnemyGotHit; 
    [SerializeField] public CustomClip EnemyDies;

    [Header("Interactalbe")]
    [SerializeField] public CustomClip ObjectNotYetUnlocked;
    [SerializeField] public CustomClip ChestOpens;
    [SerializeField] public CustomClip ChestCloses;
    [SerializeField] public CustomClip DoorOpens;
    [SerializeField] public CustomClip DoorCloses;

    [SerializeField] public CustomClip BoxIsMoving;
    [SerializeField] public CustomClip KeyIsCollected;

    [SerializeField] public CustomClip PotionIsPickedUp;
    [SerializeField] public CustomClip WeaponIsPickedUp;
    [SerializeField] public CustomClip EnemyIsPickedUp;

    [Header("Feedback")]
    [SerializeField] public List<CustomTimer> Timers = new List<CustomTimer>();





    [System.Serializable]
    public class CustomTimer
    {
        public CustomTimer(string tag, float rate, float time)
        {
            this.tag = tag;
            this.rate = rate;
            this.time = time;
        }

        public string tag;
        public float rate;
        public float time;
    }


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

    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void FixedUpdate()
    {
        ProcessTimers();
    }

    private void ProcessTimers()
    {
        Timers.ForEach(timer => { 
            timer.time -= Time.deltaTime;
        });
    }

    public void CreateTimer(string tag, float rate)
    {
        Timers.Add(new CustomTimer(tag, rate, 0));
    }

    private CustomTimer GetTimer(string tag)
    {
        CustomTimer t = null;
        Timers.ForEach(timer => {
            if (timer.tag == tag)
            {
                t = timer;
                return;
            }
        });

        return t;
    }

    private void SetTimer(CustomTimer t)
    {
        for(int i = 0; i < Timers.Count; i++)
        {
            if(Timers[i].tag == t.tag)
            {
                Timers[i] = t;
                break;
            }
        }
    }

    public void ToggleMenuAudio(bool active)
    {
        if (MenuClip == null) return;

        if (active)
        {
            audioSource.clip = MenuClip.clip;
            audioSource.volume = (Settings.MusicAudioLevel + MenuClip.volume) / 2;
            audioSource.loop = true;
            if (MenuClip.delay > 0) audioSource.PlayDelayed(MenuClip.delay);
            else audioSource.Play();
        }
        else
        {
            audioSource.Stop();
        }

    }

    public void ToggleGamePlayAudio(bool active)
    {
        if (GameplayClip == null) return;

        if(active)
        {
            audioSource.clip = GameplayClip.clip;
            audioSource.loop = true;
            audioSource.volume = (Settings.MusicAudioLevel + GameplayClip.volume) / 2;
            if (GameplayClip.delay > 0) audioSource.PlayDelayed(GameplayClip.delay);
            else audioSource.Play();
        } else
        {
            audioSource.Stop();
        }
        
    }

    public void PlayFromPosition(CustomClip customClip, Transform transform)
    {
        // this plays from a position to make a "3d" effect of the volume
        // if you want to disallow specific items, please check the type of "CustomClip" and disallow the items
        // don't disable it directly from the controllers, instead, disable it here
        if (customClip == null) return;
        if (transform == null) return;
        if (customClip.clip == null) return;

        if (customClip.delay > 0) StartCoroutine(PlayDelayedOnPoint(customClip, transform, customClip.delay));
        else AudioSource.PlayClipAtPoint(customClip.clip, transform.position, (Settings.FXAudioLevel + customClip.volume) / 2);
    }

    public void PlayeFromAudioInstance(AudioSource audiosrc, CustomClip customClip, bool toggle = true)
    {
        if (audiosrc == null) return;
        if (customClip == null) return;
        if (customClip.clip == null) return;

        audiosrc.clip = customClip.clip;
        audiosrc.volume = (Settings.FXAudioLevel + customClip.volume) / 2;

        if (toggle)
        {
            if (customClip.delay > 0) audiosrc.PlayDelayed(customClip.delay);
            else audiosrc.Play();
        } else
            audiosrc.Stop();
    }
    public void PlayeOneShotFromAudioInstance(AudioSource audiosrc, CustomClip customClip, string tag = null)
    {
        if (audiosrc == null) return;
        if (customClip == null) return;
        if (customClip.clip == null) return;

        if(tag != null)
        {
            CustomTimer ct = GetTimer(tag);
            if (ct == null) return;
            if (ct.time >= 0f) return;
            ct.time = ct.rate;
            SetTimer(ct);
            // it means that this is method that gets called in each loop, therefore, we need a selected timer
        }

        if (customClip.delay > 0) StartCoroutine(PlayDelayedOneShot(audiosrc, customClip, customClip.delay));
        else audiosrc.PlayOneShot(customClip.clip, (Settings.FXAudioLevel + customClip.volume) / 2);


    }

    IEnumerator PlayDelayedOneShot(AudioSource audiosrc, CustomClip customClip, float delay)
    {
        yield return new WaitForSeconds(delay);
        audiosrc.PlayOneShot(customClip.clip, (Settings.FXAudioLevel + customClip.volume) / 2);
    }
    

    IEnumerator PlayDelayedOnPoint(CustomClip customClip, Transform transform, float delay)
    {
        yield return new WaitForSeconds(delay);
        AudioSource.PlayClipAtPoint(customClip.clip, transform.position, (Settings.FXAudioLevel + customClip.volume) / 2);
    }


    //überladene funktionen um support für mehrere Audiofiles für eine action hinzuzufügen
    public void PlayFromPosition(List<CustomClip> customClipList, Transform transform)
    {
        PlayFromPosition(SelectRandomClip(customClipList), transform);
    }

    public void PlayeFromAudioInstance(AudioSource audiosrc, List<CustomClip> customClipList, bool toggle = true)
    {
        PlayeFromAudioInstance(audiosrc, SelectRandomClip(customClipList), toggle);
    }
    public void PlayeOneShotFromAudioInstance(AudioSource audiosrc, List<CustomClip> customClipList, string tag = null)
    {
        PlayeOneShotFromAudioInstance(audiosrc, SelectRandomClip(customClipList), tag);
    }

    void PlayDelayedOneShot(AudioSource audiosrc, List<CustomClip> customClipList, float delay)
    {
        PlayDelayedOneShot(audiosrc, SelectRandomClip(customClipList), delay);
    }


    void PlayDelayedOnPoint(List<CustomClip> customClipList, Transform transform, float delay)
    {
        PlayDelayedOnPoint(SelectRandomClip(customClipList), transform, delay);
 
    }



    public CustomClip SelectRandomClip(List<CustomClip> customClipList)
    {
        if (customClipList == null || customClipList.Count == 0) return null;
        int rand_int = Random.Range(0, customClipList.Count);
        CustomClip customClip = customClipList[rand_int];
        return customClip;

    }
}

