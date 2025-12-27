using Utilities;

using UnityEngine;
using System.Threading.Tasks;

// universal Singleton template object
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static bool quittingApp = false;

    public static T Instance
    {
        get // getter C# property
        {
            if (quittingApp) return null;
            if (instance != null) return instance;

            // Try to find one in the scene first
            instance = FindFirstObjectByType<T>();

            if (instance == null)
            {
                GameObject singletonObject = new GameObject(typeof(T).Name);
                instance = singletonObject.AddComponent<T>();
            }

            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            // create the new instance
            instance = this as T;
        } else if (instance != this)
            {
                // remove other instances
                Destroy(gameObject);
            }
    }

    // when the application quits perform the following
    void OnApplicationQuit()
    {
        quittingApp = true;
    }
}

public class PlayerManager : Singleton<PlayerManager>
{
    [Header("Player Components")]
    [SerializeField] Animator playerAnimator;
    [SerializeField] PlayerCamera playerCamera;

    [Header("Player Stats")]
    public float moveSpeed = 4, jumpForce = 3, gravityMultiplier = 2, launchPadMultiplier = 2.5f;
    public int attackDamage = 10;
    public float targetRange = 3f;
    
    int maxHealth = 100;
    int maxAura = 100;

    int totalPoints = 0;
    int points = 0;
    int pointRequirement = 100;
    int level = 1;

    [SerializeField] int health = 100, aura = 0;

    RunAfter evt;
    public void ToggleIFrame()
    {
        iframeActive = true;
        evt = new RunAfter(1f, DisableIFrame);
    }
    void DisableIFrame()
    {
        iframeActive = false;
    }

    public float GetHealthPercent() => Mathf.Clamp01((float)health / (float)maxHealth);
    public float GetAuraPercent() => Mathf.Clamp01((float)aura / (float)maxAura);
    // get percent in decimal form of progress from 0-1
    public float GetLevelProgress() => Mathf.Clamp01((float)points / (float)pointRequirement);
    public int GetAuraAmount() => aura;
    // get level number value used in displaying to the screen
    public int GetLevel() => level;

    public void GainHealth(float amount)
    {
        if (isDead) return;

        health += (int)((float)maxHealth * amount);
        if (health >= maxHealth) health = maxHealth;
    }
    public void GainAura(int amount)
    {
        aura += amount;
        if (aura >= maxAura) aura = maxAura;
    }
    public void GainPoints(int amount)
    {
        totalPoints += amount;
        points += amount;

        if (points >= pointRequirement)
        {
            ++level;
            points = 0;
            pointRequirement = (int)Mathf.Ceil((float)pointRequirement * 1.5f);
            PlayerHUD.Instance.UpdateLevelDisplay();
        }
    }

    public void TakeDamage(int amount)
    {
        if (iframeActive) return;
        
        health -= amount;
        if (health <= 0)
        {
            health = 0;
            isDead = true;
            Death();
        } else
            {
                playerAnimator.Play("hurt");
            }
    }
    public void InstantKill()
    {
        health = 0;
        isDead = true;
        Death();
    }
    void Death()
    {
        if (playerAnimator != null)
                playerAnimator.Play("death");
                
        if (playerCamera != null)
                playerCamera.ShowDeathScreen();
    }

    public void ConsumeAura(int amount)
    {
        aura -= amount;
        if (aura < 0) aura = 0;
    }

    public void SetBossViewPoint(Vector3 pos)
    {
        if (playerCamera) playerCamera.SetBossViewPoint(pos);
    }
    public void DetachCamera()
    {
        if (playerCamera) playerCamera.Detach();
    }
    public void AttachCamera()
    {
        if (playerCamera) playerCamera.Attach();
    }

    [Header("Player States")]
    public bool grounded;
    public bool huggingWall;
    public bool gamePaused;
    public bool attacking = false;
    public bool falling = false;
    public bool inBossFight = false;
    bool isDead = false;
    [SerializeField] bool iframeActive = false;

    public bool IsDead() => isDead;

    [Header("Raycast Layers")]
    public LayerMask groundMask;
    public LayerMask wallLayer;
    public LayerMask obstacleLayer;
    public LayerMask enemyLayer;

    void Start()
    {
        maxHealth = health;
    }
}