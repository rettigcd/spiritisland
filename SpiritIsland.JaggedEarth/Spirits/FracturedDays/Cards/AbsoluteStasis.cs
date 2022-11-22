namespace SpiritIsland.JaggedEarth;

public class AbsoluteStasis {

	// !!! ??? Is there a check that Fractured Days must have time in order to play this card?  Or can they just have negative time?

	[SpiritCard("Absolute Stasis",1,Element.Sun,Element.Air,Element.Earth), Fast]
	// This cannot target an Ocean even if Oceans are in play.
	[FromSacredSite(2,Target.NotOcean)]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		if(ctx.Self is not FracturedDaysSplitTheSky frac)
			return;

		// Cost to use: 1
		if(frac.Time == 0) return;
		await frac.SpendTime(1);

		// Until the end of the slow phase, target land and everything in it cease to exist for all purposes except checking victory/defeat.
			
		ctx.Tokens.InStasis = true;

		// you cannot target into, out of, or through where the land was.

		// --------
		// Restore 
		// --------
		ctx.GameState.TimePasses_ThisRound.Push( ( gs ) => {
			ctx.Tokens.InStasis = false;
			return Task.CompletedTask;
		} );

	}

}