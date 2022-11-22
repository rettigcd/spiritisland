namespace SpiritIsland.Basegame;

public class Thunderspeaker : Spirit {

	public const string Name = "Thunderspeaker";

	public override SpecialRule[] SpecialRules => new SpecialRule[] { SwarnToVictory, AllyOfTheDahan } ;

	static readonly SpecialRule SwarnToVictory = new SpecialRule("Sworn To Victory","After a Ravage Action destroys 1 or more Dahan, for each Dahan Destroyed, Destroy 1 of your Presence within 1.");
	static readonly SpecialRule AllyOfTheDahan = new SpecialRule("Ally of the Dahan","Your Presence may move with Dahan. (Whenever a Dahan moves from 1 of your lands to another land, you may move 1 Presence along with it.)");

	public override string Text => Name;

	public Thunderspeaker():base(
		new SpiritPresence(
			new PresenceTrack( Track.Energy1, Track.AirEnergy, Track.Energy2, Track.FireEnergy, Track.SunEnergy, Track.Energy3 ),
			new PresenceTrack( Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.CardReclaim1, Track.Card3, Track.Card4 )
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
		var spots = board.Spaces.OrderByDescending( s => gs.DahanOn(s).Count ).Take( 2 ).ToArray();
		Presence.PlaceOn( spots[0], gs );
		Presence.PlaceOn( spots[1], gs );

		// Special Rules -Ally of the Dahan - Your presense may move with dahan
		gs.Tokens.TokenMoved.ForGame.Add( new MovePresenceWithTokens( this, TokenType.Dahan ).CheckForMove );

		// Special Rules - Sworn to Victory - For each dahan stroyed by invaders ravaging a land, destroy 1 of your presense withing 1
		gs.Tokens.TokenRemoved.ForGame.Add( DestroyNearbyPresence );
	}

	async Task DestroyNearbyPresence( ITokenRemovedArgs args ) {
		if( args.Reason != RemoveReason.DestroyedInBattle ) return;
		if(args.Token.Class != TokenType.Dahan) return;

		string prompt = $"{SwarnToVictory.Title}: {args.Count} dahan destroyed. Select presence to destroy.";

		int numToDestroy = args.Count;
		Space[] options;
		Space[] Intersect() => args.Space.Range( 1 ).Select(x=>x.Space)
			.Intersect( Presence.Spaces ).ToArray();

		while(numToDestroy-->0 && (options=Intersect()).Length > 0) {
			var space = await this.Action.Decision( Select.DeployedPresence.ToDestroy( prompt, options, Present.Always ) );
			await Presence.Destroy(space, args.GameState, DestoryPresenceCause.DahanDestroyed );
		}

	}

}