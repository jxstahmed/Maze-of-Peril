using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SerializeField] private Rigidbody2D target;
    [SerializeField] private Rigidbody2D origin;

    [SerializeField] private float movement_speed = 0.6f;
    [SerializeField] private float max_follow_radius = 8f;
    [SerializeField] private float min_follow_radius = 0f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private int attack_pause = 2;
    [SerializeField] private float health = 100;

    private bool can_move = true;

    Animator animator;

    // subscribe
    private void Awake()
    {
        GameManager.GameEvent += onGameEventListen;
    }

    // unsubscribe
    private void OnDestroy()
    {
        GameManager.GameEvent -= onGameEventListen;

    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    

    private void FixedUpdate()
    {
        validateMovement();
    }
    

    private void validateMovement()
    {
        if (!can_move || origin == null || target == null)
            return;

        // Check position
        Vector2 target_position = GameManager.Instance.getTargetPosition(origin.transform, target.transform);
        bool isRight = target_position.x < 0;
        bool isTop = target_position.y < 0;


        // Flip if different position
        spriteRenderer.flipX = !isRight;


        if (
            Vector2.Distance(origin.position, target.position) > min_follow_radius && Vector2.Distance(origin.position, target.position) < max_follow_radius)
        {
            // Keep going if inside the radius
            Vector2 dir = new Vector2(target_position.x * -1, target_position.y * -1);
            dir.Normalize();
            origin.velocity = movement_speed * dir;
        }
        else
        {
            // stop the movement if it's outside the radius
            origin.velocity = new Vector2(0, 0);
        }
    }

    private Vector2 getTargetPosition(Transform origin, Transform target)
    {
        Vector2 pos = new Vector2(0f, 0f);

        pos.x = origin.position.x - target.position.x;
        pos.y = origin.position.y - target.position.y;

        return pos;
    }

    public void applyDamage(float weaponDamage)
    {
        if (health <= 0)
        {
            return;
        }

        health -= weaponDamage;
        Debug.Log("Enemy got hit with: " + weaponDamage + ", health is: " + health);

        if (health <= 0)
        {
            killEnemy();
        }
    }

    public void RemoveEnemy()
    {
        Destroy(gameObject);
    }

    private void stopEnemy() {
        can_move = false;
        origin.velocity = new Vector2(0, 0);
    }

    private void killEnemy() {
        stopEnemy();
        animator.SetTrigger("defeated");
    }

    private float last_hit = 0;


    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player"))
        {
            Debug.Log("First hit");
            last_hit = Time.time;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Current time & delay
            // First touch at second 2
            // We test at second 5, attack pause is 2 => first touch + 2 => 5 > 4, we can hit
            // set the time of last_hit to 5 and then retry 
            // player gets away and OnExit is trigger
            // player gets back at 7.2 and when he first touches => he gets hit

            float allowed_hit_time = last_hit + attack_pause;
            if(last_hit != 0 && Time.time > allowed_hit_time) {
                // hitting 
                Debug.Log("hitting");
                last_hit = Time.time;
                GameManager.Instance.AttackPlayer(damage);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("out of the way");
            last_hit = 0;
        }
    }


    private void onGameEventListen(Hashtable payload)
    {
        if ((GameState)payload["state"] == GameState.StopEnemies)
        {
            stopEnemy();
        }
    }
}
