namespace SpiritIsland.Basegame.Spirits.RampantGreen.Aspects;

public class Tangles : IAspect {

	// https://spiritislandwiki.com/index.php?title=Tangles

	static public AspectConfigKey ConfigKey => new AspectConfigKey(ASpreadOfRampantGreen.Name,Name);
	public const string Name = "Tangles";
	public string[] Replaces => [CreepersTearIntoMortar.Name,GiftOfProliferation.Name];

	public void ModSpirit(Spirit spirit) {
		// Replace Innate with Impenetrable Tangles
		for(int i=0;i<spirit.InnatePowers.Length;i++) 
			if( spirit.InnatePowers[i].Title == CreepersTearIntoMortar.Name )
				spirit.InnatePowers[i] = InnatePower.For(typeof(ImpenetrableTanglesOfGreenery));

		// Replace Card
		for( int i = 0; i < spirit.Hand.Count; i++ )
			if( spirit.Hand[i].Title == GiftOfProliferation.Name )
				spirit.Hand[i] = PowerCard.ForDecorated(BelligerentAndAggressiveCrops.ActAsync );
	}

}