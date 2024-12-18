namespace SpiritIsland.FeatherAndFlame;

public class HeartOfTheWildfire : Spirit {

	public const string Name = "Heart of the Wildfire";

	public override string SpiritName => Name;

	static Track FireAndPlantEnergy => new Track( "fire,plant", Element.Fire, Element.Plant ) {
		Icon = new IconDescriptor {
				BackgroundImg = Img.Coin,
				ContentImg = Img.Token_Fire,
				ContentImg2 = Img.Token_Plant
		}
	};
	public HeartOfTheWildfire() : base( 
		spirit => new SpiritPresence( spirit,
			new PresenceTrack( Track.Energy0, Track.FireEnergy, Track.Energy1, Track.Energy2, FireAndPlantEnergy, Track.Energy3 ),
			new PresenceTrack( Track.Card1, FireCard, Track.Card2, Track.Card3, FireCard, Track.Card4 ),
			new WildfireToken( spirit )
		),
		new GrowthTrack(
			new GrowthGroup(
				new ReclaimAll(),
				new GainPowerCard(),
				new GainEnergy( 1 )
			),
			new GrowthGroup(
				new GainPowerCard(),
				new PlacePresence( 3 )
			),
			new GrowthGroup(
				new PlacePresence( 1 ),
				new GainEnergy( 2 ),
				new EnergyForFire()
			)
		)
		,PowerCard.ForDecorated(AsphyxiatingSmoke.ActAsync)
		,PowerCard.ForDecorated(FlashFires.ActAsync)
		,PowerCard.ForDecorated(ThreateningFlames.ActAsync)
		,PowerCard.ForDecorated(FlamesFury.ActAsync)
	) {

		InnatePowers = [
			InnatePower.For(typeof(FireStorm)),
			InnatePower.For(typeof(TheBurnedLandRegrows))
		];

		SpecialRules = [
			BlazingPresence_Rule,
			WildfireToken.DestructiveNature_Rule
		];
	}

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// in the hightest-numbered Sands on your starting board
		var space = board.Spaces.Last(x => x.IsSand).ScopeSpace;
		// Put 3 presence
		space.Setup(Presence.Token,3);
		// and 2 blight
		space.Blight.Adjust(2); // Blight comes from the box, not the blight card
	}

	static Track FireCard => Track.MkCard( Element.Fire );

	public static SpecialRule BlazingPresence_Rule => new SpecialRule(
		"Blazing Presence",
		"After you add or move presence after Setup, in the land it goes to: For each fire showing on your presence Tracks, do 1 Damage."
		+"  If 2 fire or more are showing on your presence Tracks, add 1 blight."
		+"  Push all beasts and any number of dahan.  Added blight does not destroy your presence."
	);

	class WildfireToken( Spirit spirit ) : SpiritPresenceToken(spirit)
		, IModifyRemovingToken
		, IHandleTokenAdded
	{

		static public readonly SpecialRule DestructiveNature_Rule = new SpecialRule(
			"Destructive Nature",
			"Blight added due to Spirit Effects (Powers, Special Rules, Scenario-based Rituals, etc) does not destroy your presence. (including cascades)"
		);

		Task IModifyRemovingToken.ModifyRemovingAsync( RemovingTokenArgs args ) {
			// Blight added due to Spirit effects( Powers, Special Rules, Scenario-based Rituals, etc) does not destroy your Presence. ( This includes cascades.)
			var addedBlight = BlightToken.ScopeConfig.BlightFromCardTrigger;
			if( DestroysMyPresence(args) 
				&& (addedBlight is null || !addedBlight.Reason.IsOneOf(AddReason.Ravage, AddReason.BlightedIsland, AddReason.None))
			) {
				ActionScope.Current.Log(new Log.Debug($"Blight added due do Spirit effects does not destroy Wildfire presence."));
				args.Count = 0;
			}
			return Task.CompletedTask;
		}

		public async Task HandleTokenAddedAsync( Space to, ITokenAddedArgs args ) {
			if(args.Added != this) return;
			// !! There is a bug here somehow that after placing the 2nd fire, track, still returned only 1 
			// !! maybe we need to make Elements smarter so it is easier to calculate, like breaking it into:
			//	(track elements, prepared elements, card elements)
			int fireCount = Self.Presence.TrackElements[Element.Fire];
			var ctx = Self.Target( to.SpaceSpec );
			// For each fire showing, do 1 damage
			await ctx.DamageInvaders( fireCount );
			// if 2 fire or more are showing, add 1 blight
			if(2 <= fireCount)
				await ctx.AddBlight( 1, AddReason.SpecialRule );
		}
	}


}
