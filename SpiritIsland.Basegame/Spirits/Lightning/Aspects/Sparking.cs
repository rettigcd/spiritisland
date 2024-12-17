namespace SpiritIsland.Basegame.Spirits.Lightning.Aspects;

public class Sparking : IAspect {

	// https://spiritislandwiki.com/index.php?title=Sparking

	static public AspectConfigKey ConfigKey => new AspectConfigKey(LightningsSwiftStrike.Name, Name);
	public const string Name = "Sparking";
	public string[] Replaces => [ThunderingDestruction.Name, RagingStorm.Name];

	public void ModSpirit(Spirit spirit) {
		spirit.ReplaceInnate(ThunderingDestruction.Name, InnatePower.For(typeof(GiftOfTheSparkingSky)));
		spirit.ReplaceCard(RagingStorm.Name, PowerCard.ForDecorated(SmiteTheLandWithFulmination.ActAsync));
	}
}