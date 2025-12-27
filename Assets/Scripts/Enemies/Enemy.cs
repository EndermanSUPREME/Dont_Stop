interface IEnemy
{
    void TakeDamage(int amount, bool getAura = true);
    void Ignite(float duration);
    bool IsDead();
}

interface IBoss
{
    void StartFight();
}