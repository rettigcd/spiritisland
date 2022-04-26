namespace SpiritIsland.JaggedEarth;

public class LureOfTheDeepWilderness : Spirit {

	public const string Name = "Lure of the Deep Wilderness";

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[] { HomeOfTheIslandsHeart, EnthrallTheForeignExplorersRule };

	static readonly SpecialRule HomeOfTheIslandsHeart = new SpecialRule("Home of the Island's Heart", "Your presence may only be added/moved to lands that are inland.");
	static readonly SpecialRule EnthrallTheForeignExplorersRule = new SpecialRule("Enthrall the Foreign Explorers", "For each of your presence in a land, ignore up to 2 explorer during the Ravage Step and any Ravage Action.");

	public LureOfTheDeepWilderness():base(
		new SpiritPresence(
			new PresenceTrack(Track.Energy1, Track.Energy2, Track.MoonEnergy, Track.MkEnergy(3,Element.Plant), Track.MkEnergy(4,Element.Air), Track.Energy5Reclaim1 ),
			new PresenceTrack(Track.Card1, Track.Card2, Track.AnimalEnergy, Track.Card3, Track.Card4, Track.Card5Reclaim1)
		)
		,PowerCard.For<GiftOfTheUntamedWild>()
		,PowerCard.For<PerilsOfTheDeepestIsland>()
		,PowerCard.For<SoftlyBeckonEverInward>()
		,PowerCard.For<SwallowedByTheWilderness>()
	) {
		Growth = new Growth(
			new GrowthOption(new ReclaimAll(),new GainEnergy(1)),
			new GrowthOption(new PlacePresence(4,Target.Inland))
		).Add( 
			new GrowthOptionGroup( 1,
				new GrowthOption(new Gain1Element(Element.Moon,Element.Air,Element.Plant), new GainEnergy(2)),
				new GrowthOption(new DrawPowerCard())
			)
		);

		Presence.IsValid = (s) => !s.IsOcean && !s.IsCoastal;

		InnatePowers = new InnatePower[] {
			InnatePower.For<ForsakeSocietyToChaseAfterDreams>(),
			InnatePower.For<NeverHeardFromAgain>()
		};
	}

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// Put 3 presence on your starting board: 2 in land #8, and 1 in land #7.
		Presence.PlaceOn(board[8], gameState);
		Presence.PlaceOn(board[8], gameState);
		Presence.PlaceOn(board[7], gameState);

		// Add 1 beast to land #8
		gameState.Tokens[board[8]].Beasts.Init(1);

		gameState.PreRavaging.ForGame.Add( EnthrallTheForeignExplorers );
	}

	async Task EnthrallTheForeignExplorers( RavagingEventArgs args ) {
		var ravageSpacesWithPresence = args.Spaces.Intersect(this.Presence.Spaces).ToArray();
		var selfCtx = this.Bind( args.GameState );
		foreach(var space in ravageSpacesWithPresence)
			await EntralExplorersOnSpace( selfCtx.Target(space) );
	}

	async Task EntralExplorersOnSpace( TargetSpaceCtx ctx ) {
		int maxRemovable = ctx.PresenceCount * 2;

		int explorerCount = ctx.Tokens.Sum( Invader.Explorer );

		var explorerTypes = ctx.Tokens.OfType( Invader.Explorer ).ToList();

		int removableCount = System.Math.Min( maxRemovable, explorerCount );
		int removed = 0;
		while( removed < removableCount ) {
			// Select type to not participate (strifed / non-strifed)
			var explorerTypeToNotParticipate = explorerTypes.Count == 1 ? explorerTypes[0] 
				: await ctx.Decision(Select.TokenFrom1Space.TokenToRemove(ctx.Space,1,explorerTypes.ToArray(),Present.Done));
			if( explorerTypeToNotParticipate == null ) break;

			var countToNotParticipate = await ctx.Self.SelectNumber( $"# of {explorerTypeToNotParticipate} to not participate.", ctx.Tokens[explorerTypeToNotParticipate], 0 );

			if(countToNotParticipate > 0)
				ctx.GameState.ModifyRavage( ctx.Space, cfg => cfg.NotParticipating[explorerTypeToNotParticipate] += countToNotParticipate );

			explorerTypes.Remove( explorerTypeToNotParticipate ); // don't let them select the same type twice and over-count the # of non-participants of that type.

			removed += countToNotParticipate;
		}

	}

}