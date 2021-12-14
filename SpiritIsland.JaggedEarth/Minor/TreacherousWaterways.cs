using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class TreacherousWaterways{ 

		[MinorCard("Treacherous Waterways",0,Element.Fire,Element.Water,Element.Earth),Fast,FromPresence(1,Target.MountainOrWetland)]
		static public Task ActAsync(TargetSpaceCtx ctx){
			return ctx.SelectActionOption( 
				Cmd.Add1Wilds, 
				Cmd.PushUpToNExplorers(1)
			);
		}

	}
}
