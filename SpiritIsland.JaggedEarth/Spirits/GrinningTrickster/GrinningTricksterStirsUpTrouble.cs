using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class GrinningTricksterStirsUpTrouble : Spirit {

		public const string Name = "Grinning Trickster Stirs Up Trouble";
		public override string Text => Name;

		public override SpecialRule[] SpecialRules => new SpecialRule[] { new SpecialRule("A Real Flair for Discord", "After one of your Powers adds strife in a land, you may pay 1 Energy to add 1 strife within Range-1 of that land."), new SpecialRule("Cleaning up Messes is a Drag", "After one of your Powers Removes blight, Destory 1 of your presence.  Ignore this rule for Let's See What Happens.") };

		public GrinningTricksterStirsUpTrouble()
			:base(
				new SpiritPresence(
					new PresenceTrack(Track.Energy1,Track.MoonEnergy,Track.Energy2,Track.AnyEnergy,Track.FireEnergy,Track.Energy3),
					new PresenceTrack(Track.Card2,Track.Push1Dahan,Track.Card3,Track.Card3,Track.Card4,Track.AirEnergy,Track.Card5)
				)
				,PowerCard.For<ImpersonateAuthority>()
				,PowerCard.For<InciteTheMob>()
				,PowerCard.For<OverenthusiasticArson>()
				,PowerCard.For<UnexpectedTigers>()
			)
		{
			// Growth
			this.growthOptionGroup = new GrowthOptionGroup(
				new GrowthOption(new GainEnergy(-1),new ReclaimAll(), new MovePresence() ){ GainEnergy = -1 },
				new GrowthOption(new PlacePresence(2)){ AutoSelectSingle=true },
				new GrowthOption(new DrawPowerCard()){ AutoSelectSingle=true },
				new GrowthOption(new GainEnergyEqualToCardPlays() ){ AutoSelectSingle=true }
			).Pick(2);

			// Innates
			InnatePowers = new InnatePower[] {
				InnatePower.For<LetsSeeWhatHappens>(),
				InnatePower.For<WhyDontYouAndThemFight>()
			};
		}

		protected override void InitializeInternal( Board board, GameState gameState ) {
			// Place presence on highest numbered land with dahan
			Presence.PlaceOn(board.Spaces.Where(s=>gameState.Tokens[s].Dahan.Any).Last());
			// and in land #4
			Presence.PlaceOn(board[4]);

		}

		// A Real Flair for Discord
		// After one of your Powers adds strife in a land, you may pay 1 Energy to add 1 strife within Range-1 of that land."
		public override async Task AddStrife( TargetSpaceCtx ctx, Token invader ) {
			await base.AddStrife( ctx, invader );
			if(Energy==0) return;
			var nearbyInvaders = ctx.Space.Range(1)
				.SelectMany(s=>ctx.Target(s).Tokens.Invaders().Select(t=>new SpaceToken(s,t)))
				.ToArray();
			var invader2 = await Action.Decision(new Decision.SpaceTokens("Add additional strife for 1 energy",nearbyInvaders,Present.Done));
			if(invader2 == null) return;
			Energy--;
			await base.AddStrife( ctx.Target(invader2.Space), invader2.Token);
		}

		// Cleanup Up Messes is such a drag
		public override async Task RemoveBlight( TargetSpaceCtx ctx ) {
			await base.RemoveBlight( ctx );
			Presence.Destroy( await this.Action.Decision( new Decision.Presence.DeployedToDestory( "Remove presence for blight cleanup", Presence.Spaces, Present.Always ) )); // !!! Duplicate in thunderspeaker
		}

	}

}
