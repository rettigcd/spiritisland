using SpiritIsland.Select;

namespace SpiritIsland.NatureIncarnate;

public class AddOrMoveIncarnaToPresence : GrowthActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {
		if(ctx.Self.Presence is not BreathPresence presence) return;

		var space = await ctx.Self.Gateway.Decision( new ASpace( "Select space to place Incarna.", ctx.Self.Presence.Spaces, Present.Done ) );
		if(space == null) return;

		// Move/Place Incarna
		await presence.Incarna.MoveTo(space,true);
	}

}
