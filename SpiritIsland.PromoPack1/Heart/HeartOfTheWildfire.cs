namespace SpiritIsland.PromoPack1;

public class HeartOfTheWildfire : Spirit {

	static HeartOfTheWildfire() {
		SpaceFilterMap.Register(ThreateningFlames.BlightAndInvaders,ctx=>ctx.HasBlight && ctx.HasInvaders);
	}

	public const string Name = "Heart of the Wildfire";

	static Track FirePlantEnergy => new Track( "fire,plant", Element.Fire, Element.Plant ) {
		Icon = new IconDescriptor {
				BackgroundImg = Img.Coin,
				ContentImg = Img.Token_Fire,
				ContentImg2 = Img.Token_Plant
		}
	};
	public HeartOfTheWildfire() : base( 
		new BlazingPresence()
		,PowerCard.For<AsphyxiatingSmoke>()
		,PowerCard.For<FlashFires>()
		,PowerCard.For<ThreateningFlames>()
		,PowerCard.For<FlamesFury>()
	) {
		(Presence as BlazingPresence).spirit = this;
		InnatePowers = new InnatePower[] {
			InnatePower.For<FireStorm>(),
			InnatePower.For<TheBurnedLandRegrows>()
		};


		GrowthTrack = new(
			new GrowthOption(
				new ReclaimAll(),
				new DrawPowerCard(1),
				new GainEnergy(1)
			),
			new GrowthOption(
				new DrawPowerCard(1),
				new PlacePresence(3)
			),
			new GrowthOption(
				new PlacePresence(1),
				new GainEnergy(2),
				new EnergyForFire()
			)
		);


	}

	public override string Text => Name;

	static readonly SpecialRule DestructiveNature = new SpecialRule(
		"Destructive Nature",
		"Blight added due to Spirit Effects (Powers, Special Rules, Scenario-based Rituals, etc) does not destroy your presence. (including cascades)"
		// !!!When Destroying presence from blight, need Cause so we can tell if destroyoing it due to Ravage or something else.
	);

	public override SpecialRule[] SpecialRules => new SpecialRule[] {
		BlazingPresence.Rule,
		DestructiveNature
	};

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// Put 3 presence and 2 blight on your starting board in the hightest-numbered Sands. 
		var space = board.Spaces.Last(x=>x.IsSand);
		for(int i=0;i<3;++i)
			Presence.PlaceOn(space,gameState);

		gameState.Tokens[space].Blight.Adjust(2); // Blight comes from the box, not the blight card
	}

	class BlazingPresence : SpiritPresence {

		public static SpecialRule Rule = new SpecialRule(
			"Blazing Presence",
			"After you add or move presence after Setup, in the land it goes to: For each fire showing on your presence Tracks, do 1 Damage."
			+"  If 2 fire or more are showing on your presence Tracks, add 1 blight."
			+"  Push all beasts and any number of dahan.  Added blight does not destroy your presence."
		);

		static Track FireCard => Track.MkCard( Element.Fire );

		public BlazingPresence():base(
				new PresenceTrack( Track.Energy0, Track.FireEnergy, Track.Energy1, Track.Energy2, FirePlantEnergy, Track.Energy3 ),
				new PresenceTrack( Track.Card1, FireCard, Track.Card2, Track.Card3, FireCard, Track.Card4 )
			) { }
		public HeartOfTheWildfire spirit;

		public override async Task Place( IOption from, Space to, GameState gs ) { 
			await base.Place( from, to, gs );

			// !!! There is a bug here somehow that after placeing the 2nd fire, track, still returned only 1 
			// !! maybe we need to make Elements smarter so it is easier to calculate, like breaking it into:
			//	(track elements, prepared elements, card elements)
			int fireCount = AddElements()[Element.Fire];

			var ctx = spirit.Bind( gs, Guid.NewGuid() ).Target( to );
			// For each fire showing, do 1 damage
			await ctx.DamageInvaders( fireCount );
			// if 2 fire or more are showing, add 1 blight
			if(2 <= fireCount)
				await ctx.AddBlight( 1, AddReason.SpecialRule );
		}

		public override async Task Destroy( Space space, GameState gs, DestoryPresenceCause actionType, AddReason blightAddedReason = AddReason.None ) {

			// Blight added
			if( actionType == DestoryPresenceCause.Blight
				// due to Spirit Effects (Powers, Special Rules, Scenario-based Rituals, etc)
				&& blightAddedReason != AddReason.BlightedIsland && blightAddedReason != AddReason.Ravage
			)
				// does not destroy your presence. (including cascades)"
				return;

			await base.Destroy( space, gs, actionType, blightAddedReason );
		}

	}

}