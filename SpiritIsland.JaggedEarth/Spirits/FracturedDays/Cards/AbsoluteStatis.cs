namespace SpiritIsland.JaggedEarth;

public class AbsoluteStatis {

	[SpiritCard("Absolute Stasis",1,Element.Sun,Element.Air,Element.Earth), Fast, FromSacredSite(2,Target.NotOcean)]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		if(ctx.Self is not FracturedDaysSplitTheSky frac) return;

		// Cost to use: 1
		if(frac.Time == 0) return;
		await frac.SpendTime(1);

		// Until the end of the slow phase, target land and everything in it cease to exist for all purposes except checking victory/defeat.
			
		// Put Spirits presence into Stasis
		foreach(var spirit in ctx.GameState.Spirits)
			spirit.Presence.PutInStasis( ctx.Space, ctx.GameState );

		// Disconnect space
		var adjacents = ctx.Space.Adjacent.ToArray();
		ctx.Space.Board.Remove(ctx.Space); // !!! this will erroneously hide cities and towns from the Terror-Level Victory check

		// you cannot target into, out of, or through where the land was.
		// This cannot target an Ocean even if Oceans are in play.

		// --------
		// Restore 
		// --------
		ctx.GameState.TimePasses_ThisRound.Push( ( gs ) => {
			foreach(var spirit in ctx.GameState.Spirits)
				spirit.Presence.ReleaseFromStasis( ctx.Space, ctx.GameState );

			ctx.Space.Board.Add(ctx.Space, adjacents);

			return Task.CompletedTask;
		} );

	}

}