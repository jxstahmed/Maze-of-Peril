using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] public PlayerStats PlayerData;
    [SerializeField] public Slider PlayerUIHealth;
    [SerializeField] public Slider PlayerUIStamina;

    [Header("Attachments")]
    [SerializeField] public GameObject Weapon;
    [SerializeField] public WeaponController WeaponController;

    [Header("States")]
    [SerializeField] public bool isWeaponShown = true;
    [SerializeField] public bool isPlayerMoving = false;
    [SerializeField] public bool isPlayerRunning = false;
    [SerializeField] public bool isPlayerAttacking = false;

    [Header("Controlls")]
    [SerializeField] private KeyCode KeyMoveRight = KeyCode.D;
    [SerializeField] private KeyCode KeyMoveUp = KeyCode.W;
    [SerializeField] private KeyCode KeyMoveLeft = KeyCode.A;
    [SerializeField] private KeyCode KeyMoveDown = KeyCode.S;
    [SerializeField] private KeyCode KeyMoveSprint = KeyCode.LeftShift;
    [SerializeField] private KeyCode KeyShowWeapon = KeyCode.F;

    [SerializeField] private KeyCode KeySwordAttack = KeyCode.Space;

    private Animator animator;
    private SpriteRenderer sprite_renderer;
    private Rigidbody2D rigidBody;

    private bool canMove = true;
    private bool hasToggledWeaponKey = false;
    

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

        PlayerData.Stamina = 100f;
        PlayerData.Health = 100f;
    }


    private void FixedUpdate()
    {
        if (!canMove)
            return;


        PlayerDirection();
        PlayerMove();
        PlayerAnimation();
        PlayerWeapon();
        PlayerStatsIncrement();
        PlayerUI();
    }

    private void Update()
    {
        PlayerControlls();
    }

    private void PlayerControlls()
    {
        if (Input.GetKeyDown(KeyShowWeapon))
        {
            hasToggledWeaponKey = true;
            isWeaponShown = !isWeaponShown;
        }


        if (Input.GetKeyDown(KeySwordAttack))
        {
            // The player wants to attack, we check the current ability to hit (stamina check up)
            if(PlayerData.Stamina > 0)
            {
                WeaponStats activeWeapon = GameManager.Instance.GetActiveWeaponProfile();
                if (activeWeapon != null)
                {
                    float availableStamina = PlayerData.Stamina - activeWeapon.StaminaReductionRate;

                    if (availableStamina >= 0)
                    {
                        Debug.Log("Stamina is available, providing an attack.");
                        WeaponController.ActivateAttackAllowance();
                    }
                    else
                    {
                        Debug.Log("Stamina isn't enough.");
                    }
                } else
                {
                    Debug.Log("Can't attack, active weapon isn't found.");
                }
            }
        }
            


        // Movement
        Vector2 dir = Vector2.zero;
        // get WASD Input
        if (Input.GetKey(KeyMoveLeft))
            dir.x = -1;
        if (Input.GetKey(KeyMoveRight))
            dir.x = 1;
        if (Input.GetKey(KeyMoveUp))
            dir.y = 1;
        if (Input.GetKey(KeyMoveDown))
            dir.y = -1;

        isPlayerMoving = playerMovement.x != 0 || playerMovement.y != 0;
        isPlayerRunning = Input.GetKey(KeyMoveSprint) && isPlayerMoving && (PlayerData.Stamina - PlayerData.SpeedStaminaReductionRate) >= 0;

        dir.Normalize();

        playerMovement = dir;
    }


    private void PlayerDirection()
    {
        if (IsPlayerMoving(PLAYER_DIRECTIONS.RIGHT, true))
        {
            sprite_renderer.flipX = false;

            if (Weapon != null && isWeaponShown)
            {
                Weapon.transform.localScale = new Vector3(1, Weapon.transform.localScale.y, Weapon.transform.localScale.z);
            }
        }
        else if (IsPlayerMoving(PLAYER_DIRECTIONS.LEFT, true))
        {
            sprite_renderer.flipX = true;

            if(Weapon != null && isWeaponShown)
            {
                Weapon.transform.localScale = new Vector3(-1, Weapon.transform.localScale.y, Weapon.transform.localScale.z);
            }
        }
    }

    private void PlayerMove()
    {
        if (playerMovement.x != 0 || playerMovement.y != 0)
        {
            playerMovementLast = playerMovement;
        }



        rigidBody.velocity = PlayerMovementMultiplier() * playerMovement;
    }


    private void PlayerAnimation()
    {
        animator.SetBool("isWalking", isPlayerMoving && !isPlayerRunning);
        animator.SetBool("isRunning", isPlayerMoving && isPlayerRunning);
    }

    private void PlayerWeapon()
    {
        if (Weapon == null) return;


        if (hasToggledWeaponKey)
        {
            hasToggledWeaponKey = false;
            Weapon.SetActive(isWeaponShown);
        }
    }



    private float PlayerMovementMultiplier()
    {
        float base_speed = PlayerData.Speed;

        if (PlayerData.WalkFactor > 0)
        {
            // Factor to increase the walk
            base_speed += (base_speed * PlayerData.WalkFactor);
        }

        if (isPlayerRunning)
        {
            // Factor to increase the run
            base_speed += (base_speed * PlayerData.RunFactor);
            Debug.Log("Player is running, reducing: -" + PlayerData.SpeedStaminaReductionRate);
            AffectStamina(-PlayerData.SpeedStaminaReductionRate);

        }

        return base_speed;
    }

    public bool IsPlayerMoving(PLAYER_DIRECTIONS direction, bool useLastSaved)
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

    public float PlayerMovingAngle(bool useLastSaved)
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

    private void PlayerStatsIncrement()
    {
        AffectHealth(PlayerData.HealthRegenerationRate);
        AffectStamina(PlayerData.StaminaRegenerationRate);
    }

    private void PlayerUI()
    {
        PlayerUIHealth.value = PlayerData.Health / 100;
        PlayerUIStamina.value = PlayerData.Stamina / 100;
    }


    public void LockMovement()
    {
        rigidBody.velocity = new Vector2(0, 0);
        canMove = false;
    }
    public void UnlockMovement()
    {
        canMove = true;
    }


    public void applyDamage(float enemyDamage)
    {
        if (PlayerData.Health <= 0)
        {
            return;
        }

        PlayerData.Health -= enemyDamage;
        Debug.Log("Player got hit with: " + enemyDamage + ", health is: " + PlayerData.Health);

        if (PlayerData.Health <= 0)
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

    public void AffectHealth(float health)
    {
        float newHealth = PlayerData.Health + health;

        if (newHealth > 100) newHealth = 100;
        else if (newHealth < 0) newHealth = 0;
        
        PlayerData.Health = newHealth;
    }

    public void AffectStamina(float stamina)
    {
        float newstamina = PlayerData.Stamina + stamina;

        if (newstamina > 100) newstamina = 100;
        else if (newstamina < 0) newstamina = 0;

        PlayerData.Stamina = newstamina;
    }

    private void onGameEventListen(Hashtable payload)
    {
        if ((GameState)payload["state"] == GameState.AttackPlayer)
        {
            applyDamage((float)payload["damage"]);
        } else if ((GameState)payload["state"] == GameState.AffectStamina)
        {
            Debug.Log("received stamina reduction request in player, reduction is: " + (float)payload["stamina"]);
            AffectStamina((float)payload["stamina"]);
        } else if ((GameState)payload["state"] == GameState.AffectHealth)
        {
            AffectHealth((float)payload["health"]);
        }
    }

    public enum PLAYER_DIRECTIONS
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