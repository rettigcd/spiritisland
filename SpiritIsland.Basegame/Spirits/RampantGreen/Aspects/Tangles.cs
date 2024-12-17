namespace SpiritIsland.Basegame.Spirits.RampantGreen.Aspects;

public class Tangles : IAspect {

	// https://spiritislandwiki.com/index.php?title=Tangles

	static public AspectConfigKey ConfigKey => new AspectConfigKey(ASpreadOfRampantGreen.Name,Name);
	public const string Name = "Tangles";
	public string[] Replaces => [CreepersTearIntoMortar.Name,GiftOfProliferation.Name];

	public void ModSpirit(Spirit spirit) {
		spirit.ReplaceInnate(CreepersTearIntoMortar.Name, InnatePower.For(typeof(ImpenetrableTanglesOfGreenery)));
		spirit.ReplaceCard(GiftOfProliferation.Name, PowerCard.ForDecorated(BelligerentAndAggressiveCrops.ActAsync));
	}

}