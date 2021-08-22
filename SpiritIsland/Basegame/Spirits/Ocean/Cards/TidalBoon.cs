using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class TidalBoon {

		[SpiritCard("Tidal Boon",1,Speed.Slow,Element.Moon,Element.Water,Element.Earth)]
		[TargetSpirit]
		static public async Task Act(TargetSpiritCtx ctx ) {
			var targetCtx = new PowerCtx( ctx.Target, ctx.GameState );

			// target spirit gains 2 energy 
			targetCtx.Self.Energy += 2;

			// and may push 1 town and up to 2 dahan from one of their lands.
			var pushLand = await targetCtx.Self.SelectSpace("Select land to push town and 2 dahan",ctx.Target.Presence.Spaces, Present.Done );
			if(pushLand==null) return;
			await targetCtx.PowerPushUpToNInvaders(pushLand,1,Invader.Town);
			await targetCtx.PowerPushUpToNDahan(pushLand,2);

			// !!! If dahan are pushed to your ocean, you may move them to any costal land instead of drowning them.
		}

	}
}
