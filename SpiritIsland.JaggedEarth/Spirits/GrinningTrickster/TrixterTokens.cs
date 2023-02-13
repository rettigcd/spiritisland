namespace SpiritIsland.JaggedEarth;

// == Tokens ==
public class TrixterTokens : SpaceState {
	public TrixterTokens( SpaceState src, bool runAtMax=false ) : base( src ) { _runAtMax = runAtMax; }
	public override BlightTokenBinding Blight => new TricksterBlight(this);
	public override TokenGatherer Gather( Spirit self ) => _runAtMax ? new MaxGatherer(self,this) : base.Gather( self );
	public override TokenPusher Pusher( Spirit self ) => _runAtMax ? new MaxPusher( self, this ) : base.Pusher( self );

	readonly bool _runAtMax;
}

// == Gatherer ==
/// <summary> Gathers at max, event when instructions say Up To </summary>
public class MaxGatherer : TokenGatherer {
	public MaxGatherer( Spirit self, SpaceState tokens ) : base( self, tokens ) { }
	public override Task<SpaceToken[]> GatherUpToN() => GatherN(); // do max!
}

// == Pusher ==
public class MaxPusher : TokenPusher {
	public MaxPusher( Spirit self, SpaceState tokens ) : base( self, tokens ) { }
	public override Task<Space[]> MoveUpToN() => MoveN(); // do max!
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