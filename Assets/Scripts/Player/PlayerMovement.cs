using Utilities;

using UnityEngine;

public static class PlayerInput // support keyboard and controller
{
    public static bool PressedJump() => Input.GetKeyDown(KeyCode. Space) || Input.GetButtonDown("Jump");
    public static bool HoldingJump() => Input.GetKey(KeyCode. Space) || Input.GetButton("Jump");

    public static bool MovingLeft() => Input.GetKey(KeyCode. A) || Input.GetAxis("Left_Stick_Horizontal") < -0.15f;
    public static bool MovingRight() => Input.GetKey(KeyCode. D) || Input.GetAxis("Left_Stick_Horizontal") > 0.15f;
    
    public static bool PressedAttack() => Input.GetMouseButtonDown(1) || Input.GetButtonDown("Attack");

    // public static bool PressedPrevSkill() => Input.GetKeyDown(KeyCode. Q) || Input.GetButtonDown("LeftBumper");
    // public static bool PressedNextSkill() => Input.GetKeyDown(KeyCode. E) || Input.GetButtonDown("RightBumper");

    public static bool PressedSkillButton() => Input.GetKeyDown(KeyCode. F) || Input.GetButtonDown("SkillUse");
}

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D rb2d;
    Animator anim;
    SpriteRenderer spriteRenderer;

    [SerializeField] Vector2 groundCheckSize, hurtBoxSize;
    [SerializeField] Transform playerFoot, leftHitCenter, rightHitCenter;
    [SerializeField] AnimationClip attackClip;

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
    }

    void Update()
    {
        if (PlayerManager.Instance.IsDead()) return;

        Move();
        Attack();
    }

    bool isGrounded()
    {
        var groundCollider = Physics2D.OverlapBox(
            playerFoot.position,
            groundCheckSize,
            0,
            PlayerManager.Instance.groundMask
        );
        return groundCollider != null;
    }

    void Move()
    {
        float velx = 0;
        if (PlayerInput.MovingRight()) velx = PlayerManager.Instance.moveSpeed;
        else if (PlayerInput.MovingLeft()) velx = -PlayerManager.Instance.moveSpeed;

        if (velx != 0) spriteRenderer.flipX = velx < 0;

        rb2d.linearVelocity = new Vector2(velx, rb2d.linearVelocityY);

        anim.SetFloat("speed", Mathf.Abs(velx));
        
        PlayerManager.Instance.grounded = isGrounded();
        anim.SetBool("grounded", PlayerManager.Instance.grounded);
        anim.SetBool("attacking", PlayerManager.Instance.attacking);

        if (isGrounded())
        {
            if (PlayerManager.Instance.falling)
            {
                PlayerManager.Instance.falling = false;

                if (!PlayerManager.Instance.attacking)
                    anim.Play("landing");
            }

            if (PlayerInput.PressedJump())
            {
                if (!PlayerManager.Instance.attacking)
                    anim.Play("jump");
                    
                float velY = PlayerManager.Instance.jumpForce * PlayerManager.Instance.launchPadMultiplier;
                rb2d.linearVelocity = new Vector2(rb2d.linearVelocityX, velY);
            }
        } else
            {
                if (!PlayerManager.Instance.falling)
                {
                    if (rb2d.linearVelocityY <= 0) PlayerManager.Instance.falling = true;

                    // mid-jump
                    if (!PlayerInput.HoldingJump() && rb2d.linearVelocityY > 0)
                    {
                        rb2d.linearVelocity = new Vector2(rb2d.linearVelocityX, 0);
                        PlayerManager.Instance.falling = true;
                    }
                } else
                    {
                        // falling
                        if (rb2d.linearVelocityY < -24) rb2d.linearVelocityY = -20; // cap player falling velocity
                        else rb2d.linearVelocityY += -10 * PlayerManager.Instance.gravityMultiplier * Time.deltaTime;
                    }
            }
    }

    void AttackFinished()
    {
        PlayerManager.Instance.attacking = false;
    }
    public void HurtNearbyEnemies() // Animation Event
    {
        // Debug.Log("Attack Animation Event Triggered!");
        // based on player sprite original orientation
        Vector2 center = (spriteRenderer.flipX) ? (Vector2)leftHitCenter.position : (Vector2)rightHitCenter.position;

        var hitObjects = Physics2D.OverlapBoxAll(
            center,
            hurtBoxSize,
            0,
            PlayerManager.Instance.enemyLayer
        );

        foreach (var hitObj in hitObjects)
        {
            IEnemy enemy = hitObj.GetComponent<IEnemy>();
            if (enemy != null)
            {
                // Debug.Log($"Landed Attack on -> {hitObj.transform.name}");
                enemy.TakeDamage(PlayerManager.Instance.attackDamage);
            }
        }
    }
    void Attack()
    {
        if (spriteRenderer == null) return;

        if (PlayerInput.PressedAttack() && !PlayerManager.Instance.attacking)
        {
            PlayerManager.Instance.attacking = true;
            anim.Play("attack");
            RunAfter evt = new RunAfter(attackClip.length * 0.85f, AttackFinished);
        }
    }

    void OnDrawGizmos()
    {
        if (playerFoot == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(playerFoot.position, groundCheckSize);

        if (leftHitCenter == null || rightHitCenter == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(leftHitCenter.position, hurtBoxSize);
        Gizmos.DrawWireCube(rightHitCenter.position, hurtBoxSize);

        Gizmos.color = Color.cyan;
        if (PlayerManager.Instance != null)
            Gizmos.DrawWireSphere(transform.position, PlayerManager.Instance.targetRange);
    }
}//EndScript