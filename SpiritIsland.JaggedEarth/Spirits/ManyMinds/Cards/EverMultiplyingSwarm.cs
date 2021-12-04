using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class EverMultiplyingSwarm {

		[SpiritCard("Ever-Multiplying Swarm",1,Element.Fire,Element.Earth,Element.Animal), Slow, FromPresence(0)]
		static public Task ActAsync(TargetSpaceCtx ctx ) {
			// Add 2 beast
			ctx.Beasts.Add( 2 );
			return Task.CompletedTask;
		}
	}


}
