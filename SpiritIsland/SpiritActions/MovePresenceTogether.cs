namespace SpiritIsland.NatureIncarnate;

public class MovePresenceTogether : SpiritAction {

	public MovePresenceTogether():base( "Move up to 3 Presence together" ) { }

	public override async Task ActAsync( Spirit self ) {

		await new TokenMover(self,"Move",
			new SourceSelector(self.Presence.Lands)
				.AddGroup(3,self.Presence)
				.FromASingleLand(),
			new DestinationSelector( st => st.Space.Range(3).ScopeTokens() )
				.Config( Distribute.ToASingleLand )
		).DoUpToN();
	}
}
