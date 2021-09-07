using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class BloodwrackPlague {

		[MajorCard("Bloodwrack Plague",4,Speed.Fast,Element.Water,Element.Earth,Element.Animal)]
		[FromSacredSite(1)]
		static public Task ActAsync(TargetSpaceCtx ctx ) {
			// add 2 disease
			// for each diseast in target land, defend 1 in target and all adjacent lands
			// if you have 2 earthn 4 animal:  2 fear.  For each disease in target land, do 1 damage in target or adjacent land
			return Task.CompletedTask;
		}

	}

}
