using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class DahanEnheartened : IFearOptions {

		public const string Name = "Dahan Enheartened";

		[FearLevel( 1, "Each player may Push 1 Dahan from a land with Invaders or Gather 1 Dahan into a land with Invaders." )]
		public async Task Level1( FearCtx ctx ) {
			var gs = ctx.GameState;
			foreach( var spirit in ctx.Spirits ) {
				var spacesWithInvaders = gs.Island.AllSpaces.Where( s=>spirit.Target(s).HasInvaders ).ToArray();
				var target = await spirit.Self.Action.Decision( new Decision.TargetSpace( "Select Space to Gather or push 1 dahan", spacesWithInvaders));

				var spaceCtx = spirit.Target(target);
				await spirit.SelectActionOption(
					new ActionOption("Push",    () => spaceCtx.PushUpTo( 1, TokenType.Dahan) ),
					new ActionOption( "Gather", () => spaceCtx.GatherUpTo( 1, TokenType.Dahan ) )
				);
			}
		}

		[FearLevel( 2, "Each player chooses a different land. In chosen lands: Gather up to 2 Dahan, then 1 Damage if Dahan are present." )]
		public async Task Level2( FearCtx ctx ) {
			var gs = ctx.GameState;
			HashSet<Space> used = new ();
			foreach(var spirit in gs.Spirits) {
				var engine = spirit.MakeDecisionsFor( gs );
				var options = gs.Island.AllSpaces.Where( gs.DahanIsOn ).Except( used ).ToArray();
				var target = await spirit.Action.Decision( new Decision.TargetSpace( "Fear:select land with dahan for 1 damage", options ));
				await engine.GatherUpTo(target,2, TokenType.Dahan );
				if(gs.DahanIsOn(target))
					await gs.SpiritFree_FearCard_DamageInvaders(target, 1 );
				used.Add( target );
			}
		}

		[FearLevel( 3, "Each player chooses a different land. In chosen lands: Gather up to 2 Dahan, then 1 Damage per Dahan present." )]
		public async Task Level3( FearCtx ctx ) {
			HashSet<Space> used = new ();
			var gs = ctx.GameState;
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces.Where( gs.DahanIsOn ).Except( used ).ToArray();
				var target = await spirit.Action.Decision( new Decision.TargetSpace( "Fear:select land with dahan for 1 damage", options ));
				await spirit.MakeDecisionsFor( gs ).GatherUpTo( target, 2, TokenType.Dahan );
				await gs.SpiritFree_FearCard_DamageInvaders(target, gs.DahanGetCount(target) );
				used.Add( target );
			}
		}
	}
}
