using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
public class TimerPickup : Interactables
{
    [SerializeField] int TimeAmount = 20;
    
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    protected override void Collect()
    {
        PlayerHUD.Instance.GainTime(TimeAmount);
        Destroy();
    }

    void OnTriggerEnter2D(Collider2D collider2d)
    {
        if (collider2d != null && collider2d.GetComponent<PlayerManager>() != null)
        {
            Debug.Log("Gained Time!");
            Collect();
        }
    }
}//EndScript