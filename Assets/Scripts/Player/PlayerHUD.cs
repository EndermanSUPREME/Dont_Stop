using System;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : Singleton<PlayerHUD>
{
    [SerializeField] Image healthBar, auraBar;
    [SerializeField] Gradient healthGradient, auraGradient;
    [SerializeField] Text timerDisplay;
    [SerializeField] float remainingTime = 90f;
    [SerializeField] Animator popup_text;

    void Start()
    {
        _ = UpdateTimeDisplay();
    }

    void Update()
    {
        UpdateHealthBar();
        UpdateAuraBar();
    }

    void UpdateHealthBar()
    {
        float x = PlayerManager.Instance.GetHealthPercent();
        Vector3 nScale = new Vector3(x, 1, 1);

        healthBar.color = Color.Lerp(healthBar.color, healthGradient.Evaluate(x), 10 * Time.deltaTime);

        float dist = Vector3.Distance(healthBar.transform.localScale, nScale);
        
        if (dist > 0.1f)
            healthBar.transform.localScale = Vector3.Lerp(healthBar.transform.localScale, nScale, 10 * Time.deltaTime);
        else
            healthBar.transform.localScale = nScale;
    }

    void UpdateAuraBar()
    {
        float x = PlayerManager.Instance.GetAuraPercent();
        Vector3 nScale = new Vector3(x, 1, 1);

        auraBar.color = Color.Lerp(auraBar.color, auraGradient.Evaluate(x), 10 * Time.deltaTime);

        float dist = Vector3.Distance(auraBar.transform.localScale, nScale);
        
        if (dist > 0.1f)
            auraBar.transform.localScale = Vector3.Lerp(auraBar.transform.localScale, nScale, 10 * Time.deltaTime);
        else
            auraBar.transform.localScale = nScale;
    }

    async Task UpdateTimeDisplay()
    {
        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            ShowRemainingTime();
            await Task.Delay(1000);
        }
        remainingTime = 0;
    }
    void ShowRemainingTime()
    {
        int totalSeconds = Mathf.FloorToInt(remainingTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        string timerString = $"{minutes:00}:{seconds:00}";
        timerDisplay.text = timerString;
    }

    bool warningShown = false;
    public void AuraFarmDetected()
    {
        if (warningShown) return;
        
        warningShown = true;
        popup_text.Play("aura_farm_pop_up");
    }
}//EndScript