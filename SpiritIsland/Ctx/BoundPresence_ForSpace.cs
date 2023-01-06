namespace SpiritIsland;

public class BoundPresence_ForSpace : BoundPresence {

	#region constructor

	readonly TargetSpaceCtx ctx;

	public BoundPresence_ForSpace(TargetSpaceCtx ctx ) : base( ctx ) {
		this.ctx = ctx;
	}

	#endregion

	public bool IsSelfSacredSite => IsSacredSite(ctx.Space);

	public bool IsHere       => ctx.Self.Presence.IsOn( ctx.Tokens );

	public int Count => ctx.Self.Presence.CountOn(ctx.Tokens);

	public async Task PlaceDestroyedHere( int count = 1 ) {
		count = Math.Min(count, ctx.Self.Presence.Destroyed);
		while(count-- > 0 )
			await ctx.Self.Presence.Place( Track.Destroyed, ctx.Space, ctx.GameState, _actionScope );
	} 

	public async Task PlaceHere() {
		var from = await SelectSource_Movable();
		await ctx.Presence.Place( from, ctx.Space );
	}

	public async Task MoveHereFromAnywhere(int count) {

		while(count > 0) {
			// !! cleanup - have SelectDeployed have a version, that only selects moveable
			var from = await ctx.Presence.SelectDeployedMovable($"Select presence to move. ({count} remaining)");
			if( ctx.Self.Presence.HasMovableTokens(ctx.GameState.Tokens[from]))
				await Move( from, ctx.Space );
			count--;
		}
	}

}