using System.Collections.Generic;
using System.Linq;
using SpiritIsland.Basegame.Spirits.VitalStrength;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	/*
	================================================
	Vital Strngth of the Earth
	* reclaim, +1 presense range 2
	* +1 power card, +1 presense range 0
	* +1 presence range 1, +2 energy

	2 3 4 6 7 8
	1 1 2 2 3 4

	Innate: Gift of Strength => fast, any spirit
	1 sun 2 mountain 2 green  once this turn, target spirit may repeat 1 power card with energy cost of 1 or less
	2 sun 3 mountain 2 green  instead, the energy cost limit is 3 or less
	2 sun 4 mountain 3 green  instead, energy cost limit is 6 or less
	Special Rules: Earth's Vitality - Defend 3 in every land where you have sacred site.
	Setup: 2 in highest numbered mountain, 1 in highest numbered jungle

	Guard the Healing Land => 3 => fast, withing 1 of sacred, any => water, mountain, plant => remove 1 blight, defend 4
	A Year of Perfect Stillness => 3 => fast, range 1, any => sun, mountain => invaders skip all actions in target land this turn
	Rituals of Destruction => 3 => slow, with 1 of sacred => sun, moon, fire, plant => 2 damanage,  if target land has at least 3 dahan, then +3 damange and 2 fear
	Draw of the Fruitful Earth => 1 => slow, range 1, any => mountain, plant, animal => gather up to 2 explorers, gather up to 2 dahan

Power Progression:
	1. Rouse the Trees and Stones
	2. Call To Migrate
	3. Poisoned Land (Major; forget a Power)
	4. Devouring Ants
	5. Vigor Of The Breaking Dawn (Major; forget a Power)
	6. Voracious Growth
	7. Savage Mawbeasts

	*/

	public class VitalStrength : Spirit {

		public const string Name = "Vital Strength of Earth";
		public override string Text => Name;

		public VitalStrength():base(
			PowerCard.For<GuardTheHealingLand>(),
			PowerCard.For<AYearOfPerfectStillness>(),
			PowerCard.For<RitualsOfDestruction>(),
			PowerCard.For<DrawOfTheFruitfulEarth>()
		){
			GrowthOptions = new GrowthOption[]{
				new GrowthOption( new ReclaimAll(), new PlacePresence(2) ),
				new GrowthOption( new DrawPowerCard(), new PlacePresence(0) ),
				new GrowthOption( new GainEnergy(2), new PlacePresence(1) ),
			};

			this.giftOfStrength = new GiftOfStrength_InnatePower();
			this.InnatePowers = new InnatePower[]{ giftOfStrength };

		}

		readonly GiftOfStrength_InnatePower giftOfStrength;

		protected override int[] EnergySequence => new int[]{2, 3, 4, 6, 7, 8};
		protected override int[] CardSequence => new int[]{1, 1, 2, 2, 3, 4};

		public override void AddActionFactory(IActionFactory actionFactory) {

			if(actionFactory is DrawPowerCard){
				var newCard = PowerProgression[0];
				this.RegisterNewCard( newCard );
				PowerProgression.RemoveAt( 0 );
				if(newCard.PowerType == PowerType.Major)
					base.AddActionFactory(new ForgetPowerCard());
			} else
				base.AddActionFactory(actionFactory);
		}

		readonly List<PowerCard> PowerProgression = new List<PowerCard>{
			PowerCard.For<RouseTheTreesAndStones>(),
			PowerCard.For<CallToMigrate>(),
			PowerCard.For<PoisonedLand>(), // Major
			PowerCard.For<DevouringAnts>(),  
			PowerCard.For<VigorOfTheBreakingDawn>(),// Major
			PowerCard.For<VoraciousGrowth>(),
			PowerCard.For<SavageMawbeasts>()
		};

		public override void Initialize( Board board, GameState gameState ){
			base.Initialize( board, gameState );
			InitPresence( board );
			gameState.Ravaging += GameState_Ravaging;
			giftOfStrength.Initialize(gameState);
		}

		void InitPresence( Board board ){
			var higestJungle = board.Spaces.OrderByDescending( s => s.Label ).First( s => s.Terrain == Terrain.Jungle );
			var higestMountain = board.Spaces.OrderByDescending( s => s.Label ).First( s => s.Terrain == Terrain.Mountain );
			Presence.Add( higestMountain );
			Presence.Add( higestMountain );
			Presence.Add( higestJungle );
		}

		void GameState_Ravaging( GameState gs, Space[] ravageSpaces ) {
			foreach(var space in SacredSites.Where(ravageSpaces.Contains))
				gs.Defend( space, 3 );
		}


	}

}
