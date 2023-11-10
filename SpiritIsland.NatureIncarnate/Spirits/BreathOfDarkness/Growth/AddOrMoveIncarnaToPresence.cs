namespace SpiritIsland.NatureIncarnate;

public class AddOrMoveIncarnaToPresence : SpiritAction {

	public AddOrMoveIncarnaToPresence():base( "AddOrMoveIncarnaToPresence" ) { }

	public override async Task ActAsync( SelfCtx ctx ) {
		if(ctx.Self.Presence is not BreathPresence presence) return;

		var space = await ctx.Self.Gateway.Select( new A.Space( "Select space to place Incarna.", ctx.Self.Presence.Spaces, Present.Done ) );
		if(space == null) return;

		// Move/Place Incarna
		await presence.Incarna.MoveTo(space,true);
	}

}
