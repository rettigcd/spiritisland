
namespace SpiritIsland {

/*

=========================================================
A Spread of Rampant Green

* +1 presense to jungle or wetland (Always do this + one of the following)
* reclaim, +1 power card
* +1 presense range 1, play +1 extra card this turn
* +1 power card, +3 energy

0 1 plant 2 2 plant 3
1 1 2 2 3 4
Innate-1:  Creepers Tear Into Mortar  => slow, 0,any
1 moon 2 plant   1 damange to 1 town or city
2 moon 3 plant   repeat this power
3 moon 4 plant   repeat this power again
Innate-2:   All-Enveloping Green => fast, within 1 of sacred, any
1 water 3 plant  defend 2
2 water 4 plant  instead, defend 4
3 water 1 rock 5 plant  also, remove 1 blight
Special Ruels - Choke the land with green - whenever invaders would ravage or build in a land with your sacred site, you may prevent it by destroying one of your presense in that land
Special Rules - Stedy regeneration - when adding presense to the board via growth, yo may optionally use your destroyed presense.  If the island is helathy, do so freely.  If the island is blighted, doing so costs 1 energy per destroyed presense you add.
Setup: 1 in the highest numbered wedland and 1 in the jungle without any dahan

Overgrow in a Night => 2 => fast, range 1, any => moon, plant => add 1 presense -OR- If target land has your presense and invaders, 3 fear
Stem the Flow of Fresh Water => 0 => slow, withing 1 of sacred site, any => water, plant => 1 damage to 1 town or city.  If target land is mountain orsand, instead 1 damange to EACH town/city
Fields Choked with Growth => 0 => slow, range 1, any => sun, water, plant => push 1 town -OR- push 3 dahan
Gift of Proliferation => 1 => fast, any spirit => moon, plant => target spirit adds 1 presense up to 1 from their presesnse
*/

	public class RampantGreen : Spirit {

		public RampantGreen(){
			NumberOfCardsPlayablePerTurn = 1;
		}

		public override GrowthOption[] GetGrowthOptions() {
			
			bool IsWetlandOrJungle( Space bs, GameState gs ) 
				=> bs.Terrain == Terrain.Jungle || bs.Terrain == Terrain.Wetland;
			var placePresenceInJungleOrWetland = new PlacePresence(this,2,IsWetlandOrJungle);

			return new GrowthOption[]{
				// reclaim, +1 power card
				new GrowthOption( placePresenceInJungleOrWetland, new ReclaimAll(this), new DrawPowerCard(this,1) ),
				// +1 presense range 1, play +1 extra card this turn
				new GrowthOption( placePresenceInJungleOrWetland, new PlacePresence(this,1),new PlayExtraCardThisTurn(this) ),
				// +1 power card, +3 energy
				new GrowthOption( placePresenceInJungleOrWetland, new GainEnergy(this,3), new DrawPowerCard(this) ),
			};
		}

		public override void PlayAvailableCards( params int[] cards ) {
			base.PlayAvailableCards( cards );
			NumberOfCardsPlayablePerTurn -= tempCardBoost; // remove temp boost
			tempCardBoost = 0;
		}
		public int tempCardBoost = 0;

	}

	/// <summary>
	/// One of Rampant Green's special growth options
	/// </summary>
	class PlayExtraCardThisTurn : GrowthAction {
		public PlayExtraCardThisTurn(Spirit spirit):base(spirit){}
		public override void Apply() {
			(spirit as RampantGreen).tempCardBoost++;
			spirit.NumberOfCardsPlayablePerTurn ++;
		}
	}


}
