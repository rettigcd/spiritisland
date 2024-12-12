namespace SpiritIsland.Basegame.Spirits.Lightning.Aspects;

public class Sparking : IAspect {

	// https://spiritislandwiki.com/index.php?title=Sparking

	static public AspectConfigKey ConfigKey => new AspectConfigKey(LightningsSwiftStrike.Name, Name);
	public const string Name = "Sparking";

	public void ModSpirit(Spirit spirit) {
		// Replaces	Innate Power: Thundering Destruction
		spirit.InnatePowers[0] = InnatePower.For(typeof(GiftOfTheSparkingSky));
		
		// Replace Raging Storm with Smite the Land with Fulmination
		var ragingStore = spirit.Hand.First(x=>x.Title==RagingStorm.Name);
		spirit.Hand.Remove(ragingStore);
		spirit.Hand.Add(PowerCard.For(typeof(SmiteTheLandWithFulmination)));
	}
}