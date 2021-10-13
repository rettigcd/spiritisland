using System.Linq;

namespace SpiritIsland.BranchAndClaw {

	/*


	==========================================
	Keeper of the Forbidden Wilds
	(pick 2)
	* reclaim, +1 energy
	* +1 power card
	* add presense range 3 containing wilds or presense, +1 energy
	* -3 energy, +1 power card, add presense to land without blight range 3

	2 sun 4 5 plant 7 8 9
	1 2 2 3 4 5

	Innate - Punish Those Who Trespass => slow, range 0, any
	2 sun 1 fire 2 plant    2 damange.  destroy 1 dahan
	2 sun 2 fire 3 plant   +1 damange per sun-plant you have
	4 plant    you may split this powers damange however desired between target land and one of your lands

	Innate - Spreading Wilds  => slow, range 1, no blight
	2 sun    push 1 explorer from target land per 2 sun
	1 plant   if target land has no explorers, add 1 hairy snail
	3 plant  this power has +1 range
	1 air    this power has +1 range

	Special Rules - Forbidden Ground - any time you create a sacred site, push all dahan from that land.  Dahan events never move dahan to you sacred site but powers can do so.
	Set Up - put 1 presense and 1 hairy snail on your starting board in the highest numbered jungle

	Boon of Growing Power => 1 => slow, any spirit => sun, moon, plant => target spirit gains a power card.  If you target another spirit, they also gain 1 energy
	Towering Wrath => 3 => slow, withing 1 of S.S., any => sun, fire, plant => 2 fear.  For each your SS in or adjacent to target land, 2 damage.  Destroy all Dahan
	Regrow from Roots => 1 => slow, range 1, jungle or wetlands => water, rock, plant => if there are 2 blight or fewer in target land, remove 1 blight
	Sacrosanct Wilderness => 2 => fast, range 1, no blight => sun, rock, plant => push 2 dahan.  (2 damange per hairy snail in target land. -OR- add 1 hairy snail)

	 */

	public class Keeper : Spirit {

		public const string Name = "Keeper of the Forbidden Wilds";

		public override string Text => Name;

		public override string SpecialRules => "Forbidden Ground - any time you create a sacred site, push all dahan from that land.  Dahan events never move dahan to you sacred site but powers can do so.";

		public Keeper():base(
			new KeeperPresence(
				new PresenceTrack( Track.Energy2, Track.SunEnergy, Track.Energy4, Track.Energy5, Track.PlantEnergy, Track.Energy7, Track.Energy8, Track.Energy9 ),
				new PresenceTrack( Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.Card4, Track.Card5Reclaim1 )
			),
			PowerCard.For<BoonOfGrowingPower>(),
			PowerCard.For<RegrowFromRoots>(),
			PowerCard.For<SacrosanctWilderness>(),
			PowerCard.For<TowingWrath>()
		) {
			(this.Presence as KeeperPresence).keeper = this;

			GrowthOptions = new GrowthOption[]{
				new GrowthOption( new ReclaimAll() ,new GainEnergy(1) ){ GainEnergy = 1 },
				new GrowthOption( new DrawPowerCard(1) ),
				new GrowthOption( new GainEnergy(1) ,new PlacePresence(3,Target.PresenceOrWilds) ){ GainEnergy = 1 },
				new GrowthOption( new GainEnergy(-3),new DrawPowerCard(1) ,new PlacePresence(3,Target.NoBlight) ){ GainEnergy = -3 },
			};
			growthOptionSelectionCount = 2;

			InnatePowers = new InnatePower[] {
				InnatePower.For<PunishThoseWhoTrespass>(),
				InnatePower.For<SpreadingWilds>(),
			};
		}

		protected override void InitializeInternal( Board board, GameState gs ){
			// In the highest-numbered Jungle.
			var space = board.Spaces.OrderByDescending( x => x.Terrain == Terrain.Jungle ).First();
			// Put 1 Presence
			Presence.PlaceOn( space );
			// 1 Wild 
			gs.Tokens[space].Wilds().Count++;
		}

	}

}
