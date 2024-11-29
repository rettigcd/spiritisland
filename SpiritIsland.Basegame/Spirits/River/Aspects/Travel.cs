namespace SpiritIsland.Basegame;

public class Travel : IAspect {

	static public AspectConfigKey ConfigKey => new AspectConfigKey(RiverSurges.Name, Name);

	public const string Name = "Travel";

	public void ModSpirit(Spirit spirit) {
		if( spirit is not RiverSurges) throw new ArgumentException("Travel may only be applied to River.");
		// Replace 'River's Domain' with Travel and Tend
		spirit.SpecialRules = [ TravelOnRiversBack.Rule, TendingPresence.Rule];

		// Replace Presene and Token
		var old = spirit.Presence;
		spirit.Presence = new TendingPresence(spirit, old.Energy, old.CardPlays);

		// Add Post-Growth Action (after precenses are placed).
		old.Energy.Slots.First().Action = new TravelOnRiversBack();
	}

}

class TravelOnRiversBack : SpiritAction {

	static public SpecialRule Rule => new SpecialRule(
		"Travel on the River's Back",
		"After Growth, choose up to 2 Dahan among your lands. Move each of them to any land contiguously connected by your Presence."
	);

	public TravelOnRiversBack() {}

	public override async Task ActAsync(Spirit spirit) {
		for( int i = 1; i <= 2; ++i ) {
			var dahanOptions = spirit.Presence.Lands
				.SelectMany(s => s.SpaceTokensOfTag(TokenCategory.Dahan));
			var dahan = await spirit.SelectAsync(new A.SpaceTokenDecision($"Select Dahan to Move to contiguously presence-connected land. ({i} of 2)", dahanOptions, Present.Done));
			if( dahan == null ) return;

			HashSet<Space> destinationOptions = GetContiguoslyConnectedPresense(spirit, dahan.Space);
			if( destinationOptions.Count == 0 ) continue;

			var destination = await spirit.SelectSpaceAsync("Move Dahan to.", destinationOptions, Present.Done);
			if( destination is not null )
				await dahan.MoveToAsync(destination);
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
