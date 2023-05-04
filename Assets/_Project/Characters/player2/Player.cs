using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 1f;
    public float health = 100;

    private Animator animator;
    SpriteRenderer spriteRenderer;
    public SwordAttack swordAttack;
    bool can_move = true;

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
        if (!can_move)
            return;

        if (can_move && Input.GetKey(KeyCode.Space))
        {
            animator.SetTrigger("sword_attack");
        }
        Vector2 dir = Vector2.zero;


        //get WASD Input
        if (Input.GetKey(KeyCode.A))
            dir.x -= 1;
        if (Input.GetKey(KeyCode.D))
            dir.x += 1;
        if (Input.GetKey(KeyCode.W))
            dir.y += 1;
        if (Input.GetKey(KeyCode.S))
            dir.y -= 1;

        dir.Normalize();

        //set movement animation direction
        animator.SetBool("P2_is_moving_h", dir.x != 0);
        animator.SetBool("P2_is_moving_up", (dir.x == 0) && (dir.y > 0));
        animator.SetBool("P2_is_moving_down", (dir.x == 0) && (dir.y < 0));

        //set direction in which the player is facing
        if (dir.x < 0)
            spriteRenderer.flipX = true;

        else if (dir.x > 0)
            spriteRenderer.flipX = false;


        if (!can_move)
            dir = Vector2.zero;
        GetComponent<Rigidbody2D>().velocity = speed * dir;
    }

    public void lockMovement()
    {
        can_move = false;
    }
    public void unlockMovement()
    {
        can_move = true;
        swordAttack.StopAttack();
        animator.SetTrigger("sword_attack_reset");
    }

    public void SwordAttack_h()
    {

        lockMovement();
        if (spriteRenderer.flipX)
            swordAttack.AttackLeft();
        else
            swordAttack.AttackRight();

    }
    public void SwordAttack_up()
    {

        lockMovement();
        swordAttack.AttackUp();
    }
    public void SwordAttack_down()
    {
        lockMovement();
        swordAttack.AttackDown();
    }

    public void applyDamage(float enemyDamage)
    {
        if (health <= 0)
        {
            return;
        }

        health -= enemyDamage;
        Debug.Log("Player got hit with: " + enemyDamage + ", health is: " + health);

        if (health <= 0)
        {
            killPlayer();
        }
    }

    private void stopPlayer()
    {
        GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
        lockMovement();
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