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
			await ctx.Self.Presence.Place( Track.Destroyed, ctx.Space, ctx.GameState );
	} 

	public async Task PlaceHere() {
		var from = await SelectSource();
		await ctx.Self.Presence.Place( from, ctx.Space, ctx.GameState );
	}

	public async Task MoveHereFromAnywhere(int count) {

		while(count > 0) {
			var from = await ctx.Presence.SelectDeployed($"Select presence to move. ({count} remaining)");
			this.Move( from, ctx.Space );
			count--;
		}
	}

}