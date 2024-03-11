namespace SpiritIsland.Basegame;

public class Thunderspeaker : Spirit {

	public const string Name = "Thunderspeaker";

	public override SpecialRule[] SpecialRules => new SpecialRule[] { SwarnToVictory, AllyOfTheDahan } ;

	static readonly SpecialRule SwarnToVictory = new SpecialRule("Sworn To Victory","After a Ravage Action destroys 1 or more Dahan, for each Dahan Destroyed, Destroy 1 of your Presence within 1.");
	static readonly SpecialRule AllyOfTheDahan = new SpecialRule("Ally of the Dahan","Your Presence may move with Dahan. (Whenever a Dahan moves from 1 of your lands to another land, you may move 1 Presence along with it.)");

	public override string Text => Name;

	public Thunderspeaker():base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack( Track.Energy1, Track.AirEnergy, Track.Energy2, Track.FireEnergy, Track.SunEnergy, Track.Energy3 ),
			new PresenceTrack( Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.CardReclaim1, Track.Card3, Track.Card4 ),
			new FollowingPresenceToken( spirit, Human.Dahan )
		),
		new GrowthTrack(
			new GrowthGroup(
				new ReclaimAll(),
				new GainPowerCard(),
				new GainPowerCard()
			),
			new GrowthGroup(
				new PlacePresence( 2, Filter.Dahan ),
				new PlacePresence( 1, Filter.Dahan )
			),
			new GrowthGroup(
				new PlacePresence( 1 ),
				new GainEnergy( 4 )
			)
		),
		PowerCard.For(typeof(ManifestationOfPowerAndGlory)),
		PowerCard.For(typeof(SuddenAmbush)),
		PowerCard.For(typeof(VoiceOfThunder)),
		PowerCard.For(typeof(WordsOfWarning))
	) {

		InnatePowers = [
			InnatePower.For(typeof(GatherTheWarriors)),
			InnatePower.For(typeof(LeadTheFuriousAssult)),
		];

	}

	protected override void InitializeInternal( Board board, GameState gs ) {
		// Put 2 Presence on your starting board: 1 in each of the 2 lands with the most Dahan
		Space[] mostDahanSpots = board.Spaces.OrderByDescending( s => s.ScopeTokens.Dahan.CountAll ).Take( 2 ).ToArray();
		mostDahanSpots[0].ScopeTokens.Setup(Presence.Token, 1);
		mostDahanSpots[1].ScopeTokens.Setup(Presence.Token, 1);

		// Special Rules - Sworn to Victory - For each dahan stroyed by invaders ravaging a land, destroy 1 of your presense within 1
		gs.AddIslandMod( new TokenRemovedHandlerAsync_Persistent( DestroyNearbyPresence ) );


	}

	async Task DestroyNearbyPresence( ITokenRemovedArgs args ) {
		if( args.Reason != RemoveReason.Destroyed ) return;
		if( ActionScope.Current.Category != ActionCategory.Invader) return;
		if(args.Removed.Class != Human.Dahan) return;

		string prompt = $"{SwarnToVictory.Title}: {args.Count} dahan destroyed. Select presence to destroy.";

		int numToDestroy = args.Count;
		Space from = (Space)args.From;
		SpaceState fromTokens = from.ScopeTokens;
		var spaces = from.Range(1).IsInPlay().ToHashSet();
		SpaceToken[] options;
		SpaceToken[] Intersect() => Presence.Deployed.Where(x=>spaces.Contains(x.Space)).ToArray(); // Ravage Only, not dependent on PowerRangeCalculator

		while(numToDestroy-->0 && (options=Intersect()).Length > 0) {
			SpaceToken spaceToken = await SelectAsync( new A.SpaceToken( prompt, options, Present.Always ) );
			await spaceToken.Destroy();
		}

	}

}