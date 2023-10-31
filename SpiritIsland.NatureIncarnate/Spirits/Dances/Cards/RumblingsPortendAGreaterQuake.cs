namespace SpiritIsland.NatureIncarnate;

public class RumblingsPortendAGreaterQuake {
	const string Name = "Rumblings Portend a Greater Quake";

	[SpiritCard( Name, 1, Element.Sun, Element.Moon, Element.Air, Element.Earth ), Fast, FromPresence(1)] 
	[Instructions( "If you have at least as many Impending as Power Cards in play, 1 Fear and Add 1 Quake. Push up to 3 Dahan." ), Artist( Artists.EmilyHancock )]
	static async public Task ActAsync( TargetSpaceCtx ctx ) {
		// If you have at least as many Impending as Power Cards in play,
		if(ctx.Self is DancesUpEarthquakes due && due.InPlay.Count <= due.Impending.Count) {
			// 1 Fear
			ctx.AddFear(1);
			// Add 1 Quake.
			await ctx.Tokens.Add(Token.Quake,1);
		}
		
		// Push up to 3 Dahan.
		await ctx.PushUpToNDahan(3);
	}

}
