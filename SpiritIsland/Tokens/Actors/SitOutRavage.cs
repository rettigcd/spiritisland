namespace SpiritIsland;

static public class SitOutRavage {

	/// <summary>
	/// User Selects which tokens to sit out. And sits them out for *this-action* only.
	/// </summary>
	static public async Task SelectFightersAndSitThemOut( this SourceSelector sourceSelector, Spirit spirit ) {
		CountDictionary<HumanToken> sitOuts = new CountDictionary<HumanToken>();
		HashSet<Space> targetSpaces = new HashSet<Space>();

		IAsyncEnumerable<SpaceToken> selectedTokens = sourceSelector
			.ConfigOnlySelectEachOnce()
			.GetEnumerator( spirit, Prompt.RemainingParts( "For Ravage, Sit Out" ), Present.Done );
		await foreach(var st in selectedTokens) {
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
		var group = space.Humans( countToNotParticipate, humanToken );
		group.Adjust( SitOut ); // migrates group to new token type
		ActionScope.Current.AtEndOfThisAction( scope => group.Adjust( _ => humanToken ) );
	}
	static HumanToken SitOut(HumanToken humanToken) => humanToken.SetRavageSide( RavageSide.None );

}