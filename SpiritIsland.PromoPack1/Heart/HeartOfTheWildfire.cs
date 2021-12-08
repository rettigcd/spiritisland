using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {

	public class HeartOfTheWildfire : Spirit {

		static HeartOfTheWildfire() {
			SpaceFilterMap.Register(ThreateningFlames.BlightAndInvaders,ctx=>ctx.HasBlight && ctx.HasInvaders);
		}

		public const string Name = "Heart of the Wildfire";

		static Track FireCard => Track.MkCard(Element.Fire);
		static Track FirePlantEnergy => new Track( "fire,plant", Element.Fire, Element.Plant ) {
			Icon = new IconDescriptor {
				 BackgroundImg = ImageNames.Coin,
				 ContentImg = ImageNames.For(Element.Fire),
				 ContentImg2 = ImageNames.For(Element.Plant)
			}
		};
		public HeartOfTheWildfire() : base( 
			new SpiritPresence(
				new PresenceTrack( Track.Energy0, Track.FireEnergy, Track.Energy1, Track.Energy2, FirePlantEnergy, Track.Energy3 ),
				new PresenceTrack( Track.Card1, FireCard, Track.Card2, Track.Card3, FireCard, Track.Card4 )
			)
			,PowerCard.For<AsphyxiatingSmoke>()
			,PowerCard.For<FlashFires>()
			,PowerCard.For<ThreateningFlames>()
			,PowerCard.For<FlamesFury>()
		) {
			InnatePowers = new InnatePower[] {
				InnatePower.For<FireStorm>(),
				InnatePower.For<TheBurnedLandRegrows>()
			};


			Growth = new(
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

		public override SpecialRule[] SpecialRules => new SpecialRule[] {
			new SpecialRule("BLAZING PRESENCE","After you add or move presence after Setup, in the land it foes to: For each fire showingo n your presence Tracks, do 1 Damage.  If 2 fire or more are showing on your presence Tracks, add 1 blight.  Push al beasts and any number of dahan.  Added blight does not destory your presence.")
		} ;

		public override async Task PlacePresence( IOption from, Space to, GameState gs ) {
			await base.PlacePresence( from, to, gs );

			int fireCount = Presence.AddElements()[Element.Fire];
			var ctx = new SpiritGameStateCtx(this,gs,Cause.Growth).Target(to);
			// For each fire showing, do 1 damage
			await ctx.DamageInvaders(fireCount);
			// if 2 fire or more are showing, add 1 blight
			if(2<=fireCount)
				await ctx.AddBlight(1);
		}
		// !!! Blight added due to Spirit Effects (Powers, Special Rules, Scenario-based Rituals, etc) does not destroy your presence. (including cascades)
		// When Destroying presence from blight, need Cause so we can tell if destoryoing it due to Ravage or something else.

		protected override void InitializeInternal( Board board, GameState gameState ) {
			// Put 3 presence and 2 blight on your starting board in the hightest-numbered Sands. 
			var space = board.Spaces.Where(x=>x.Terrain==Terrain.Sand).Last();
			for(int i=0;i<3;++i)
				Presence.PlaceOn(space,gameState);

			gameState.Tokens[space][TokenType.Blight] += 2; // Blight goes from the box, not the blight card
		}
	}



}
