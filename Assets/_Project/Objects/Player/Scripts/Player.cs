using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Player : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] public float Speed = 1f;
    [SerializeField] public float Health = 100;
    [SerializeField] public float Stamina = 100;

    [Header("Attachments")]
    [SerializeField] public SwordAttack SwordAttack;
    [SerializeField] public Light2D LightSpotDirectional;

    [Header("Light")]
    [SerializeField] public float playerDirectionalCameraPositionXDefault = 0f;
    [SerializeField] public float playerDirectionalCameraPositionYDefault = 0f;

    [SerializeField] public float playerDirectionalCameraPositionDistance = 0.09f;

    private Animator animator;
    private SpriteRenderer sprite_renderer;
    private Rigidbody2D rigidBody;

    private bool can_move = true;

    private KeyCode KeyMoveRight = KeyCode.D;
    private KeyCode KeyMoveUp = KeyCode.W;
    private KeyCode KeyMoveLeft = KeyCode.A;
    private KeyCode KeyMoveDown = KeyCode.S;

    private KeyCode KeySwordAttack = KeyCode.Space;

    private Vector2 playerMovement;
    private Vector2 playerMovementLast;

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

        playerMovement = PlayerMovement();

        PlayerMove();
        PlayerLight();
        PlayerAttack();
    }




    private void PlayerMove()
    {
        if (playerMovement.x != 0 || playerMovement.y != 0)
        {
            playerMovementLast = playerMovement;
        }

        // set movement animation direction
        animator.SetBool("P2_is_moving_h", playerMovement.x != 0);
        animator.SetBool("P2_is_moving_up", (playerMovement.x == 0) && (playerMovement.y > 0));
        animator.SetBool("P2_is_moving_down", (playerMovement.x == 0) && (playerMovement.y < 0));

        // set direction in which the player is facing
        if (playerMovement.x != 0)
            sprite_renderer.flipX = playerMovement.x < 0;

        rigidBody.velocity = Speed * playerMovement;
    }


    private Vector2 PlayerMovement()
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


        return dir;
    }

    private bool IsPlayerMoving(PLAYER_DIRECTIONS direction, bool useLastSaved)
    {
        Vector2 movement;
        if(useLastSaved)
        {
            movement = playerMovementLast;
        } else
        {
            movement = playerMovement;
        }

        switch(direction)
        {
            case PLAYER_DIRECTIONS.RIGHT:
                return movement.x > 0;
            case PLAYER_DIRECTIONS.LEFT:
                return movement.x < 0;
            case PLAYER_DIRECTIONS.UP:
                return movement.y > 0;
            case PLAYER_DIRECTIONS.DOWN:
                return movement.y < 0;
            case PLAYER_DIRECTIONS.UP_RIGHT:
                return IsPlayerMoving(PLAYER_DIRECTIONS.UP, useLastSaved) && IsPlayerMoving(PLAYER_DIRECTIONS.RIGHT, useLastSaved);
            case PLAYER_DIRECTIONS.UP_LEFT:
                return IsPlayerMoving(PLAYER_DIRECTIONS.UP, useLastSaved) && IsPlayerMoving(PLAYER_DIRECTIONS.LEFT, useLastSaved);
            case PLAYER_DIRECTIONS.DOWN_RIGHT:
                return IsPlayerMoving(PLAYER_DIRECTIONS.DOWN, useLastSaved) && IsPlayerMoving(PLAYER_DIRECTIONS.RIGHT, useLastSaved);
            case PLAYER_DIRECTIONS.DOWN_LEFT:
                return IsPlayerMoving(PLAYER_DIRECTIONS.DOWN, useLastSaved) && IsPlayerMoving(PLAYER_DIRECTIONS.LEFT, useLastSaved);
        }

        return false;
    }

    private float PlayerMovingAngle(bool useLastSaved)
    {
        if (IsPlayerMoving(PLAYER_DIRECTIONS.UP_RIGHT, useLastSaved)) return -45;
        if (IsPlayerMoving(PLAYER_DIRECTIONS.DOWN_RIGHT, useLastSaved)) return -135;
        if (IsPlayerMoving(PLAYER_DIRECTIONS.DOWN_LEFT, useLastSaved)) return -225;
        if (IsPlayerMoving(PLAYER_DIRECTIONS.UP_LEFT, useLastSaved)) return -315;
        if (IsPlayerMoving(PLAYER_DIRECTIONS.RIGHT, useLastSaved)) return -90;
        if (IsPlayerMoving(PLAYER_DIRECTIONS.UP, useLastSaved)) return 0;
        if (IsPlayerMoving(PLAYER_DIRECTIONS.LEFT, useLastSaved)) return -270;
        if (IsPlayerMoving(PLAYER_DIRECTIONS.DOWN, useLastSaved)) return -180;

        return 0;
    }

    private void PlayerAttack()
    {
        if (can_move && Input.GetKey(KeySwordAttack))
        {
            LockMovement();
            animator.SetTrigger("sword_attack");
        }
    }

    private void PlayerLight()
    {

        // -90 => right, -180 => bottom, -270 => left, 0 => up
        float player_direction_z = PlayerMovingAngle(true);
        float player_position_x = playerDirectionalCameraPositionXDefault;
        float player_position_y = playerDirectionalCameraPositionYDefault;

        if (IsPlayerMoving(PLAYER_DIRECTIONS.RIGHT, true))
        {
            player_position_x = -playerDirectionalCameraPositionDistance + playerDirectionalCameraPositionXDefault;
        }
        
        if (IsPlayerMoving(PLAYER_DIRECTIONS.LEFT, true))
        {
            player_position_x = playerDirectionalCameraPositionDistance + playerDirectionalCameraPositionXDefault;
        }

        if (IsPlayerMoving(PLAYER_DIRECTIONS.UP, true))
        {
            player_position_y = -playerDirectionalCameraPositionDistance + playerDirectionalCameraPositionYDefault;
        }

        if (IsPlayerMoving(PLAYER_DIRECTIONS.DOWN, true))
        {
            player_position_y = playerDirectionalCameraPositionDistance + playerDirectionalCameraPositionYDefault;
        }

        LightSpotDirectional.transform.localRotation = Quaternion.Euler(LightSpotDirectional.transform.rotation.x, LightSpotDirectional.transform.rotation.y, player_direction_z);

        LightSpotDirectional.transform.localPosition = new Vector3(player_position_x, player_position_y, transform.localPosition.z);
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

    enum PLAYER_DIRECTIONS
    {
        UP,
        DOWN,
        RIGHT,
        LEFT,
        UP_RIGHT,
        UP_LEFT,
        DOWN_RIGHT,
        DOWN_LEFT
    }
}