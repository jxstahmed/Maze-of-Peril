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
    [SerializeField] public int health = 100;
    [SerializeField] public int damage = 20;
    [SerializeField] public int attack_pause = 2;

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

    public float Health
    {
        set
        {
            health = (int) value;
            if(Health <= 0)
            {
                Defeated();
            }
        }
        get
        {
            return health;
        }
    }

    public void Defeated()
    {
        animator.SetTrigger("defeated");
    }

    public void RemoveEnemy()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.AttackPlayer(damage);
            StartCoroutine(GameManager.Instance.DelayedAction(attack_pause));
        }
    }

    private void onGameEventListen(Hashtable payload)
    {
        if ((GameState)payload["state"] == GameState.StopEnemies)
        {
            origin.velocity = new Vector2(0, 0);
            can_move = false;
        }
    }
}
