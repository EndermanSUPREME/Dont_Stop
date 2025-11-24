using System.Threading.Tasks;

using Utilities;

using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Mushroom : MonoBehaviour, IEnemy
{
    Rigidbody2D rb2d;
    Animator anim;
    SpriteRenderer spriteRenderer;
    bool isDead = false;
    bool alerted = false;
    bool attacking = false;
    bool jumped = false;
    bool useRunVel = true;

    bool lookingLeft = false;

    [SerializeField] Vector2 groundCheckSize, hurtBoxSize;
    [SerializeField] Transform foot, leftHitCenter, rightHitCenter;
    int maxHealth = 50;
    [SerializeField] int health = 50, damage = 10, auraGain = 5;

    [SerializeField] float moveSpeed = 4f, gravityMultiplier = 1.5f, eyeSight = 4f, jumpForce = 4f, stoppingDistance = 1f;
    [SerializeField] float jumpChance = 0.3f, attackChance = 0.3f;

    [SerializeField] LayerMask playerLayer;
    [SerializeField] AnimationClip attackClip;
    [SerializeField] Animator resurrectedVfx, effectAnim;

    Vector2 vel;
    Vector2 playerPos;
    Vector2 lookDir;

    void Start()
    {
        if (PlayerManager.Instance == null)
        {
            Debug.LogError("Cannot locate Player Manager!");
            return;
        }

        rb2d = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        maxHealth = health;

        SetLookDir();
    }

    void Update()
    {
        if (IsDead())
        {
            rb2d.linearVelocityX = 0;
            return;
        }

        if (PlayerManager.Instance != null)
        {
            playerPos = new Vector2(
                PlayerManager.Instance.transform.position.x,
                PlayerManager.Instance.transform.position.y
            );
        }

        ReactToPlayer();
        Move();
        Attack();
    }

    void ReactToPlayer()
    {
        if (alerted) return;
        Vector2 eyePosition = transform.position;

        // enemies cannot see the player if theyre too high above them
        // and they cannot see below them
        float heightDiff = PlayerManager.Instance.transform.position.y - transform.position.y;
        eyePosition.y = (heightDiff > 0f && heightDiff < 2f) ? PlayerManager.Instance.transform.position.y : transform.position.y;

        RaycastHit2D hit2d = Physics2D.Raycast(eyePosition, lookDir, eyeSight, playerLayer);
        if (hit2d.collider != null)
        {
            // detected player
            alerted = true;
        }
    }

    bool isGrounded()
    {
        var groundCollider = Physics2D.OverlapBox(
            foot.position,
            groundCheckSize,
            0,
            PlayerManager.Instance.groundMask
        );
        return groundCollider != null;
    }

    RunAfter jumpCoolDown;
    void Move()
    {
        anim.SetBool("grounded", isGrounded());

        if (rb2d.linearVelocityX < 0) lookingLeft = true;
        else if (rb2d.linearVelocityX > 0) lookingLeft = false;

        if (isGrounded())
        {
            // initially stand still
            // when player is detected or an attack only injures chase the player
            if (alerted)
            {
                bool moveRight = transform.position.x < playerPos.x;

                if (!useRunVel)
                {
                    useRunVel = true;
                    anim.Play("landing");
                    jumpCoolDown = new RunAfter(2f, ResetJump);
                }
                
                if (useRunVel)
                {
                    float dist2Player = Vector2.Distance(new Vector2(transform.position.x, playerPos.y), playerPos);
                    if (dist2Player > stoppingDistance && !attacking) rb2d.linearVelocityX = (moveRight) ? moveSpeed : -moveSpeed;
                    else rb2d.linearVelocityX = 0;
                }

                anim.SetFloat("speed", Mathf.Clamp01(Mathf.Abs(rb2d.linearVelocityX)));
                
                spriteRenderer.flipX = moveRight;

                // randomly decide to jump
                Jump();

                // when close to the player randomly decide to attack
                Attack();
            }

            // apply const force incase enemy runs off a ledge/edge
            if (rb2d.linearVelocityY < 0)
            {
                rb2d.linearVelocityY = -2;
            }
        } else
            {
                // apply falling velocity
                rb2d.linearVelocityY += -10f * gravityMultiplier * Time.deltaTime;
            }
    }

    void ResetJump() { jumped = false; }
    void Jump()
    {
        if (jumped) return;

        Vector2 a = new Vector2(0, playerPos.y);
        Vector2 b = new Vector2(0, transform.position.y);

        float heightDiff = Vector2.Distance(a, b);

        bool playerInAir = heightDiff > 2f && !PlayerManager.Instance.grounded;
        bool playerOnHigherGround = heightDiff > 2f && PlayerManager.Instance.grounded;
        
        if (playerInAir || playerOnHigherGround)
        {
            float randValue = Random.Range(0f, 1f);
            if (randValue < jumpChance)
            {
                anim.Play("jump");
                jumped = true;
                useRunVel = false;

                // give jump random direction
                float velX = rb2d.linearVelocityX;
                if (velX < 0) velX -= Random.Range(0, 4);
                else if (velX > 0) velX += Random.Range(0, 4);
                else velX += Random.Range(-4, 4);

                rb2d.linearVelocityX = velX;
                rb2d.linearVelocityY = jumpForce + Random.Range(0, 4);
            }
        }
    }

    Sleep attackDelay = null;
    void AttackFinished()
    {
        attacking = false;
        attackDelay = new Sleep(3f);
    }

    void Attack()
    {
        if (spriteRenderer == null || !alerted) return;
        if (attacking) return;

        float dist2Player = Vector2.Distance(transform.position, playerPos);
        if (dist2Player < stoppingDistance + 0.15f)
        {
            float randValue = Random.Range(0f, 1f);
            if ((attackDelay == null || attackDelay.Finished()) && randValue < attackChance)
            {
                attacking = true;
                anim.Play("attack");
                RunAfter evt = new RunAfter(attackClip.length * 0.85f, AttackFinished);
            }
        }
    }
    public void CheckHurtBox() // Animation Event
    {
        Vector2 center = (!lookingLeft) ? (Vector2)rightHitCenter.position : (Vector2)leftHitCenter.position;

        var hitObject = Physics2D.OverlapBox(
            center,
            hurtBoxSize,
            0,
            playerLayer
        );

        if (hitObject != null && hitObject.transform != null &&
            hitObject.transform.GetComponent<PlayerManager>() != null)
        {
            PlayerManager.Instance.TakeDamage(damage);
        }
    }

    void Resurrect()
    {
        PlayerHUD.Instance.AuraFarmDetected();

        anim.Play("revive");
        resurrectedVfx.Play("activate");

        health = maxHealth * 2;
        damage *= 2;
        auraGain = 0;

        new RunAfter(anim.GetCurrentAnimatorClipInfo(0)[0].clip.length * 0.85f, ReviveFinished);
    }
    void ReviveFinished() { isDead = false; }

    bool effectActive = false;
    RunAfter effectAfter;
    public void Ignite()
    {
        if (effectActive) return;
        if (effectAnim != null) effectAnim.Play("ignite");

        effectActive = true;
        _ = DamageOverTime(1000);
        effectAfter = new RunAfter(4f, EndEffect);
    }
    async Task DamageOverTime(int delay)
    {
        while (effectActive)
        {
            TakeDamage(3, false);
            await Task.Delay(delay);
        }
    }
    void EndEffect()
    {
        if (effectAnim != null) effectAnim.Play("no_effect");
    }

    int afterDeathHits = 0;
    public void TakeDamage(int amount, bool getAura)
    {
        PlayerManager.Instance.GainAura(auraGain);

        if (isDead)
        {
            ++afterDeathHits;
            if (afterDeathHits == 5)
            {
                Resurrect();
            }
            return;
        }

        health -= amount;
        alerted = true;
        
        if (health <= 0)
        {
            health = 0;
            anim.Play("death");
            resurrectedVfx.Play("idle");
            afterDeathHits = 0;
            isDead = true;
        } else
            {
                anim.Play("hurt");
            }
    }
    public bool IsDead() => isDead;

    void SetLookDir()
    {
        bool lookRight = Random.Range(0,100) % 3 == 0;
        lookDir = (lookRight) ? transform.right : -transform.right;

        if (lookRight)
        {
            spriteRenderer.flipX = true;
        }
    }

    void OnDrawGizmos()
    {
        if (foot == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(foot.position, groundCheckSize);

        if (leftHitCenter == null || rightHitCenter == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(leftHitCenter.position, hurtBoxSize);
        Gizmos.DrawWireCube(rightHitCenter.position, hurtBoxSize);

        // not always accurate, this is a reference to adjust eyeSight
        if (lookDir != Vector2.zero) Debug.DrawRay(transform.position, lookDir * eyeSight, Color.red);
    }
}//EndScript