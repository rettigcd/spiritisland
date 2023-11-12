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
		=> _self.Presence.PlaceDestroyedAsync(count,ctx.Space);

	public async Task PlaceHere() {
		var from = await _self.SelectSourcePresence();
		await ctx.Self.Presence.Place( from, ctx.Space );
	}

	public async Task MoveHereFromAnywhere(int count) {
		if(!_self.Presence.CanMove) return;

		while(count > 0) {
			// !! cleanup - have SelectDeployed have a version, that only selects moveable
			var src = await ctx.Self.SelectDeployedMovable($"Select presence to move. ({count} remaining)");
			if( ctx.Self.Presence.HasMovableTokens( src.Space.Tokens ))
				await src.MoveTo( ctx.Tokens );
			count--;
		}
	}

}