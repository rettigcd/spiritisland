using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class SteamVents {


		[MinorCard("Steam Vents", 1, Speed.Fast, "fire,air,water,earth")]
		[FromPresence(0)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {
			var target = ctx.Target;
			var grp = ctx.InvadersOn(target);

			// if your have 3 earth, 
			if(3<=ctx.Self.Elements[Element.Earth] && grp.HasTown)
				// instead destroy 1 town
				await grp.Destroy(Invader.Town,1);
			else
				// destroy 1 explorer
				await grp.Destroy(Invader.Explorer,1);
		}

	}
}
