namespace SpiritIsland.NatureIncarnate;

public class AddOrMoveIncarnaToPresence : SpiritAction {

	public AddOrMoveIncarnaToPresence():base( "Add or Move Incarna to Presence" ) { }

	public override async Task ActAsync( SelfCtx ctx ) {
		if(ctx.Self.Presence is not IncarnaPresence presence) return;

		var space = await ctx.Self.Select( new A.Space( "Select space to place Incarna.", ctx.Self.Presence.Spaces, Present.Done ) );
		if(space == null) return;

		// Move/Place Incarna
		await presence.Incarna.MoveTo(space,true);
	}

}
