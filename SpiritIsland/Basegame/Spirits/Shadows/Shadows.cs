
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	/*
	=================================================
	Shadows Flicker Like Flame
	* reclaim, gain power Card
	* gain power card, add a presense range 1
	* add a presence withing 3, +3 energy

	0 1 3 4 5 6
	1 2 3 3 4 5

	Innate: Darness Swallows the Unwary => fast, 1 from sacred, any
	2 moon, 1 fire	gather 1 explorr
	3 moon, 2 fire  destroy up to 2 explorer, 1 feer per explorer destoyed
	4 moon, 3 fire 2 air   3 damage, 1 feer per invador destroyed by this damange
	Special Rules: Shadows of the Dahan - Whenever you use a power, you may pay 1 energy to target land with Dahan regardless of range
	Setup: 2 in highest # jungle and 1 in #5

	Mantle of Dread => 1 => slow, target any spirit => moon fire air => 2 Fear.  Target Spirit may push 1 explorer and 1 town from a land where it has presense
	Favors Called Due => 1 => slow, range 1, any => moon air animal => Gather up to 4 dahan.  If invaders are present and dahan now outnumber them, then 3 fear
	Crops Wither and Fade => 1 => slow, range 1, any => moon, fire, plant => 2 fear.  (Replace 1 down with 1 explorer  -OR- Replace 1 city with 1 town)
	Conceiling Shadows => 0 => fast, range 0, any => moon air => 1 fear, dahan take no damange from ravaging invaders this turn

Shadows Flicker like Flame:
1. Dark and Tangled Woods
2. Shadows of the Burning Forest
3. The Jungle Hungers (Major; forget a Power)
4. Land of Haunts and Embers
5. Terrifying Nightmares (Major; forget a Power)
6. Call of the Dahan Ways
7. Visions of Fiery Doom


	*/

	public class Shadows : Spirit {
		public override string Text => "Shadows Flicker Like Flames";

		public Shadows():base(
			PowerCard.For<MantleOfDread>(),
			PowerCard.For<FavorsCalledDue>(),
			PowerCard.For<CropsWitherAndFade>(),
			PowerCard.For<ConcealingShadows>()
		) {
			GrowthOptions = new GrowthOption[]{
				new GrowthOption( new ReclaimAll(), new DrawPowerCard(1) ),
				new GrowthOption( new DrawPowerCard(1), new PlacePresence(1) ),
				new GrowthOption( new PlacePresence(3), new GainEnergy(3) )
			};
			this.InnatePowers = new InnatePower[]{
				InnatePower.For<DarknessSwallowsTheUnwary>()
			};
		}

		protected override int[] EnergySequence => new int[]{0, 1, 3, 4, 5, 6 };
		protected override int[] CardSequence => new int[]{1, 2, 3, 3, 4, 5 };

		public override void AddActionFactory(IActionFactory actionFactory) {

			if(actionFactory is DrawPowerCard){
				var newCard = PowerProgression[0];
				this.Hand.Add( newCard );
				PowerProgression.RemoveAt( 0 );
				if(newCard.PowerType == PowerType.Major)
					base.AddActionFactory(new ForgetPowerCard());
			} else
				base.AddActionFactory(actionFactory);
		}

		readonly List<PowerCard> PowerProgression = new List<PowerCard>{
			PowerCard.For<DarkAndTangledWoods>(),
			PowerCard.For<ShadowsOfTheBurningForest>(),
			PowerCard.For<TheJungleHungers>(), // Major
			PowerCard.For<LandOfHauntsAndEmbers>(),
			PowerCard.For<TerrifyingNightmares>(),// Major
			PowerCard.For<CallOfTheDahanWays>(),
			PowerCard.For<VisionsOfFieryDoom>()
		};

		public override PowerCardApi GetPowerCardApi( ActionEngine engine ) {
			return new ShadowApi(engine);
		}

		class ShadowApi : PowerCardApi {
			public ShadowApi(ActionEngine engine):base(engine){ }

			public override async Task<Space> TargetSpace( IEnumerable<Space> source, int range, Func<Space, bool> filter = null ) {
				// no money, do normal
				if(engine.Self.Energy == 0)
					return await base.TargetSpace( source, range, filter );

				// find normal Targetable spaces
				var normalSpaces = source.Range(range).ToArray();
				if(filter!=null) normalSpaces = normalSpaces.Where(filter).ToArray();

				// find dahan-only spaces that are not in targetable spaces
				var dahanOnlySpaces = engine.GameState.Island.Boards
					.SelectMany(board=>board.Spaces)
					.Where(engine.GameState.HasDahan)
					.Except(normalSpaces)
					.ToArray();
				// no dahan-only spaces, do normal
				if(dahanOnlySpaces.Length == 0)
					return await base.TargetSpace( source, range, filter );

				// append Target-Dahan option to end of list
				var options = normalSpaces.Cast<IOption>().ToList();
				options.Add(new TextOption("Pay 1 energy to target land with dahan"));
				var option = await engine.SelectOption("Select target.",options.ToArray());

				// if they select regular space, use it
				if(option is Space space)
					return space;

				// pay 1 energy
				--engine.Self.Energy;

				// pick from dahan-only spaces
				return await engine.SelectSpace("Target land with dahan",dahanOnlySpaces);
			}

		}

		public override void Initialize( Board board, GameState _1 ) {

			// !!! extract this so we can reuse it between Shadow and Earth
			var higestJungle = board.Spaces.OrderByDescending(s=>s.Label).First(s=>s.Terrain==Terrain.Jungle);

			this.Presence.Add( higestJungle );
			this.Presence.Add( higestJungle );
			this.Presence.Add( board[5] );
			// !!! unit tests for this
		}

	}

}
