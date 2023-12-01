namespace SpiritIsland.JaggedEarth;

// == Tokens ==
public class TricksterTokens : SpaceState {
	public TricksterTokens( Spirit spirit, SpaceState src, bool runAtMax=false ) : base( src ) { 
		_spirit = spirit;
		_runAtMax = runAtMax;
	}
	public override BlightTokenBinding Blight => new TricksterBlight(this);
	public override TokenMover Gather( Spirit self ) => base.Gather( self ).RunAtMax( _runAtMax );

	public override TokenMover Pusher( Spirit self, SourceSelector sourceSelector ) 
		=> base.Pusher( self, sourceSelector )
			.RunAtMax( _runAtMax );

	public override async Task<SpaceToken> Add1StrifeToAsync( HumanToken invader ) {
		HumanToken humanToken = await AddRemoveStrifeAsync( invader, 1, 1 );
		if( _spirit.Energy != 0 )
			await Pay1EnergyToStrifeInRange1Land();
		return humanToken.On(Space);
	}

	public static readonly SpecialRule ARealFlairForDiscord_Rule = new SpecialRule(
		"A Real Flair for Discord", 
		"After one of your Powers adds strife in a land, you may pay 1 Energy to add 1 strife within Range-1 of that land."
	);

	// A Real Flair for Discord
	async Task Pay1EnergyToStrifeInRange1Land() {
		var nearbyInvaders = _spirit.PowerRangeCalc.GetSpaceOptions( Adjacent, new TargetCriteria( 1 ) )
			.SelectMany( ss => ss.InvaderTokens().On( ss.Space ) )
			.ToArray();
		var invader2 = await _spirit.SelectAsync( new A.SpaceToken( "Add additional strife for 1 energy", nearbyInvaders, Present.Done ) );
		if(invader2 != null) {
			--_spirit.Energy;
			var tokens2 = (TricksterTokens)invader2.Space.Tokens; // need to cast in order to access non-cascading protected member .AddRemoveStrife()
			await tokens2.AddRemoveStrifeAsync( invader2.Token.AsHuman(), 1, 1 );
		}
	}

	readonly Spirit _spirit;
	readonly bool _runAtMax;
}

// Blight
public class TricksterBlight : BlightTokenBinding {

	public TricksterBlight( SpaceState tokens ) : base( tokens ) { }

	public override async Task Remove( int count, RemoveReason reason = RemoveReason.Removed ) {
		var self = ActionScope.Current.Owner ?? throw new InvalidOperationException( "Action Scope has no owner." );
		await GrinningTricksterStirsUpTrouble.CleaningUpMessesIsSuckADrag( self, _tokens ); // feature envy?
		await base.Remove( count, reason );
	}
}