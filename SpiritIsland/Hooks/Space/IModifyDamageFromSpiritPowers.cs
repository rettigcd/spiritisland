namespace SpiritIsland;

public interface IModifyDamageFromSpiritPowers {
	Task ModifyDamage(DamageFromSpiritPowers args);
}

public class DamageFromSpiritPowers {
	public required Space Space;
	public int Damage;
	public required ITokenClass[] Classes;
}

