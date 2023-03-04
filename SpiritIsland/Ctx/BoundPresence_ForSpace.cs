namespace SpiritIsland;

public class BoundPresence_ForSpace {

	readonly TargetSpaceCtx ctx;
	readonly Spirit _self;

	public BoundPresence_ForSpace(TargetSpaceCtx ctx ) {
		this.ctx = ctx;
		_self = ctx.Self;
	}

	public bool IsSelfSacredSite => _self.Presence.IsSacredSite(ctx.Tokens);

	public bool IsHere       => ctx.Tokens.Has(_self.Token);

	public int Count => ctx.Tokens[_self.Token];

	public async Task PlaceDestroyedHere( int count = 1 ) {
		count = Math.Min(count, _self.Presence.Destroyed);
		while(count-- > 0 )
			await _self.Presence.Place( Track.Destroyed, ctx.Space );
	} 

	public async Task PlaceHere() {
		var from = await _self.SelectMovablePresence();
		await ctx.Self.Presence.Place( from, ctx.Space );
	}

	public async Task MoveHereFromAnywhere(int count) {
		if(!_self.Presence.CanMove) return;

		while(count > 0) {
			// !! cleanup - have SelectDeployed have a version, that only selects moveable
			var from = await ctx.Self.SelectDeployedMovable($"Select presence to move. ({count} remaining)");
			if( ctx.Self.Presence.HasMovableTokens( from.Tokens ))
				await _self.Token.Move( from, ctx.Tokens );
			count--;
		}
	}

}