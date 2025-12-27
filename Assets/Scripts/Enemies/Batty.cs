using System.Threading.Tasks;

using Utilities;

using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]

public class Batty : MonoBehaviour, IEnemy
{
    Rigidbody2D rb2d;
    Animator anim;
    SpriteRenderer spriteRenderer;
    bool isDead = false;
    bool alerted = false;
    bool attacking = false;

    bool lookingLeft = false;

    [SerializeField] Vector2 hurtBoxSize;
    [SerializeField] Transform leftHitCenter, rightHitCenter;
    [SerializeField] int health = 50, damage = 10, auraGain = 5, pointCost = 10;

    [SerializeField] float moveSpeed = 4f, eyeSight = 4f, stoppingDistance = 1f;
    [SerializeField] float attackChance = 0.3f;

    [SerializeField] LayerMask playerLayer;
    [SerializeField] AnimationClip attackClip;
    [SerializeField] Animator effectAnim;

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

        SetLookDir();
    }

    void Update()
    {
        if (IsDead())
        {
            rb2d.linearVelocity = Vector2.zero;
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

    RunAfter wakeUpEvt = null;
    void ReactToPlayer()
    {
        if (alerted || wakeUpEvt != null) return;
        
        Collider2D collider2d = Physics2D.OverlapCircle(
            transform.position,
            eyeSight,
            playerLayer
        );

        if (collider2d != null)
        {
            anim.Play("wake_up");
            wakeUpEvt = new RunAfter(anim.GetCurrentAnimatorClipInfo(0)[0].clip.length, WakeUp);
        }
    }

    void WakeUp()
    {
        // detected player
        alerted = true;
    }

    RunAfter jumpCoolDown;
    void Move()
    {
        if (rb2d.linearVelocityX < 0) lookingLeft = true;
        else if (rb2d.linearVelocityX > 0) lookingLeft = false;

        if (alerted)
        {
            bool moveRight = transform.position.x < playerPos.x;

            // move towards the player (ignore platforms)
            Vector2 moveDir = playerPos - new Vector2(transform.position.x, transform.position.y);
            Vector2 desiredVel = moveDir.normalized * moveSpeed;

            if (Vector2.Distance(transform.position, playerPos) > stoppingDistance)
                rb2d.linearVelocity = Vector2.Lerp(rb2d.linearVelocity, desiredVel, 4f * Time.deltaTime);
            else
                rb2d.linearVelocity = Vector2.zero;

            anim.SetFloat("speed", Mathf.Clamp01(Mathf.Abs(rb2d.linearVelocityX)));
            
            spriteRenderer.flipX = moveRight;

            // when close to the player randomly decide to attack
            Attack();
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
                new RunAfter(attackClip.length * 0.85f, AttackFinished);
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

    bool effectActive = false;
    RunAfter effectAfter;
    public void Ignite(float duration)
    {
        if (isDead) return;
        if (effectActive) return;
        if (effectAnim != null) effectAnim.Play("ignite");

        effectActive = true;
        _ = DamageOverTime(1000);
        effectAfter = new RunAfter(duration, EndEffect);
    }
    async Task DamageOverTime(int delay)
    {
        while (effectActive)
        {
            if (isDead) return;
            TakeDamage(3, false);
            await Task.Delay(delay);
        }
    }
    void EndEffect()
    {
        effectActive = false;
        if (effectAnim != null) effectAnim.Play("no_effect");
    }

    public void TakeDamage(int amount, bool getAura)
    {
        if (isDead) return;
        PlayerManager.Instance.GainAura(auraGain);
        
        health -= amount;
        if (!alerted)
        {
            anim.Play("wake_up");
            wakeUpEvt = new RunAfter(anim.GetCurrentAnimatorClipInfo(0)[0].clip.length, WakeUp);
        }
        
        if (health <= 0)
        {
            PlayerManager.Instance.GainPoints(pointCost);

            if (effectAfter != null) effectAfter.Stop();
            EndEffect();

            PlayerHUD.Instance.GainTime(10);

            health = 0;
            anim.Play("death");
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
        if (leftHitCenter == null || rightHitCenter == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(leftHitCenter.position, hurtBoxSize);
        Gizmos.DrawWireCube(rightHitCenter.position, hurtBoxSize);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, eyeSight);
    }
}//EndScript