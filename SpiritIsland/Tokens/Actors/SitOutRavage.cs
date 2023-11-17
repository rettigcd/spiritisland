namespace SpiritIsland;

static public class SitOutRavage {

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
				SitOutNextRavage( targetSpaces.Single(), sitOuts );
				return;
			default: 
				throw new InvalidOperationException( "SelectFightersAndSitThemOut is only designed to work on 1 space at a time but this is targetting: " + string.Join( ",", targetSpaces ) );
		}

	}

	static void SitOutNextRavage( SpaceState space, CountDictionary<HumanToken> sitOuts ) {
		foreach(var pair in sitOuts)
			DontParticipateInRavage( space, pair.Key, pair.Value );
	}

	static void DontParticipateInRavage( SpaceState space, HumanToken humanToken, int countToNotParticipate ) {
		var nonParticipating = humanToken.SetRavageSide( RavageSide.None );
		space.Adjust( nonParticipating, countToNotParticipate );
		space.Adjust( humanToken, -countToNotParticipate );

		Restore( space, humanToken, countToNotParticipate, nonParticipating );
	}


	static void Restore( SpaceState space, HumanToken humanToken, int countToNotParticipate, HumanToken nonParticipating ) {
		ActionScope.Current.AtEndOfThisAction( scope => {
			space.Adjust( nonParticipating, -countToNotParticipate );
			space.Adjust( humanToken, countToNotParticipate );
		} );
	}

}