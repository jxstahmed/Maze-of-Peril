using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] public float Speed = 1f;
    [SerializeField] float Health = 100;
    [SerializeField] float Stamina = 100;
    [SerializeField] SwordAttack SwordAttack;

    private Animator animator;
    private SpriteRenderer sprite_renderer;
    private Rigidbody2D rigidBody;

    private bool can_move = true;

    private KeyCode KeyMoveRight = KeyCode.D;
    private KeyCode KeyMoveUp = KeyCode.W;
    private KeyCode KeyMoveLeft = KeyCode.A;
    private KeyCode KeyMoveDown = KeyCode.S;

    private KeyCode KeySwordAttack = KeyCode.Space;

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
        sprite_renderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();
    }


    private void FixedUpdate()
    {
        if (!can_move)
            return;

        PlayerAttack();
        PlayerMove();
    }

    private void PlayerAttack()
    {
        if (can_move && Input.GetKey(KeySwordAttack))
        {
            LockMovement();
            animator.SetTrigger("sword_attack");
        }
    }

    private void PlayerMove()
    {
        Vector2 dir = Vector2.zero;
        // get WASD Input
        if (Input.GetKey(KeyMoveLeft))
            dir.x -= 1;
        if (Input.GetKey(KeyMoveRight))
            dir.x += 1;
        if (Input.GetKey(KeyMoveUp))
            dir.y += 1;
        if (Input.GetKey(KeyMoveDown))
            dir.y -= 1;

        dir.Normalize();

        // set movement animation direction
        animator.SetBool("P2_is_moving_h", dir.x != 0);
        animator.SetBool("P2_is_moving_up", (dir.x == 0) && (dir.y > 0));
        animator.SetBool("P2_is_moving_down", (dir.x == 0) && (dir.y < 0));

        // set direction in which the player is facing
        if (dir.x != 0)
            sprite_renderer.flipX = dir.x < 0;
        rigidBody.velocity = Speed * dir;
    }

    public void LockMovement()
    {
        rigidBody.velocity = new Vector2(0, 0);
        can_move = false;
    }
    public void UnlockMovement()
    {
        can_move = true;
        SwordAttack.StopAttack();
        animator.SetTrigger("sword_attack_reset");
    }

    public void SwordAttack_h()
    {
        LockMovement();
        if (sprite_renderer.flipX)
            SwordAttack.AttackLeft();
        else
            SwordAttack.AttackRight();

    }
    public void SwordAttack_up()
    {

        LockMovement();
        SwordAttack.AttackUp();
    }
    public void SwordAttack_down()
    {
        LockMovement();
        SwordAttack.AttackDown();
    }

    public void applyDamage(float enemyDamage)
    {
        if (Health <= 0)
        {
            return;
        }

        Health -= enemyDamage;
        Debug.Log("Player got hit with: " + enemyDamage + ", health is: " + Health);

        if (Health <= 0)
        {
            killPlayer();
        }
    }

    private void stopPlayer()
    {
        LockMovement();
    }

    private void killPlayer()
    {
        stopPlayer();
         GameManager.Instance.StopEnemies(true);
        animator.SetBool("die", true);
    }


    private void onGameEventListen(Hashtable payload)
    {
        if ((GameState)payload["state"] == GameState.AttackPlayer)
        {
            applyDamage((float)payload["damage"]);
        }
    }
}