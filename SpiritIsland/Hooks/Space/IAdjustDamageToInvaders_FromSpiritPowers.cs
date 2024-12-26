namespace SpiritIsland;

public interface IAdjustDamageToInvaders_FromSpiritPowers {
	Task ModifyDamage(DamageFromSpiritPowers args);
}

public class DamageFromSpiritPowers {
	public required Space Space;
	public int Damage;
	public required ITokenClass[] Classes;
}
