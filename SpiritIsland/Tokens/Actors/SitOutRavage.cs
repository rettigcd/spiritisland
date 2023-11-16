namespace SpiritIsland;

static public class SitOutRavage {

	static public async Task SelectFightersAndSitThemOut( Spirit spirit, SpaceState space, SourceSelector sourceSelector ) {
		CountDictionary<HumanToken> sitOuts = new CountDictionary<HumanToken>();

		while(true) {
			var st = await sourceSelector.GetSource( spirit, "For Ravage, Sit Out", Present.Done );
			if(st == null) break;

			++sitOuts[st.Token.AsHuman()];
		}

		SitOutNextRavage( space, sitOuts );
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