using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class DahanEnheartened : IFearCard {

		public const string Name = "Dahan Enheartened";

		[FearLevel( 1, "Each player may Push 1 Dahan from a land with Invaders or Gather 1 Dahan into a land with Invaders." )]
		public async Task Level1( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var spacesWithInvaders = gs.Island.AllSpaces.Where( gs.HasInvaders ).ToArray();
				var target = await spirit.Action.Choose( new TargetSpaceDecision( "Select Space to Gather or push 1 dahan", spacesWithInvaders));
				bool canPush = gs.Dahan.Has(target);
				bool canGather = target.Adjacent.Any(gs.Dahan.Has);
				if(canPush && canGather) {
					if(await spirit.SelectText("Push or Gather?","push","gather")=="push")
						canPush=false;
					else
						canGather=false;
				}

				var engine = spirit.MakeDecisionsFor(gs);
				if(canPush)
					await engine.FearPushUpToNDahan(target,1);
				else if(canGather)
					await engine.GatherUpToNDahan(target,1);
			}
		}

		[FearLevel( 2, "Each player chooses a different land. In chosen lands: Gather up to 2 Dahan, then 1 Damage if Dahan are present." )]
		public async Task Level2( GameState gs ) {
			HashSet<Space> used = new HashSet<Space>();
			foreach(var spirit in gs.Spirits) {
				var engine = spirit.MakeDecisionsFor( gs );
				var options = gs.Island.AllSpaces.Where( gs.Dahan.Has ).Except( used ).ToArray();
				var target = await spirit.Action.Choose( new TargetSpaceDecision( "Fear:select land with dahan for 1 damage", options ));
				await engine.GatherUpToNDahan(target,2);
				if(gs.Dahan.Has(target))
					await gs.SpiritFree_DamageInvaders(target, 1 );
				used.Add( target );
			}
		}

		[FearLevel( 3, "Each player chooses a different land. In chosen lands: Gather up to 2 Dahan, then 1 Damage per Dahan present." )]
		public async Task Level3( GameState gs ) {
			HashSet<Space> used = new HashSet<Space>();
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces.Where( gs.Dahan.Has ).Except( used ).ToArray();
				var target = await spirit.Action.Choose( new TargetSpaceDecision( "Fear:select land with dahan for 1 damage", options ));
				await spirit.MakeDecisionsFor( gs ).GatherUpToNDahan( target, 2 );
				await gs.SpiritFree_DamageInvaders(target, gs.Dahan.Count(target) );
				used.Add( target );
			}
		}
	}
}
