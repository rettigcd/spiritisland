namespace SpiritIsland.JaggedEarth;

public class FlyFastAsThought : ISpiritMod, IConfigureMyActions {
	public const string Name = "Fly Fast as Thought";
	const string Description = "When you Gather or Push Beast, they may come from or go to lands up to 2 distant.";
	static public SpecialRule Rule => new SpecialRule( Name, Description );

	void IConfigureMyActions.Configure(Spirit spirit, ActionScope actionScope) {
		actionScope.MoverFactory = new ManyMindsMover();
	}
}

class ManyMindsMover : IMoverFactory {

	// Same as default...
	public TokenMover Pusher(Spirit self, SourceSelector sourceSelector, DestinationSelector? dest = null)
		=> new TokenMover(self, "Push", sourceSelector, dest ?? DestinationSelector.Adjacent);

	// Different
	public TokenMover Gather(Spirit self, Space space) => new TokenMover(self, "Gather",
		new BeastSourceSelector(space),
		new DestinationSelector(space)
	);

	// Different
	public DestinationSelector PushDestinations => new DestinationSelector(ExtendBeastBy1);

	static Space[] ExtendBeastBy1(SpaceToken st) {
		int range = st.Token.Class == Token.Beast ? 2 : 1; // Compare Class, not Token so we get all beasts
		return st.Space.Range(range).ToArray();
	}

	class BeastSourceSelector(Space destination)
		: SourceSelector(destination.Adjacent) {
		public override SpaceToken[] GetSourceOptions() {
			var items = new List<SpaceToken>();
			foreach( var group in RemainingTypes ) {
				int range = group == Token.Beast ? 2 : 1;
				foreach( var space in _destination.Range(range) ) // gather, not Range
					items.AddRange(GetSourceOptionsOn1Space(space));
			}
			return [.. items];
		}
		readonly Space _destination = destination;
	}

}
