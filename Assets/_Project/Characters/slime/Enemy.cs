using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public Rigidbody2D target;
    public Rigidbody2D origin;

    [SerializeField] public float movement_speed = 0.6f;
    [SerializeField] public float max_follow_radius = 8f;
    [SerializeField] public float min_follow_radius = 0f;
    [SerializeField] public float damage = 20f;
    [SerializeField] public int attack_pause = 2;
    [SerializeField] public float health = 100;

    private bool can_move = true;
    private float last_hit = 0;

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
        if (origin != null && target != null && can_move)
        {

            // Check position
            Vector2 target_position = GameManager.Instance.getTargetPosition(origin.transform, target.transform);
            bool isRight = target_position.x < 0;
            bool isTop = target_position.y < 0;


            // Flip if different position

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !isRight;
            }


            if (
                Vector2.Distance(origin.position, target.position) > min_follow_radius && Vector2.Distance(origin.position, target.position) < max_follow_radius)
            {
                // Keep going if inside the radius
                origin.velocity = movement_speed * new Vector2(target_position.x * -1, target_position.y * -1);
            }
            else
            {
                // stop the movement if it's outside the radius
                origin.velocity = new Vector2(0, 0);
            }
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



    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if(Time.time >= (attack_pause + last_hit)) {
                Debug.Log("Touching somethin");
                last_hit = Time.time;
                GameManager.Instance.AttackPlayer(damage);
            }
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
