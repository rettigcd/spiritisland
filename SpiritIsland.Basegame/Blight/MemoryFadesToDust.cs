using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class MemoryFadesToDust : BlightCardBase {

		public MemoryFadesToDust() : base( "Memory Fades to Dust", 2, 4 ) {}

		protected override async Task BlightAction( GameState gs ) {
			foreach(var spirit in gs.Spirits)
				await new SelfCtx( spirit, gs, Cause.Blight ).SelectActionOption(
					"BLIGHT: Memory Fades to Dust",
					new SelfAction("Destroy Presence",ctx=>ctx.GameState.Destroy1PresenceFromBlightCard(spirit,gs,Cause.Blight)),
					new SelfAction("Forget Power card", ctx => ctx.Self.ForgetPowerCard_UserChoice() )
				);
		}

	}

}
