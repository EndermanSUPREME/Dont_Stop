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

    [Header("Player Stats")]
    public float moveSpeed = 4, jumpForce = 3, gravityMultiplier = 2, launchPadMultiplier = 2.5f;
    public int attackDamage = 10;
    int maxHealth = 100;
    [SerializeField] int health = 100;

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

    public void TakeDamage(int amount)
    {
        if (iframeActive) return;
        
        health -= amount;
        if (health <= 0)
        {
            health = 0;
            isDead = true;
            if (playerAnimator != null)
                playerAnimator.Play("death");
        } else
            {
                playerAnimator.Play("hurt");
            }
    }

    [Header("Player States")]
    public bool grounded;
    public bool huggingWall;
    public bool gamePaused;
    public bool attacking = false;
    public bool falling = false;
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