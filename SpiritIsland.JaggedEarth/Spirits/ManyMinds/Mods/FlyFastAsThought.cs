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
		new GatherBeastSourceSelector(space),
		new DestinationSelector(space)
	);

	// Different
	public DestinationSelector PushDestinations => new DestinationSelector(ExtendBeastBy1);

	static Space[] ExtendBeastBy1(SpaceToken st) {
		int range = st.Token.Class == Token.Beast ? 2 : 1; // Compare Class, not Token so we get all beasts
		return st.Space.Range(range).ToArray();
	}

	// to ONLY be used for "Gather"
	class GatherBeastSourceSelector(Space destination) : SourceSelector(Adjacent2(destination))
	{
		// Extend range to 2
		static IEnumerable<Space> Adjacent2(Space space) {
			Dictionary<Space, int> distances = space.CalcDistances(2);
			distances.Remove(space);
			return distances.Keys;
		}

		public override SpaceToken[] GetSourceOptions() {
			var adj = new HashSet<Space>(_destination.Adjacent);
			return base.GetSourceOptions()
				// limit everything but beasts to Adjacent
				.Where(t=>t.Token.HasTag(Token.Beast) || adj.Contains(t.Space) )
				.ToArray();
		}
		readonly Space _destination = destination;
	}

}
