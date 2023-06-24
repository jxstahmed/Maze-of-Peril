using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableObjectController : MonoBehaviour
{
    // Start is called before the first frame update
    private AudioSource audioSource;


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        AudioManager.Instance.CreateTimer("moveable_" + GetInstanceID(), AudioManager.Instance.BoxMovementRate);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == GameManager.Instance.PlayerTag)
        {
            AudioManager.Instance.PlayeOneShotFromAudioInstance(audioSource, AudioManager.Instance.BoxIsMoving, "moveable_" + GetInstanceID());
        }
    }
}
