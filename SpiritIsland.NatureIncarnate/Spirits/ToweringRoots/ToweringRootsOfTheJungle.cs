namespace SpiritIsland.NatureIncarnate;

public class ToweringRootsOfTheJungle : Spirit {

	public const string Name = "Towering Roots of the Jungle";

	// 3 Innates - stub
	// 4 Cards -stub
	// 5 Incarna
		// Range from Incarna
	// 6 Growth - finish
	// 6 Innates - finish
	// 7 Cards - finish
	// 8 empowered incarna
	// 9 incarna acts as a presence
	public ToweringRootsOfTheJungle() : base(
		new SpiritPresence(
			new PresenceTrack( Track.Energy0, Track.Energy2, Track.EarthEnergy, Track.Energy4, Track.PlantEnergy, Track.Energy6 ),
			new PresenceTrack( Track.Card1, Track.Card2, Track.SunEnergy, Track.Card3, Track.PlantEnergy, Track.Card4 )
		)
		, PowerCard.For<EntwineTheFatesOfAll>()
		, PowerCard.For<RadiantAndHallowedGrove>()
		, PowerCard.For<BloomingOfTheRocksAndTrees>()
		, PowerCard.For<BoonOfResilientPower>()
	) {
		// Growth
		GrowthTrack = new GrowthTrack(
			new GrowthOption( new ReclaimAll(), new PlacePresence(0) ),
			new GrowthOption( new DrawPowerCard(), new PlacePresence(1), new AddVitalityToIncarna() ),
			new GrowthOption( new DrawPowerCard(), new PlacePresence(3), new ReplacePresenceWithIncarna(), new GainEnergy(1) )
		);

		// Innates
		InnatePowers = new InnatePower[] {
			InnatePower.For<ShelterUnderToweringBranches>(),
			InnatePower.For<RevokeSanctuaryAndCastOut>()
		};
	}

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[]{
		new SpecialRule("Enduring Vitality", "Some of your Actions Add Vitality Tokens."),
		new SpecialRule("Heart-Tree Guards the Land", "You have an Incarna. Your Powers get +1 range if Incarna is in the origin land.  Invaders/Dahan/Beast can't be damaged or destroyed at Incarna.  Empower Encarna the first time it's in a land with 3 or more vitality.  Skip all Build Actions at empowered Incarna.")
	};

	protected override void InitializeInternal( Board board, GameState gameState ) {
		var highestFirst = board.Spaces.Reverse().Tokens().ToArray();
		// 1 in highest-numbered jungle without blight
		SpaceState jungle = highestFirst.First(x=>x.Space.IsJungle && !x.Blight.Any);
		jungle.Init(Presence.Token,1);
		// 1 in the highest-numbered mountain
		highestFirst
			.First( x => x.Space.IsMountain )
			.Init( Presence.Token, 1 );
		// 1 in the highetst numbered wetland
		highestFirst
			.First( x => x.Space.IsWetland )
			.Init( Presence.Token, 1 );
		// Incarna goes in the jungle with presence
		jungle.Init(Incarna,1); 
		Incarna.Space = jungle.Space;

	}

	public ToweringRootsIncarna Incarna = new ToweringRootsIncarna();
}

public class ToweringRootsIncarna : IToken, IEntityClass, IHandleTokenAdded, IHandleTokenRemoved  {
	public Img Img => Img.TRotJ_Incarna;

	public IEntityClass Class => this;

	public string Text => "#";

	#region IEntityClass properties
	public string Label => "My incarna???";

	public TokenCategory Category => TokenCategory.Incarna;

	#endregion

	#region tracking location
	public SpaceState? Space { get; set; }

	public void HandleTokenAdded( ITokenAddedArgs args ) {
		TrackAdding( args );
		if( args.Added == Token.Vitality && args.To[Token.Vitality] == 3) {
			// Do Power-Up here
		}
	}

	protected void TrackAdding( ITokenAddedArgs args ) {
		if(Equals( args.Added )) {
			if(Space != null) throw new InvalidOperationException( "Must first remove token before adding it to another space." );
			Space = args.To;
		}
	}

	public void HandleTokenRemoved( ITokenRemovedArgs args ) {
		if(!Equals( args.Removed )) return;
		if(Space == null) throw new InvalidOperationException( "Can't remove.  Space is already null." );
		Space = null;
	}
	#endregion
}

public class AddVitalityToIncarna : GrowthActionFactory {
	public override async Task ActivateAsync( SelfCtx ctx ) {
		if(ctx.Self is ToweringRootsOfTheJungle roots && roots.Incarna.Space is not null)
			await roots.Incarna.Space.Add(Token.Vitality,1);
	}
}

public class ReplacePresenceWithIncarna : GrowthActionFactory {
	public override async Task ActivateAsync( SelfCtx ctx ) {
		Space space = await ctx.Self.SelectDeployedPresence("Select presence to replace with Incarna.");
		var incarna = ((ToweringRootsOfTheJungle)ctx.Self).Incarna;
		if(incarna.Space != null)
			await incarna.Space.Remove( incarna, 1 );

		await space.Tokens.Add(incarna,1);
	}
}