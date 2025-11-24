interface IEnemy
{
    void TakeDamage(int amount, bool getAura = true);
    void Ignite();
    bool IsDead();
}