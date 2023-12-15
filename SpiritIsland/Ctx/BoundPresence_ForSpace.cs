namespace SpiritIsland;

public class BoundPresence_ForSpace {

	readonly TargetSpaceCtx ctx;
	readonly Spirit _self;

	public BoundPresence_ForSpace(TargetSpaceCtx ctx ) {
		this.ctx = ctx;
		_self = ctx.Self;
	}

	public bool IsSelfSacredSite => _self.Presence.IsSacredSite(ctx.Tokens);

	public bool IsHere       => _self.Presence.IsOn( ctx.Tokens );

	public int Count => _self.Presence.CountOn( ctx.Tokens );

	public Task PlaceDestroyedHere( int count = 1 )
		=> _self.Presence.Destroyed.MoveToAsync(ctx.Tokens,count);

	public async Task PlaceHere() {
		var from = await _self.SelectSourcePresence();
		await from.MoveToAsync(ctx.Tokens);
//		await ctx.Self.Presence.PlaceAsync( from, ctx.Space );
	}

	public async Task MoveHereFromAnywhere(int count) {
		while(count > 0) {
			// !! cleanup - have SelectDeployed have a version, that only selects moveable
			var src = await ctx.Self.SelectDeployedMovable($"Select presence to move. ({count} remaining)");
			if( src.Space.Tokens.Has(ctx.Self.Presence) )
				await src.MoveTo( ctx.Tokens );
			count--;
		}
	}

}