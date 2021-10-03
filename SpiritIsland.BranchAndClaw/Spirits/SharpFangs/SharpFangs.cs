using System.Linq;

namespace SpiritIsland.BranchAndClaw {

	/*


	================================================
	Sharp Fangs Behind the Leaves
	(pick 2)

	 * cost -1, reclaim cards, gain +1 power card
	 * add a presense to jungle or a land with beasts (range 3)
	 * gain power card, gain +1 energy
	 * +3 energy

	1 animal plant 2 animal 3 4
	2 2 3 relaim-1 4 5&reclaim-1

	Innate - Ranging Hunt  => fast, range 1, no blight
	2 animal  you may gather 1 beast
	2 plant 3 animal  1 damange per beast
	2 animal  you may push up to 2 beast
	Innate - Frenzied Assult  => slow range 1, must have beast
	1 moon 1 fire 4 animal   1 fear and 2 damage,  remove 1 beast
	1 moon 2 fire 5 animal   +1 fear and +1 damange
	Special Rules - Ally of the Beasts - Your presensee may move with beast.  
	Call Forth Predators - During each spirit phase, you may replace 1 of your presense with 1 beast.  The replace presense leaves the game 
	Set Up - put 1 presense and 1 beast in hightest numbered jungle.  Put 1 presense in a land of your choice with beast anywhere on the island.

	Prey on the Builders => 1 => fast, range 0, any => moon, fire, animal => you may gather 1 beast.  If target land has beast, invaders do not ubild there this turn.
	Teeth Gleam From Darkness => 1 => slow, withing 1 of presense in jungle, no blight => moon, plant, animal => (1 fear.  Add 1 beast) -OR- If target land has both beast and invaders: 3 fear
	Too Near the Jungle => 0 => slow, within 1 of presense in jungle, any => plant, animal => 1 fear. destroy 1 dahan
	Terrifying Chase => 1 => slow, range 0, any => sun, animal => Push 2 explorer/town/dahan.  Push another 2 explorer/down/dahan per beast in target land.  If you pushed any invaders, 2 fear


	 */
	public class SharpFangs : Spirit {

		public const string Name = "Sharp Fangs Behind the Leaves";

		public override string SpecialRules => "Ally of the Beasts - Your presensee may move with beast.";

		public override string Text => Name;

		static Track FivePlaysReclaim1() {
			var track = Track.MkCard(5);
			track.ReclaimOne = true;
			return track;
		}

		public SharpFangs():base(
			new MyPresence(
				new Track[] { Track.Energy1, Track.AnimalEnergy, Track.PlantEnergy, Track.Energy2, Track.AnimalEnergy, Track.Energy3, Track.Energy4 },
				new Track[] { Track.Card2, Track.Card2, Track.Card3, Track.Reclaim1, Track.Card4, FivePlaysReclaim1() }
			),
			PowerCard.For<PreyOnTheBuilders>(),
			PowerCard.For<TeethGleamFromDarkness>(),
			PowerCard.For<TerrifyingChase>(),
			PowerCard.For<TooNearTheJungle>()
		) {
		
			var beastOrJungleRange3 = new PlacePresence(3, Target.BeastOrJungle);


			GrowthOptions = new GrowthOption[]{
				new GrowthOption( new ReclaimAll(), new GainEnergy(-1), new DrawPowerCard(1) ){ GainEnergy=-1 },
				new GrowthOption( beastOrJungleRange3 ),
				new GrowthOption( new DrawPowerCard(1), new GainEnergy(1) ){ GainEnergy = 1 },
				new GrowthOption( new GainEnergy(3) ){ GainEnergy = 3 }
			};
			this.growthOptionSelectionCount = 2;

			this.InnatePowers = new InnatePower[] {
				InnatePower.For<FrenziedAssult>(),
				InnatePower.For<RagingHunt>(),
			};

		}

		public override void Grow( GameState gameState, int optionIndex ) {

			var (growthOptions,_) = this.GetGrowthOptions();

			var actions = growthOptions[optionIndex].GrowthActions;
			foreach(var action in actions.Take(5))
				AddActionFactory( action );

			AddActionFactory( new ReplacePresenceWithBeast() );

		}

		protected override void InitializeInternal( Board board, GameState gs ) {
			var highestJungle = board.Spaces.Where(x=>x.Terrain == Terrain.Jungle).Last();
			Presence.PlaceOn(highestJungle);
			gs.Tokens[highestJungle].Beasts().Count++;

			// init special growth (note - we don't want this growth in Unit tests, so only add it if we call InitializeInternal())
			this.AddActionFactory(new Setup_PlacePresenceOnBeastLand());

			var x = new SpiritIsland.MovePresenceWithTokens( this, "Move presence with beast?", BacTokens.Beast.Generic );
			gs.Tokens.TokenMoved.ForEntireGame( x.CheckForMove );
		}

	}

}
