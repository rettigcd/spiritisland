namespace SpiritIsland.NatureIncarnate;

public class BloomingOfTheRocksAndTrees {
	const string Name = "Blooming of the Rocks and Trees";

	[SpiritCard( Name, 0, Element.Sun, Element.Air, Element.Earth, Element.Plant ), Slow, FromSacredSite(1)]
	[Instructions( "If no Blight is present, Add 1 Vitality. -or- If no Invaders are present, Add 1 Wilds. -If you have- 3 Plant: You may do both." ), Artist( Artists.AalaaYassin )]
	static async public Task ActAsync( SelfCtx ctx ) {
		await Task.Run( () => { } );
	}

}
