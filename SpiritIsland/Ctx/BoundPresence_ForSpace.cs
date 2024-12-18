namespace SpiritIsland;

public class BoundPresence_ForSpace( TargetSpaceCtx _ctx ) {
	readonly Spirit _self = _ctx.Self;

	public bool IsSelfSacredSite => _self.Presence.IsSacredSite(_ctx.Space);

	public bool IsHere       => _self.Presence.IsOn( _ctx.Space );

	public int Count => _self.Presence.CountOn( _ctx.Space );

	public Task PlaceDestroyedHere( int count = 1 )
		=> _self.Presence.Destroyed.MoveToAsync(_ctx.Space,count);

	public async Task PlaceHere() {
		var from = await _self.SelectSourcePresence();
		await from.MoveToAsync(_ctx.Space);
	}

	public async Task MoveHereFromAnywhere(int count) {
		while(count > 0) {
			// !! cleanup - have SelectDeployed have a version, that only selects moveable
			var src = (await _ctx.Self.SelectDeployedMovable($"Select presence to move. ({count} remaining)"))!;
			if( src.Space.Has(_ctx.Self.Presence) )
				await src.MoveTo( _ctx.Space );
			count--;
		}
	}

}