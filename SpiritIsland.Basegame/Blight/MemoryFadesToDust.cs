using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class MemoryFadesToDust : BlightCardBase {

		public MemoryFadesToDust() : base( "Memory Fades to Dust", 2, 4 ) {}

		protected override async Task BlightAction( GameState gs ) {
			foreach(var spirit in gs.Spirits)
				await new SpiritGameStateCtx( spirit, gs, Cause.Blight ).SelectActionOption(
					"BLIGHT: Memory Fades to Dust",
					new ActionOption("Destroy Presence",()=>gs.Destroy1PresenceFromBlightCard(spirit,gs,Cause.Blight)),
					new ActionOption("Forget Power card", () => spirit.ForgetPowerCard() )
				);
		}

	}

}
