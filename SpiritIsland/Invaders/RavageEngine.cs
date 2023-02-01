namespace SpiritIsland;

public class RavageEngine {
	protected virtual bool MatchesCardForRavage( InvaderCard card, SpaceState spaceState ) => card.MatchesCard( spaceState );

	public virtual async Task ActivateCard( InvaderCard card, GameState gameState ) {
		gameState.Log( new Log.InvaderActionEntry( "Ravaging:" + card.Text ) );
		var ravageSpacesMatchingCard = gameState.AllActiveSpaces.Where( ss => MatchesCardForRavage( card, ss ) ).ToList();

		// find ravage spaces that have invaders
		var ravageSpacesWithInvaders = ravageSpacesMatchingCard
			.Where( tokens => tokens.HasInvaders() )
			.ToArray();

		// Add Ravage tokens to spaces with invaders
		foreach(var s in ravageSpacesWithInvaders)
			s.Adjust( ModToken.DoRavage, 1 );

		// get spaces with just-added Ravages + any previously added ravages
		var spacesWithDoRavage = gameState.AllActiveSpaces
			.Where( ss => ss[ModToken.DoRavage] > 0 )
			.ToArray();

		foreach(var ravageSpace in spacesWithDoRavage)
			await DoAllRavagesOn1Space( ravageSpace );
	}

	static async Task DoAllRavagesOn1Space( SpaceState ravageSpace ) {
		int ravageCount = PullRavageTokens( ravageSpace );

		while(0 < ravageCount--)
			await ravageSpace.Ravage();
	}

	static int PullRavageTokens( SpaceState ravageSpace ) {
		int ravageCount = ravageSpace[ModToken.DoRavage];
		ravageSpace.Init( ModToken.DoRavage, 0 );
		return ravageCount;
	}

}
