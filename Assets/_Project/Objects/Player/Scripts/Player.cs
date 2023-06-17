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
    [SerializeField] public GameObject WeaponObject;
    [SerializeField] public WeaponController WeaponController;
    [SerializeField] public ParticleSystem MovingDust;

    [Header("States")]
    [SerializeField] public bool IsDead = false;
    [SerializeField] public bool IsWeaponShown = true;
    [SerializeField] public bool isPlayerMoving = false;
    [SerializeField] public bool isPlayerRunning = false;
    [SerializeField] public bool isPlayerAttacking = false;
    [SerializeField] public bool isPlayerBeingAttacked = false;
    [SerializeField] public float lastAttackedTime = 0;

    [Header("Controlls")]
    [SerializeField] private KeyCode KeyMoveRight = KeyCode.D;
    [SerializeField] private KeyCode KeyMoveUp = KeyCode.W;
    [SerializeField] private KeyCode KeyMoveLeft = KeyCode.A;
    [SerializeField] private KeyCode KeyMoveDown = KeyCode.S;
    [SerializeField] private KeyCode KeyMoveSprint = KeyCode.LeftShift;
    [SerializeField] private KeyCode KeyShowWeapon = KeyCode.F;
    [SerializeField] private KeyCode KeyToggleWeapon = KeyCode.R;

    [SerializeField] private KeyCode KeySwordAttack = KeyCode.Space;

    private Animator animator;
    private SpriteRenderer sprite_renderer;
    private Rigidbody2D rigidBody;
    private float internalIncrementTimer = 0f;
    private float internalStaminaCooldownTimer = 0f;
    private float internalHealthCooldowTimer = 0f;

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

        PlayerData.Health = PlayerData.OverallHealth;
        PlayerData.Stamina = PlayerData.OverallStamina;

        ApplyEquippedWeapon();
    }


    private void FixedUpdate()
    {
        PlayerUI();

        if (!IsDead && PlayerData.Health <= 0)
        {
            KillPlayer();
        }


        if (!canMove || IsDead)
            return;


        PlayerDirection();
        PlayerMove();
        PlayerAnimation();
        PlayerWeapon();

        if (isPlayerRunning)
        {
            CreateDust();
        }
    }



    private void Update()
    {
        if (!canMove || IsDead) return;


        internalIncrementTimer += Time.deltaTime;
        internalStaminaCooldownTimer += Time.deltaTime;
        internalHealthCooldowTimer += Time.deltaTime;
        if (isPlayerBeingAttacked) internalHealthCooldowTimer = 0;
        if (isPlayerRunning) internalStaminaCooldownTimer = 0;



        PlayerControlls();

        if (internalIncrementTimer >= PlayerData.IncrementEverySeconds)
        {
            internalIncrementTimer = 0;
            PlayerStatsIncrement();
        }
    }

    private void PlayerControlls()
    {
        if (Input.GetKeyDown(KeyShowWeapon))
        {
            ToggleWeapon();
        }

        if (Input.GetKeyDown(KeyToggleWeapon))
        {
            ToggleWeaponSelection();
        }



        if (Input.GetKeyDown(KeySwordAttack))
        {
            // The player wants to attack, we check the current ability to hit (stamina check up)
            if (PlayerData.Stamina > 0)
            {
                WeaponStats activeWeapon = GetActiveWeaponProfile();
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
        //ist der spieler nach links oder rechts gedreht
        if (playerMovement.x < 0)
            sprite_renderer.flipX = true;
        else if (playerMovement.x > 0)
            sprite_renderer.flipX = false;


        //gesamte Is PlayerMoving funktion ist überkompliziert und kann durch die oberen code erstzt werden
       /* if (IsPlayerMoving(PLAYER_DIRECTIONS.RIGHT, true))
        {
            sprite_renderer.flipX = false;
        }
        else if (IsPlayerMoving(PLAYER_DIRECTIONS.LEFT, true))
        {
            sprite_renderer.flipX = true;
        }*/

        if (WeaponObject != null && IsWeaponShown)
        {
            WeaponObject.transform.localScale = new Vector3(sprite_renderer.flipX ? -1 : 1, WeaponObject.transform.localScale.y, WeaponObject.transform.localScale.z);
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
        if (WeaponObject == null) return;


        if (hasToggledWeaponKey)
        {
            hasToggledWeaponKey = false;
            WeaponObject.SetActive(IsWeaponShown);
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

        

        if (internalStaminaCooldownTimer > PlayerData.RegenerateStaminaCooldownWhenRun)
        {
            AffectStamina(PlayerData.StaminaRegenerationRate);
        }

        if (internalHealthCooldowTimer > PlayerData.RegenerateHealthCooldownWhenHit)
        {
            AffectHealth(PlayerData.HealthRegenerationRate);
        }


    }

    private void ToggleWeapon()
    {
        hasToggledWeaponKey = true;
        IsWeaponShown = !IsWeaponShown;
    }

    public void HideWeapon()
    {
        hasToggledWeaponKey = true;
        IsWeaponShown = false;
    }

    private void PlayerUI()
    {
        PlayerUIHealth.value = PlayerData.Health > 0 ? PlayerData.Health / PlayerData.OverallHealth : 0;
        PlayerUIStamina.value = PlayerData.Stamina > 0 ? PlayerData.Stamina / PlayerData.OverallStamina : 0;
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


    public void ApplyDamage(float enemyDamage)
    {
        Debug.Log("Damage is: " + enemyDamage);
        AffectHealth(enemyDamage);

        if (PlayerData.Health <= 0)
        {
            KillPlayer();
        }
    }

    private void StopPlayer()
    {
        LockMovement();
    }

    private void KillPlayer()
    {
        hasToggledWeaponKey = true;
        IsWeaponShown = false;
        IsDead = true;
        StopPlayer();
        GameManager.Instance.StopEnemies(true);
        animator.SetBool("isDead", true);
    }

    public void AffectHealth(float health)
    {
        float newHealth = PlayerData.Health + health;

        Debug.Log("New health is: " + newHealth);

        if (newHealth > PlayerData.OverallHealth) newHealth = PlayerData.OverallHealth;
        else if (newHealth < 0) newHealth = 0;
        
        PlayerData.Health = newHealth;
    }

    public void AffectStamina(float stamina)
    {
        float newstamina = PlayerData.Stamina + stamina;

        if (newstamina > PlayerData.OverallStamina) newstamina = PlayerData.OverallStamina;
        else if (newstamina < 0) newstamina = 0;

        PlayerData.Stamina = newstamina;
    }

    public void PickWeapon(WeaponStats weaponData)
    {

        if (PlayerData.WeaponsIDsList.Contains(weaponData.ID))
        {
            Debug.Log("WeaponSystem: Contains");
            SelectWeapon(WeaponsManager.Instance.FindWeaponIndex(weaponData.ID));
            return;
        };
        Debug.Log("WeaponSystem: Added");
        PlayerData.WeaponsIDsList.Add(weaponData.ID);
        ToggleWeaponSelection();
    }

    public void ApplyEquippedWeapon()
    {
        Debug.Log("WeaponSystem: ApplyEquippedWeapon, ID: " + PlayerData.EquippedWeaponID);
        // ID is empty
        if (PlayerData.EquippedWeaponID == null) return;
        // ID doesn't exist in the list
        if (!PlayerData.WeaponsIDsList.Contains(PlayerData.EquippedWeaponID)) return;
        SelectWeapon(WeaponsManager.Instance.FindWeaponIndex(PlayerData.EquippedWeaponID));

    }



    public void SelectWeapon(int index)
    {
        Debug.Log("WeaponSystem: Index is " + index);
        if (!(index >= 0)) return;

        WeaponStats pr = GameManager.Instance.WeaponsPackData.Swords[index];
        // Select the weapon and initiate it
        ApplySelectedWeapon(pr);
    }

    public void ToggleWeaponSelection()
    {
        if (PlayerData.WeaponsIDsList.Count == 0) return;

        WeaponStats weaponStatsProfile = GameManager.Instance.WeaponsPackData.Swords[WeaponsManager.Instance.FindWeaponIndex(GetNextWeaponProfile())];
        ApplySelectedWeapon(weaponStatsProfile);
    }

    public void ApplySelectedWeapon(WeaponStats weaponStatsProfile)
    {
        PlayerData.EquippedWeaponID = weaponStatsProfile.ID;
        Debug.Log("WeaponSystem: Weapon toggled");

        if(WeaponObject.transform.childCount > 0)
        {
            Destroy(WeaponObject.transform.GetChild(0).gameObject);
        }
        WeaponStatsProfile controller = weaponStatsProfile.prefab.GetComponent<WeaponStatsProfile>();
        controller.isEquipEnabled = false;

        GameObject obj = Instantiate(weaponStatsProfile.prefab, Vector2.zero, weaponStatsProfile.prefab.transform.rotation);
        obj.transform.SetParent(WeaponObject.transform, true);
        obj.transform.localPosition = weaponStatsProfile.Position;
        obj.transform.localScale = new Vector3(1, obj.transform.localScale.y, obj.transform.localScale.z);
        obj.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);

        WeaponController.ApplyNewSettings(weaponStatsProfile);
    }

    public WeaponStats GetActiveWeaponProfile()
    {
        if (PlayerData == null || GameManager.Instance.WeaponsPackData == null || PlayerData.EquippedWeaponID == null || !IsWeaponShown) return null;

        WeaponStats weaponStatsProfile = null;

        GameManager.Instance.WeaponsPackData.Swords.ForEach(weapon =>
        {
            if (weapon.ID == PlayerData.EquippedWeaponID)
            {
                Debug.Log("Weapon has been found");
                weaponStatsProfile = weapon;
                return;
            }
        });

        return weaponStatsProfile;
    }

    public string GetNextWeaponProfile()
    {
        if (PlayerData == null || GameManager.Instance.WeaponsPackData == null) return null;

        string weaponStatsProfile = null;

        int nextWeaponIndex = 0;

        for(int i = 0; i < PlayerData.WeaponsIDsList.Count; i++)
        {
            string pr = PlayerData.WeaponsIDsList[i];
            if(PlayerData.EquippedWeaponID == null)
            {
                nextWeaponIndex = i;
                break;
            } else if(PlayerData.EquippedWeaponID != null && pr == PlayerData.EquippedWeaponID)
            {

                if (PlayerData.WeaponsIDsList.Count == 1) break;

                if((i + 1) >= PlayerData.WeaponsIDsList.Count)
                {
                    nextWeaponIndex = 0;
                }
                else
                {
                    nextWeaponIndex = (i + 1);
                }
                break;
            }
        }

        if(nextWeaponIndex >= 0)
        weaponStatsProfile = PlayerData.WeaponsIDsList[nextWeaponIndex];



        return weaponStatsProfile;
    }

    void CreateDust()
    {
        if(MovingDust != null)
            MovingDust.Play();
    }

    private void onGameEventListen(Hashtable payload)
    {
        Debug.Log("Got event");
        Debug.Log(payload["state"]);
        if ((GameState)payload["state"] == GameState.AttackPlayer)
        {
            ApplyDamage((float)payload["damage"]);
        } else if ((GameState)payload["state"] == GameState.AffectStamina)
        {
            Debug.Log("received stamina reduction request in player, reduction is: " + (float)payload["stamina"]);
            AffectStamina((float)payload["stamina"]);
        } else if ((GameState)payload["state"] == GameState.AffectHealth)
        {
            Debug.Log("Affecting health");
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