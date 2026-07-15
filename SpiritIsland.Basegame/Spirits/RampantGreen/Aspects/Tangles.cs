namespace SpiritIsland.Basegame.Spirits.RampantGreen.Aspects;

public class Tangles : IAspect {

	// https://spiritislandwiki.com/index.php?title=Tangles

	static public AspectConfigKey ConfigKey => new AspectConfigKey(ASpreadOfRampantGreen.Name,Name);
	public const string Name = "Tangles";
	public string[] Replaces => [CreepersTearIntoMortar.Name,GiftOfProliferation.Name];

	static InnatePower NewInnate => InnatePower.For(typeof(ImpenetrableTanglesOfGreenery));
	public InnatePower[] NewInnates => [NewInnate];
	static PowerCard NewCard => PowerCard.ForDecorated(BelligerentAndAggressiveCrops.ActAsync);
	public PowerCard[] NewCards => [NewCard];

	public void ModSpirit(Spirit spirit) {
		spirit.ReplaceInnate(CreepersTearIntoMortar.Name, NewInnate);
		spirit.ReplaceCard(GiftOfProliferation.Name, NewCard);
	}

}