using Utilities;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public abstract class Ability : ScriptableObject
{
    // shared variables between derived classes
    [SerializeField] protected int cost;
    [SerializeField] protected float delay;
    [SerializeField] protected Sprite icon;
    [SerializeField] protected string name;
    [SerializeField] Color baseColor;

    // shared method between derived classes
    protected bool EnoughAura()
    {
        int currentAura = PlayerManager.Instance.GetAuraAmount();
        return currentAura >= cost;
    }

    public Color GetBaseColor() => baseColor;
    public Sprite GetIconSprite() => icon;
    public string GetName() => name;

    // derived classes must implement this function
    public abstract void Perform();
}

[CreateAssetMenu(menuName = "Abilities/Healing")]
public class Healing : Ability
{
    [SerializeField] [Range(0f,1f)] float healAmount = 0.35f;

    public override void Perform()
    {
        if (EnoughAura())
        {
            PlayerManager.Instance.ConsumeAura(cost);
            new RunAfter(delay, GiveHealth);
        }
    }
    void GiveHealth()
    {
        PlayerManager.Instance.GainHealth(healAmount);
    }
}

[CreateAssetMenu(menuName = "Abilities/Blaze")]
public class Blaze : Ability
{
    [SerializeField] float duration = 2f;

    public override void Perform()
    {
        if (EnoughAura())
        {
            PlayerManager.Instance.ConsumeAura(cost);
            new RunAfter(delay, BlazeBlast);
        }
    }
    Collider2D[] FindTargets()
    {
        Vector3 playerPosition = PlayerManager.Instance.transform.position;

        Collider2D[] targets = Physics2D.OverlapCircleAll(
            playerPosition,
            PlayerManager.Instance.targetRange,
            PlayerManager.Instance.enemyLayer
        );

        return targets;
    }
    void BlazeBlast()
    {
        Collider2D[] targets = FindTargets();
        foreach (Collider2D target in targets)
        {
            IEnemy enemy = target.GetComponent<IEnemy>();
            if (enemy != null)
            {
                if (!enemy.IsDead())
                    enemy.Ignite();
            }
        }
    }
}

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class AuraAbilities : MonoBehaviour
{
    [SerializeField] Ability[] abilities;
    Animator abilityVfx;
    SpriteRenderer vfxRenderer;

    [SerializeField] Image iconImage;
    [SerializeField] int selected = 0;

    void Start()
    {
        abilityVfx = GetComponent<Animator>();
        vfxRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (abilities.Length == 0) return;
        if (abilityVfx == null) return;

        // new unity input system usage
        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            if (gamepad.dpad.left.wasPressedThisFrame)
            {
                Debug.Log("Pressed L-DPAD");
                selected = (selected - 1 + abilities.Length) % abilities.Length;
            } else if (gamepad.dpad.right.wasPressedThisFrame)
                {
                    Debug.Log("Pressed R-DPAD");
                    selected = ++selected % abilities.Length;
                }
        }

        UpdateIcon();

        if (PlayerInput.PressedSkillButton())
        {
            Ability activeAbility = abilities[selected];
            
            UpdateColor(activeAbility);
            abilityVfx.SetFloat("effectType", selected);
            
            if (activeAbility != null)
            {
                abilityVfx.Play("activate");
                activeAbility.Perform();
            }
        }
    }

    void UpdateIcon()
    {
        Ability activeAbility = abilities[selected];
        
        // Debug.Log($"Active Ability -> {activeAbility.GetName()}");

        if (iconImage != null && activeAbility != null)
            iconImage.sprite = activeAbility.GetIconSprite();
    }

    void UpdateColor(Ability ability)
    {
        if (ability != null && vfxRenderer != null)
        {
            vfxRenderer.color = ability.GetBaseColor();
        }
    }
}//EndScript