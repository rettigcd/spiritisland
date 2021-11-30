using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	// Slow And Silent Death

	public class ShroudOfSilentMist : Spirit {

		public const string Name = "Shroud of Silent Mist";

		public override string Text => Name;

		static readonly SpecialRule GatherPowerFromTheCoolAndDark = new SpecialRule(
			"Gather Power from the Cool and Dark",
			"Once a turn, when you Gain a Power Card without fire, gain 1 Energy"
		);

		public static Track MovePresence => new Track( "Moveonepresence.png" ){ Action=new MovePresence() };


		public override SpecialRule[] SpecialRules => new SpecialRule[]{ 
			GatherPowerFromTheCoolAndDark, 
			MistsShiftAndFlow.Rule, 
			SlowAndSilentDeathHealer.Rule,
		};

		public ShroudOfSilentMist():base(new SpiritPresence(
				new PresenceTrack(Track.Energy0,Track.Energy1,Track.WaterEnergy,Track.Energy2,Track.AirEnergy),
				new PresenceTrack(Track.Card1,Track.Card2,MovePresence,Track.MoonEnergy,Track.Card3,Track.Card4,Track.Reclaim1,Track.Card5)
			)
			,PowerCard.For<FlowingAndSilentFormsDartBy>()
			,PowerCard.For<UnnervingPall>()
			,PowerCard.For<DissolvingVapors>()
			,PowerCard.For<TheFogClosesIn>()
		) {
			this.growthOptionGroup = new GrowthOptionGroup(
				new GrowthOption( new ReclaimAll(), new DrawPowerCard() ),
				new GrowthOption( new PlacePresence(0), new PlacePresence(0) ),
				new GrowthOption( new DrawPowerCard(), new PlacePresence(3,Target.MountainOrWetland) )
			);


			this.InnatePowers = new InnatePower[] {
				InnatePower.For<SuffocatingShroud>(),
				InnatePower.For<LostInTheSwirlingHaze>()
			};
		}

		bool gainedCoolEnergyThisTurn = false;

		protected override void InitializeInternal( Board board, GameState gameState ) {

			gameState.Healer = new SlowAndSilentDeathHealer(this);

			// Place presence in:
			// (a) Highest # mountains,
			Presence.PlaceOn(board.Spaces.Where(s=>s.Terrain==Terrain.Mountain).Last(), gameState);
			// (b) highest # wetlands
			Presence.PlaceOn(board.Spaces.Where(s=>s.Terrain==Terrain.Wetland).Last(), gameState);

			gameState.TimePasses_WholeGame += GameState_TimePasses_WholeGame;
		}

		void GameState_TimePasses_WholeGame( GameState gs ) {
			gainedCoolEnergyThisTurn = false;

			bool SpaceHasDamagedInvaders( Space s ) => gs.Tokens[s].Invaders()
				.Any( i=>i.Health<i.FullHealth );

			// During Time Passes:
			int myLandsWithDamagedInvaders = Presence.Spaces.Count( SpaceHasDamagedInvaders );

			// 1 fear (max 5) per land of yours with Damaged Invaders.
			gs.Fear.AddDirect(new FearArgs { cause = Cause.None, count = Math.Min(5,myLandsWithDamagedInvaders) } );

			// Gain 1 Energy per 3 lands of yours with Damaged Invaders."
			Energy += (myLandsWithDamagedInvaders / 3);
		}

		#region Draw Cards (Gather from the Cool And Dark)

		public override async Task<PowerCard> Draw( GameState gameState, Func<List<PowerCard>, Task> handleNotUsed ) {
			var card = await base.Draw( gameState, handleNotUsed );
			CheckForCoolEnergy( card );
			return card;
		}

		public override async Task<PowerCard> DrawMajor( GameState gameState, int numberToDraw = 4, bool forgetCard = true ) {
			var card = await base.DrawMajor( gameState, numberToDraw, forgetCard );
			CheckForCoolEnergy( card );
			return card;
		}

		public override async Task<PowerCard> DrawMinor( GameState gameState ) {
			var card = await base.DrawMinor( gameState );
			CheckForCoolEnergy( card );
			return card;
		}
		void CheckForCoolEnergy(PowerCard card ) {
			if(gainedCoolEnergyThisTurn) return;
			if(card.Elements[Element.Fire]>0) return;
			Energy++;
			gainedCoolEnergyThisTurn = true;
		}

		#endregion

		public override Task<Space> TargetsSpace( GameState gameState, string prompt, From from, Terrain? sourceTerrain, int range, string filterEnum, TargettingFrom powerType ) {
			return new MistsShiftAndFlow(this,gameState,prompt,from,sourceTerrain,range,filterEnum,powerType)
				.TargetAndFlow();
		}

	}

	class SlowAndSilentDeathHealer : Healer {

		readonly ShroudOfSilentMist spirit;

		public SlowAndSilentDeathHealer(ShroudOfSilentMist spirit ) { this.spirit = spirit; }

		public static readonly SpecialRule Rule = new SpecialRule(
			"Slow and Silent Death",
			"Invaders and dahan in your lands don't heal Damage.  During Time PAsses: 1 fear (max 5) per land of yours with Damaged Invaders.  Gain 1 Energy per 3 lands of yours with Damaged Invaders."
		);


		public override void Heal( GameState gs ) {
			foreach(var space in gs.Tokens.Keys) {
				// Invaders and dahan in your lands don't heal Damage.
				if(!spirit.Presence.IsOn(space))
					InvaderGroup.HealTokens( gs.Tokens[space] );
			}

		}

	}

}
