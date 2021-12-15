using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class BeliefTakesRoot : IFearOptions {

		public const string Name = "Belief takes Root";

		[FearLevel( 1, "Defend 2 in all lands with Presence." )]
		public Task Level1( FearCtx ctx ) {
			Defend2WherePresence( ctx );
			return Task.CompletedTask;
		}

		static void Defend2WherePresence( FearCtx ctx ) {
			GameState gs = ctx.GameState;
			foreach(var space in gs.Spirits.SelectMany( s => s.Presence.Spaces ).Distinct())
				gs.Tokens[space].Defend.Add(2);
		}

		[FearLevel( 2, "Defend 2 in all lands with Presence. Each Spirit gains 1 Energy per SacredSite they have in lands with Invaders." )]
		public Task Level2( FearCtx ctx ) {
			Defend2WherePresence( ctx );
			foreach(var spirit in ctx.Spirits)
				spirit.Self.Energy += spirit.Self.Presence.SacredSites.Count( s => spirit.Target(s).HasInvaders );
			return Task.CompletedTask;
		}

		[FearLevel( 3, "Each player chooses a different land and removes up to 2 Health worth of Invaders per Presence there." )]
		public async Task Level3( FearCtx ctx ) {
			var used = new HashSet<Space>();
			foreach(var spiritCtx in ctx.Spirits)
				await RemoveInvadersFromLand( spiritCtx, used );

		}

		static async Task RemoveInvadersFromLand( SelfCtx ctx, HashSet<Space> used ) {
			var gs = ctx.GameState;
			// pick land
			var targetOptions = ctx.Self.Presence.Spaces
				.Where( s => gs.Tokens[s].HasAny( Invader.Town, Invader.Explorer ) )
				.Except( used )
				.ToArray();
			if(targetOptions.Length == 0) return;
			var targetSpace = await ctx.Self.Action.Decision( new Decision.TargetSpace( "Select land to remove 2 health worth of invaders/presence.", targetOptions, Present.Always ) );
			var sCtx = ctx.Target( targetSpace );

			// mark used
			used.Add( targetSpace );

			int damage = 2 * sCtx.Self.Presence.Placed.Count( x => x == sCtx.Space );

			damage = await sCtx.RemoveHealthWorthOfInvaders( damage );
		}

	}

}

