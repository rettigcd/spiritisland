using SpiritIsland;
using System.Collections.Generic;
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
				bool canGather = target.Adjacent.Any(gs.HasDahan);
				if(canPush && canGather) {
					if(await engine.SelectText("Push or Gather?","push","gather")=="push")
						canPush=false;
					else
						canGather=false;
				}
				if(canPush)
					await engine.PushUpToNDahan(target,1);
				else if(canGather)
					await engine.GatherUpToNDahan(target,1);
			}
		}

		[FearLevel( 2, "Each player chooses a different land. In chosen lands: Gather up to 2 Dahan, then 1 Damage if Dahan are present." )]
		public async Task Level2( GameState gs ) {
			HashSet<Space> used = new HashSet<Space>();
			foreach(var spirit in gs.Spirits) {
				var engine = new ActionEngine( spirit, gs );
				var options = gs.Island.AllSpaces.Where( gs.HasDahan ).Except( used ).ToArray();
				var target = await engine.SelectSpace( "Fear:select land with dahan for 1 damage", options );
				await engine.GatherUpToNDahan(target,2);
				if(gs.HasDahan(target))
					gs.DamageInvaders( target, 1 );
				used.Add( target );
			}
		}

		[FearLevel( 3, "Each player chooses a different land. In chosen lands: Gather up to 2 Dahan, then 1 Damage per Dahan present." )]
		public async Task Level3( GameState gs ) {
			HashSet<Space> used = new HashSet<Space>();
			foreach(var spirit in gs.Spirits) {
				var engine = new ActionEngine( spirit, gs );
				var options = gs.Island.AllSpaces.Where( gs.HasDahan ).Except( used ).ToArray();
				var target = await engine.SelectSpace( "Fear:select land with dahan for 1 damage", options );
				await engine.GatherUpToNDahan( target, 2 );
				gs.DamageInvaders( target, gs.GetDahanOnSpace(target) );
				used.Add( target );
			}
		}
	}
}
