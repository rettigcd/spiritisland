namespace SpiritIsland.JaggedEarth;

public partial class ManyMindsMoveAsOne {

	class ManyMindTokens : SpaceState {
		public ManyMindTokens( SpaceState src ):base( src ) { }
		public override TokenMover Gather( Spirit self ) => new TokenMover( self, "Gather", 
			new BeastSourceSelector( this ), 
			new DestinationSelector( this )
		);
		public override TokenMover Pusher( Spirit self , bool stoppedByBadlands = false) => new TokenMover(self,"Push",
			new SourceSelector(this),
			stoppedByBadlands ? new DestinationSelector() : new DestinationSelector(  ExtendBeastBy1 )
		);

		static SpaceState[] ExtendBeastBy1( SpaceToken st ) {
			int range = st.Token.Class == Token.Beast ? 2 : 1; // Compare Class, not Token so we get all beasts
			return st.Space.Tokens.Range( range ).ToArray();
		}


	}

}

class BeastSourceSelector : SourceSelector {
	public BeastSourceSelector( SpaceState destination ) : base( destination.Adjacent ) {
		_destination = destination;
	}
	protected override Task<SpaceToken[]> GetSourceOptions() {
		var items = new List<SpaceToken>();
		foreach(var group in RemainingTypes) {
			int range = group == Token.Beast ? 2 : 1;
			foreach(var space in _destination.Range( range )) // gather, not Range
				foreach(var token in space.OfTag( group ))
					items.Add( token.On( space.Space ) );
		}
		return Task.FromResult( items.ToArray() );
	}
	readonly SpaceState _destination;
}