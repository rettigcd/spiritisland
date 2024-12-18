namespace SpiritIsland.JaggedEarth;

// == Tokens ==
// * Runs things at max.  (Pusher,Gatherer)
// * Enables adding the extra strife. (this could pulled out)
public class TricksterTokens( Spirit spirit, Space src, bool runAtMax = false ) 
	: Space( src )
{

	public override BlightTokenBinding Blight => new TricksterBlight(this);
	public override TokenMover Gather( Spirit self ) => base.Gather( self ).RunAtMax( _runAtMax );

	public override TokenMover Pusher( Spirit self, SourceSelector sourceSelector, DestinationSelector? dest = null ) 
		=> base.Pusher( self, sourceSelector, dest )
			.RunAtMax( _runAtMax );

	public override async Task<SpaceToken> Add1StrifeToAsync( HumanToken invader ) {
		HumanToken humanToken = await AddRemoveStrifeAsync( invader, 1, 1 );
		if( _spirit.Energy != 0 )
			await Pay1EnergyToStrifeInRange1Land();
		return humanToken.On(this);
	}

	public static readonly SpecialRule ARealFlairForDiscord_Rule = new SpecialRule(
		"A Real Flair for Discord", 
		"After one of your Powers adds strife in a land, you may pay 1 Energy to add 1 strife within Range-1 of that land."
	);

	// A Real Flair for Discord
	async Task Pay1EnergyToStrifeInRange1Land() {
		var nearbyInvaders = _spirit.PowerRangeCalc.GetTargetingRoute_MultiSpace( Adjacent, new TargetCriteria( 1 ) ).Targets
			.SelectMany( ss => ss.InvaderTokens().On( ss ) )
			.ToArray();
		var invader2 = await _spirit.SelectAsync( new A.SpaceTokenDecision( "Add additional strife for 1 energy", nearbyInvaders, Present.Done ) );
		if(invader2 is not null) {
			--_spirit.Energy;
			var tokens2 = (TricksterTokens)invader2.Space; // need to cast in order to access non-cascading protected member .AddRemoveStrife()
			await tokens2.AddRemoveStrifeAsync( invader2.Token.AsHuman(), 1, 1 );
		}
	}

	readonly Spirit _spirit = spirit;
	readonly bool _runAtMax = runAtMax;
}

// Blight
public class TricksterBlight( Space space ) : BlightTokenBinding( space ) {
	public override async Task<ITokenRemovedArgs> Remove( int count, RemoveReason reason = RemoveReason.Removed ) {
		var self = ActionScope.Current.Owner ?? throw new InvalidOperationException( "Action Scope has no owner." );
		await GrinningTricksterStirsUpTrouble.CleaningUpMessesIsSuchADrag( self, _space ); // feature envy?
		return await base.Remove( count, reason );
	}
}