using SpiritIsland.Select;
using System.Buffers.Text;

namespace SpiritIsland.FeatherAndFlame;

public class HeartOfTheWildfire : Spirit {

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
		// in the hightest-numbered Sands on your starting board
		var space = board.Spaces.Last(x=>x.IsSand);
		var spaceState = gameState.Tokens[space];
		// Put 3 presence
		spaceState.Adjust(Presence.Token,3);
		// and 2 blight
		spaceState.Blight.Adjust(2); // Blight comes from the box, not the blight card
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

		public override void SetSpirit( Spirit spirit ) { 
			base.SetSpirit( spirit );
			Token = new WildfireToken( spirit );
		}

	}

	class WildfireToken : SpiritPresenceToken, IHandleRemovingToken, IHandleTokenAdded {

		public WildfireToken( Spirit spirit ):base(spirit) {}

		public Task ModifyRemoving( RemovingTokenArgs args ) {
			// Blight added due to Spirit effects( Powers, Special Rules, Scenario-based Rituals, etc) does not destroy your Presence. ( This includes cascades.)
			if( DestroysPresence(args) && BlightAddedDueToSpiritEffects() ){
				if(args.Mode == RemoveMode.Live)
					GameState.Current.Log(new Log.Debug($"Blight added due do Spirit effects does not destroy Wildfire presence."));
				args.Count = 0;

			}
			return Task.CompletedTask;
		}

		static bool BlightAddedDueToSpiritEffects() => !BlightTokenBinding.GetAddReason()
			.IsOneOf( AddReason.Ravage, AddReason.BlightedIsland, AddReason.None );

		public async Task HandleTokenAdded( ITokenAddedArgs args ) {
			// !! There is a bug here somehow that after placing the 2nd fire, track, still returned only 1 
			// !! maybe we need to make Elements smarter so it is easier to calculate, like breaking it into:
			//	(track elements, prepared elements, card elements)
			int fireCount = _spirit.Presence.TrackElements[Element.Fire];
			var ctx = _spirit.BindSelf().Target( args.AddedTo );
			// For each fire showing, do 1 damage
			await ctx.DamageInvaders( fireCount );
			// if 2 fire or more are showing, add 1 blight
			if(2 <= fireCount)
				await ctx.AddBlight( 1, AddReason.SpecialRule );
		}
	}


}
