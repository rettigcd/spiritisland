
namespace SpiritIsland {

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

	*/

	public class Shadows : Spirit {
		public override GrowthOption[] GetGrowthOptions(GameState gameState){
			return new GrowthOption[]{
				new GrowthOption( new ReclaimAll(this), new DrawPowerCard(this,1) ),
				new GrowthOption( new DrawPowerCard(this,1), new PlacePresence(this,gameState,1) ),
				new GrowthOption( new PlacePresence(this,gameState,3), new GainEnergy(this,3) )
			};
		}
	}
}
