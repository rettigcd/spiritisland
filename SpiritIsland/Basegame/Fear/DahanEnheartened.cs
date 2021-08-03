using SpiritIsland.Core;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class DahanEnheartened : IFearCard {

		[FearLevel( 1, "Each player may Push 1 Dahan from a land with Invaders or Gather 1 Dahan into a land with Invaders." )]
		public async Task Level1( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var engine = new ActionEngine(spirit,gs);
				var target = await engine.SelectSpace("Select Space to Gather or push 1 dahan",gs.Island.AllSpaces.Where(x=>x.IsLand).ToArray());
				bool canPush = gs.HasDahan(target);
				bool canGather = target.Neighbors.Any(gs.HasDahan);
				if(canPush && canGather) {
					if(await engine.SelectText("Push or Gather?","push","gather")=="push")
						canPush=false;
					else
						canGather=false;
				}
				if(canPush)
					await engine.Push1Dahan(target);
				else if(canGather)
					await engine.GatherUpToNDahan(target,1);
			}
		}

		//"Each player chooses a different land. In chosen lands: Gather up to 2 Dahan, then 1 Damage if Dahan are present.", 
		[FearLevel( 2, "" )]
		public Task Level2( GameState gs ) {
			throw new System.NotImplementedException();
		}

		//"Each player chooses a different land. In chosen lands: Gather up to 2 Dahan, then 1 Damage per Dahan present."),
		[FearLevel( 3, "" )]
		public Task Level3( GameState gs ) {
			throw new System.NotImplementedException();
		}
	}
}
