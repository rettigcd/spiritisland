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
				bool canPush = gs.DahanIsOn(target);
				bool canGather = target.Adjacent.Any(gs.DahanIsOn);
				if(canPush && canGather) {
					if(await spirit.SelectText("Push or Gather?","push","gather")=="push")
						canPush=false;
					else
						canGather=false;
				}

				var engine = spirit.MakeDecisionsFor(gs);
				if(canPush)
					await engine.FearPushUpToNTokens(target,1,TokenType.Dahan );
				else if(canGather)
					await engine.GatherUpToNTokens(target,1,TokenType.Dahan);
			}
		}

		[FearLevel( 2, "Each player chooses a different land. In chosen lands: Gather up to 2 Dahan, then 1 Damage if Dahan are present." )]
		public async Task Level2( GameState gs ) {
			HashSet<Space> used = new HashSet<Space>();
			foreach(var spirit in gs.Spirits) {
				var engine = spirit.MakeDecisionsFor( gs );
				var options = gs.Island.AllSpaces.Where( gs.DahanIsOn ).Except( used ).ToArray();
				var target = await spirit.Action.Choose( new TargetSpaceDecision( "Fear:select land with dahan for 1 damage", options ));
				await engine.GatherUpToNTokens(target,2, TokenType.Dahan );
				if(gs.DahanIsOn(target))
					await gs.SpiritFree_FearCard_DamageInvaders(target, 1 );
				used.Add( target );
			}
		}

		[FearLevel( 3, "Each player chooses a different land. In chosen lands: Gather up to 2 Dahan, then 1 Damage per Dahan present." )]
		public async Task Level3( GameState gs ) {
			HashSet<Space> used = new HashSet<Space>();
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces.Where( gs.DahanIsOn ).Except( used ).ToArray();
				var target = await spirit.Action.Choose( new TargetSpaceDecision( "Fear:select land with dahan for 1 damage", options ));
				await spirit.MakeDecisionsFor( gs ).GatherUpToNTokens( target, 2, TokenType.Dahan );
				await gs.SpiritFree_FearCard_DamageInvaders(target, gs.DahanGetCount(target) );
				used.Add( target );
			}
		}
	}
}
