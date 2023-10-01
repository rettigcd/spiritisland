namespace SpiritIsland.Basegame;

public class Thunderspeaker : Spirit {

	public const string Name = "Thunderspeaker";

	public override SpecialRule[] SpecialRules => new SpecialRule[] { SwarnToVictory, AllyOfTheDahan } ;

	static readonly SpecialRule SwarnToVictory = new SpecialRule("Sworn To Victory","After a Ravage Action destroys 1 or more Dahan, for each Dahan Destroyed, Destroy 1 of your Presence within 1.");
	static readonly SpecialRule AllyOfTheDahan = new SpecialRule("Ally of the Dahan","Your Presence may move with Dahan. (Whenever a Dahan moves from 1 of your lands to another land, you may move 1 Presence along with it.)");

	public override string Text => Name;

	public Thunderspeaker():base(
		new FollowingPresence(
			new PresenceTrack( Track.Energy1, Track.AirEnergy, Track.Energy2, Track.FireEnergy, Track.SunEnergy, Track.Energy3 ),
			new PresenceTrack( Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.CardReclaim1, Track.Card3, Track.Card4 ),
			Human.Dahan
		),
		PowerCard.For<ManifestationOfPowerAndGlory>(),
		PowerCard.For<SuddenAmbush>(),
		PowerCard.For<VoiceOfThunder>(),
		PowerCard.For<WordsOfWarning>()
	) {
		GrowthTrack = new(
			new GrowthOption( 
				new ReclaimAll(), 
				new DrawPowerCard(1),
				new DrawPowerCard(1)
			),
			new GrowthOption( 
				new PlacePresence(2,Target.Dahan),
				new PlacePresence(1,Target.Dahan)
			),
			new GrowthOption( 
				new PlacePresence(1), 
				new GainEnergy(4)
			)
		);

		this.InnatePowers = new InnatePower[]{
			InnatePower.For<GatherTheWarriors>(),
			InnatePower.For<LeadTheFuriousAssult>(),
		};

	}

	protected override void InitializeInternal( Board board, GameState gs ) {
		// Put 2 Presence on your starting board: 1 in each of the 2 lands with the most Dahan
		Space[] mostDahanSpots = board.Spaces.OrderByDescending( s => s.Tokens.Dahan.CountAll ).Take( 2 ).ToArray();
		mostDahanSpots[0].Tokens.Adjust(Presence.Token, 1);
		mostDahanSpots[1].Tokens.Adjust(Presence.Token, 1);

		// Special Rules - Sworn to Victory - For each dahan stroyed by invaders ravaging a land, destroy 1 of your presense within 1
		gs.AddIslandMod( new TokenRemovedHandlerAsync_Persistent( DestroyNearbyPresence ) );


	}

	async Task DestroyNearbyPresence( ITokenRemovedArgs args ) {
		if( args.Reason != RemoveReason.Destroyed ) return;
		if( ActionScope.Current.Category != ActionCategory.Invader) return;
		if(args.Removed.Class != Human.Dahan) return;

		string prompt = $"{SwarnToVictory.Title}: {args.Count} dahan destroyed. Select presence to destroy.";

		int numToDestroy = args.Count;
		var spaces = args.From.InOrAdjacentTo.Select(x=>x.Space).ToHashSet();
		SpaceToken[] options;
		SpaceToken[] Intersect() => Presence.Deployed.Where(x=>spaces.Contains(x.Space)).ToArray(); // Ravage Only, not dependent on PowerRangeCalculator

		while(numToDestroy-->0 && (options=Intersect()).Length > 0) {
			SpaceToken spaceToken = await this.Gateway.Decision( new Select.ASpaceToken( prompt, options, Present.Always ) );
			await spaceToken.Destroy();
		}

	}

}