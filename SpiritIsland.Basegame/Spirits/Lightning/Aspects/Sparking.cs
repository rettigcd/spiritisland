namespace SpiritIsland.Basegame.Spirits.Lightning.Aspects;

public class Sparking : IAspect {

	// https://spiritislandwiki.com/index.php?title=Sparking

	static public AspectConfigKey ConfigKey => new AspectConfigKey(LightningsSwiftStrike.Name, Name);
	public const string Name = "Sparking";
	public string[] Replaces => [ThunderingDestruction.Name, RagingStorm.Name];

	static InnatePower NewInnate => InnatePower.For(typeof(GiftOfTheSparkingSky));
	public InnatePower[] NewInnates => [NewInnate];
	static PowerCard NewCard => PowerCard.ForDecorated(SmiteTheLandWithFulmination.ActAsync);
	public PowerCard[] NewCards => [NewCard];

	public void ModSpirit(Spirit spirit) {
		spirit.ReplaceInnate(ThunderingDestruction.Name, NewInnate);
		spirit.ReplaceCard(RagingStorm.Name, NewCard);
	}
}