namespace SpiritIsland.JaggedEarth;

// == Tokens ==
public class TrixterTokens : SpaceState {
	public TrixterTokens( SpaceState src, bool runAtMax=false ) : base( src ) { _runAtMax = runAtMax; }
	public override BlightTokenBinding Blight => new TricksterBlight(this);
	public override TokenMover Gather( Spirit self ) => base.Gather( self ).RunAtMax( _runAtMax );
	public override TokenMover Pusher( Spirit self, bool stoppedByBadlands=false ) 
		=> base.Pusher( self, stoppedByBadlands ).RunAtMax( _runAtMax );

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