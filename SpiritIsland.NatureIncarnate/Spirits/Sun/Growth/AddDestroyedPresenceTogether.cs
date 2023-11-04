namespace SpiritIsland.NatureIncarnate;

public class AddDestroyedPresenceTogether : SpiritAction {
	public AddDestroyedPresenceTogether():base("Add up to 3 Destroyed Presence together" ) { }
	public override async Task ActAsync( SelfCtx ctx ) {
		var spirit = ctx.Self;
		var pres = spirit.Presence;
		int max = Math.Min( pres.Destroyed, 3 );
		if(max == 0) return;

		// add up to 3 destroyed presence together
		var options = spirit.FindSpacesWithinRange( new TargetCriteria( 1 ), false );
		var dst = await spirit.Gateway.Decision(new Select.ASpace($"Add up to {max} Destroyed Presence", options, Present.Always, pres.Token) );
		if(dst == null ) return;

		int numToPlace = await spirit.SelectNumber("How many presences would you like to place?", max, 1);

		await dst.Tokens.Add( pres.Token, numToPlace );
		--pres.Destroyed;
	}
}