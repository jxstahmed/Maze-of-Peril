using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableObjectController : MonoBehaviour
{
    // Start is called before the first frame update
    private AudioSource audioSource;
    private Rigidbody2D rigidBody;

    [Header("Knockback")]
    [SerializeField] float powBase = 3f;
    [SerializeField] float powStartX = 0.5f;
    [SerializeField] float powExpMultiplier = 5f;


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rigidBody = GetComponent<Rigidbody2D>();
        AudioManager.Instance.CreateTimer("moveable_" + GetInstanceID(), AudioManager.Instance.BoxMovementRate);
    }


    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == GameManager.Instance.PlayerTag)
        {
            if (rigidBody.velocity.x != 0 || rigidBody.velocity.y != 0)
            {
                AudioManager.Instance.PlayeOneShotFromAudioInstance(audioSource, AudioManager.Instance.BoxIsMoving, "moveable_" + GetInstanceID());
            }
        }
    }

    public void PushIntoDirection(Vector3 pushDirection, float force)
    {
        rigidBody.AddForce(pushDirection * force, ForceMode2D.Impulse);
        StartCoroutine(StopPush());
    }

    private IEnumerator StopPush()
    {
        float KnockbackFactor = powStartX;
        while (rigidBody.velocity.x != 0 || rigidBody.velocity.y != 0)
        {
            //if calculation goes wrong: enemy cant get knocked back longer than 0.75 seconds
            if (KnockbackFactor - powStartX > 0.75)
            {
                rigidBody.velocity = Vector2.zero;
                break;
            }

            //force function to iterate in gametime
            yield return new WaitForSeconds(Time.deltaTime);
            KnockbackFactor += Time.deltaTime;

            //calculate factor for exponential decrease of velocity
            float Factor = Mathf.Pow(powBase, KnockbackFactor * powExpMultiplier) * Time.deltaTime;

            //if velocity in x or y dir reaches or surpasses 0
            if (Mathf.Abs(rigidBody.velocity.x) - Mathf.Abs(rigidBody.velocity.normalized.x) * Factor <= 0)
                rigidBody.velocity = new Vector2(0, rigidBody.velocity.y);
            if (Mathf.Abs(rigidBody.velocity.y) - Mathf.Abs(rigidBody.velocity.normalized.y) * Factor <= 0)
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0);
            Vector2 dir = rigidBody.velocity;
            dir.Normalize();
            rigidBody.velocity -= dir * Factor;
        }
        yield return 0;

    }
}
