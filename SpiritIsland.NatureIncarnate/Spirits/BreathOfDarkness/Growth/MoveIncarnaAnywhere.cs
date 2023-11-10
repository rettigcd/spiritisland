using SpiritIsland.Select;

namespace SpiritIsland.NatureIncarnate;

/// <summary> Move only. Not Add </summary>
public class MoveIncarnaAnywhere : SpiritAction {

	public MoveIncarnaAnywhere():base( "Move Incarna anywhere" ) { }
	public override async Task ActAsync( SelfCtx ctx ) {
		Space? space = await ctx.Self.Gateway.Decision( new ASpace( "Select space to place Incarna.", GameState.Current.Spaces, Present.Done ) );
		if(space == null) return;

		// Move/Place Incarna
		if(ctx.Self.Presence is not IHaveIncarna ihi) return;
		var incarna = ihi.Incarna;
		if(incarna.Space == null) return; // not on board, don't add

		await incarna.Space.Remove( incarna, 1 );
		await space.Tokens.Add( incarna, 1 );

	}

}
