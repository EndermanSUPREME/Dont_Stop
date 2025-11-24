using UnityEngine;

public abstract class Interactables : MonoBehaviour
{
    protected Animator anim;
    
    protected abstract void Collect();
    protected void Destroy()
    {
        Destroy(gameObject);
    }
}