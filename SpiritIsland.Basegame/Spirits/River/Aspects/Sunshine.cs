namespace SpiritIsland.Basegame;

public class Sunshine : IAspect {

	static public AspectConfigKey ConfigKey => new AspectConfigKey(RiverSurges.Name, Name);

	public const string Name = "Sunshine";

	public void ModSpirit(Spirit spirit) {
		// Remove Boon of Vigor
		var boonCard = spirit.Hand.FirstOrDefault(x=>x.Title==BoonOfVigor.Name);
		if(boonCard is not null)
			spirit.Hand.Remove(boonCard);

		// Add new Innate
		spirit.InnatePowers = [.. spirit.InnatePowers, InnatePower.For(typeof(BoonOfSunshinesPromise))];

		// Add 1 Energy
		++spirit.Energy;
	}
}
