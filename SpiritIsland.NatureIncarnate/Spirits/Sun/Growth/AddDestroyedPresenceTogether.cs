namespace SpiritIsland.NatureIncarnate;

public class AddDestroyedPresenceTogether : SpiritAction {

	public AddDestroyedPresenceTogether():base("Add up to 3 Destroyed Presence together" ) { }

	public override async Task ActAsync( SelfCtx ctx ) {
		var spirit = ctx.Self;
		var pres = spirit.Presence;
		if(pres.Destroyed == 0) return;

		int max = Math.Min( pres.Destroyed, 3 );

		// Place up to 3 (destroyed) presence together
		Space dst = await spirit.Select( A.Space.ToPlaceDestroyedPresence(
			new TargetCriteria( 1 ),
			Present.Always,
			spirit,
			max
		) );

		if(dst == null ) return;

		int numToPlace = await spirit.SelectNumber("How many presences would you like to place?", max, 1);

		await dst.Tokens.Add( pres.Token, numToPlace );
		--pres.Destroyed;
	}
}