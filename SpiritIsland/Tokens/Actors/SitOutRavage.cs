namespace SpiritIsland;

static public class SitOutRavage {

	/// <summary>
	/// User Selects which tokens to sit out. And sits them out for *this-action* only.
	/// </summary>
	static public async Task SelectFightersAndSitThemOut( Spirit spirit, SourceSelector sourceSelector ) {
		CountDictionary<HumanToken> sitOuts = new CountDictionary<HumanToken>();
		HashSet<Space> targetSpaces = new HashSet<Space>();

		while(true) {
			var st = await sourceSelector.GetSource( spirit, "For Ravage, Sit Out", Present.Done );
			if(st == null) break;

			targetSpaces.Add(st.Space);
			++sitOuts[st.Token.AsHuman()];
		}

		switch(targetSpaces.Count) {
			case 0: 
				return;
			case 1:
				SitOutThisRavageAction( targetSpaces.Single(), sitOuts );
				return;
			default: 
				throw new InvalidOperationException( "SelectFightersAndSitThemOut is only designed to work on 1 space at a time but this is targetting: " + string.Join( ",", targetSpaces ) );
		}

	}

	/// <summary>
	/// Sits out pre-selected tokens from the Count-Dictionary
	/// </summary>
	static public void SitOutThisRavageAction( SpaceState space, CountDictionary<HumanToken> sitOuts ) {
		foreach(var pair in sitOuts)
			DontParticipateInRavageThisAction( space, pair.Key, pair.Value );
	}

	/// <summary>
	/// Marks 1 Token Type as not-participating (and queues up restore at end of acction
	/// </summary>
	static void DontParticipateInRavageThisAction( SpaceState space, HumanToken humanToken, int countToNotParticipate ) {
		var nonParticipating = humanToken.SetRavageSide( RavageSide.None );
		space.Adjust( nonParticipating, countToNotParticipate );
		space.Adjust( humanToken, -countToNotParticipate );

		QueueUpRestore( space, humanToken, countToNotParticipate, nonParticipating );
	}

	/// <summary>
	/// Adds the restore action to the end of the current Action-Scope
	/// </summary>
	static void QueueUpRestore( SpaceState space, HumanToken humanToken, int countToNotParticipate, HumanToken nonParticipating ) {
		ActionScope.Current.AtEndOfThisAction( scope => {
			space.Adjust( nonParticipating, -countToNotParticipate );
			space.Adjust( humanToken, countToNotParticipate );
		} );
	}

}