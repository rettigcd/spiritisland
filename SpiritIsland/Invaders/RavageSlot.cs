namespace SpiritIsland;

// ??? Is this the Visitor Pattern ???
// !!! We could pull all of the Ravage behavior and Build behavior out of the Invader Cards and put them in the Slot
public class RavageSlot : InvaderSlot {
	public RavageSlot() : base( "Ravage" ) { }
	public override Task ActivateCard( InvaderCard card, GameState gameState ) => Engine.ActivateCard( card, gameState );
	public RavageEngine Engine = new RavageEngine();
}

public class RavageEngine {
	protected virtual bool MatchesCardForRavage( InvaderCard card, SpaceState spaceState ) => card.MatchesCard( spaceState );

	public virtual async Task ActivateCard( InvaderCard card, GameState gameState ) {
		gameState.Log( new InvaderActionEntry( "Ravaging:" + card.Text ) );
		var ravageSpacesMatchingCard = gameState.AllActiveSpaces.Where( ss => MatchesCardForRavage( card, ss ) ).ToList();

		// find ravage spaces that have invaders
		var ravageSpacesWithInvaders = ravageSpacesMatchingCard
			.Where( tokens => tokens.HasInvaders() )
			.ToArray();

		// Add Ravage tokens to spaces with invaders
		foreach(var s in ravageSpacesWithInvaders)
			s.Adjust( TokenType.DoRavage, 1 );

		// get spaces with just-added Ravages + any previously added ravages
		var spacesWithDoRavage = gameState.AllActiveSpaces
			.Where( ss => ss[TokenType.DoRavage] > 0 )
			.ToArray();

		foreach(var ravageSpace in spacesWithDoRavage)
			await DoAllRavagesOn1Space( gameState, ravageSpace );
	}

	static async Task DoAllRavagesOn1Space( GameState gameState, SpaceState ravageSpace ) {
		int ravageCount = PullRavageTokens( ravageSpace );

		while(0 < ravageCount--)
			await new RavageAction( gameState, ravageSpace ).Exec();
	}

	static int PullRavageTokens( SpaceState ravageSpace ) {
		int ravageCount = ravageSpace[TokenType.DoRavage];
		ravageSpace.Init( TokenType.DoRavage, 0 );
		return ravageCount;
	}

}
