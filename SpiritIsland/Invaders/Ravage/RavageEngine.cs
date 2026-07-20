namespace SpiritIsland;

public class RavageEngine {

	/// <summary>
	/// Hook for a ravage engine that caches a direct reference to one of its own island-mod tokens (e.g.
	/// Russia L6's RecordBlightAdded - docs/GameSerialization-Roadmap.md's Adversary section). Replaying
	/// Init on a restored GameState reconstructs the engine and a fresh copy of that token together, but
	/// Tokens_ForIsland.FromJson then wipes/replaces island mods from the JSON snapshot - orphaning the
	/// engine's cached reference. A restore driver should call this (with the same ISerializationContext
	/// used for that wipe) once tokens are restored, so the engine can re-point at whatever instance
	/// actually ended up live. No-op by default; only overridden where such a gap actually exists.
	/// </summary>
	public virtual void ResolveAfterTokensRestored( ISerializationContext ctx ) { }

	protected virtual bool MatchesCardForRavage( InvaderCard card, Space space ) => card.MatchesCard( space );

	public virtual async Task ActivateCard( InvaderCard card ) {
		ActionScope.Current.Log( new Log.InvaderActionEntry( "Ravaging:" + card.Code ) );
		var ravageSpacesMatchingCard = ActionScope.Current.Spaces
			.Where( ss => MatchesCardForRavage( card, ss ) )
			.ToList();

		// find ravage spaces that have invaders
		var ravageSpacesWithInvaders = ravageSpacesMatchingCard
			.Where( tokens => tokens.HasInvaders() )
			.ToArray();

		// Add Ravage tokens to spaces with invaders
		foreach(var s in ravageSpacesWithInvaders)
			s.Adjust(InvaderActionToken.DoRavage, s.SpaceSpec.InvaderActionCount );

		// get spaces with just-added Ravages + any previously added ravages
		var spacesWithDoRavage = ActionScope.Current.Spaces
			.Where( ss => ss[InvaderActionToken.DoRavage] > 0 )
			.ToArray();

		foreach(var ravageSpace in spacesWithDoRavage)
			await DoAllRavagesOn1Space( ravageSpace );
	}

	static async Task DoAllRavagesOn1Space( Space ravageSpace ) {
		int ravageCount = PullRavageTokens( ravageSpace );

		while(0 < ravageCount--)
			await ravageSpace.Ravage();
	}

	static int PullRavageTokens( Space ravageSpace ) {
		int ravageCount = ravageSpace[InvaderActionToken.DoRavage];
		ravageSpace.Init(InvaderActionToken.DoRavage, 0 );
		return ravageCount;
	}

}
