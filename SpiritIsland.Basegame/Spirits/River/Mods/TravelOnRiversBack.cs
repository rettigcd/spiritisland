namespace SpiritIsland.Basegame;

class TravelOnRiversBack : SpiritAction {

	public const string Name = "Travel on the River's Back";
	const string RuleDescription = "After Growth, choose up to 2 Dahan among your lands. Move each of them to any land contiguously connected by your Presence.";
	static public SpecialRule Rule => new SpecialRule( Name, RuleDescription );

	static public void InitAspect(Spirit spirit) {
		var old = spirit.Presence;
		old.Energy.Slots.First().Action = new TravelOnRiversBack();
	}

	public TravelOnRiversBack() {}

	public override async Task ActAsync(Spirit spirit) {
		var dahanOptions = spirit.Presence.Lands
			.SelectMany(s => s.SpaceTokensOfTag(TokenCategory.Dahan))
			.ToList();

		for( int i = 1; i <= 2; ++i ) {
			var dahanToMove = await spirit.Select(new A.SpaceTokenDecision($"Select Dahan to Move to contiguously presence-connected land. ({i} of 2)", dahanOptions, Present.Done));
			if( dahanToMove is null ) return;

			HashSet<Space> destinationOptions = GetContiguoslyConnectedPresense(spirit, dahanToMove.Space);
			if( destinationOptions.Count == 0 ) continue;

			var destination = await spirit.Select("Move Dahan to.", destinationOptions, Present.Done);
			if( destination is not null ) {
				// If this was last one in source, remove it.
				if(dahanToMove.Count == 1)
					dahanOptions.Remove(dahanToMove); // we moved it once to where we needed it, no need to move it again

				await dahanToMove.MoveToAsync(destination);

				// IF they moved a token somewhere, then they don't want to remove it from there, so remove that option.
				dahanOptions.Remove(dahanToMove.Token.On(destination));
			}
		}
	}

	static HashSet<Space> GetContiguoslyConnectedPresense(Spirit spirit, Space first) {
		HashSet<Space> contiguouslyConnected = [];
		List<Space> newSpaces = [first];
		while( 0 < newSpaces.Count ) {
			var next = newSpaces[^1]; newSpaces.RemoveAt(newSpaces.Count - 1);
			contiguouslyConnected.Add(next);
			newSpaces.AddRange(next.Adjacent.Where(spirit.Presence.IsOn).Except(contiguouslyConnected));
		}
		contiguouslyConnected.Remove(first);
		return contiguouslyConnected;
	}

}
