// A tiny contract: "I am something that can be damaged."
// Weapons (sword, pumpkin seed, enemy attacks) talk to THIS, not to a specific
// enemy or player script. That way one weapon can hurt anything that can be hurt
// - enemies, the player, or a breakable prop later - without knowing what it is.
public interface IDamageable
{
    // Apply 'amount' points of damage to this thing.
    void TakeDamage(float amount);

    // True once this thing has been killed, so attackers can stop hitting it.
    bool IsDead { get; }
}
