using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class DissolveTheBondsOfKinship {
		
		[MajorCard("Dissolve the Bonds of Kinship",4,Speed.Slow,Element.Fire,Element.Earth,Element.Animal)]
		[FromPresence(1)]
		static public async Task ActAsync(TargetSpaceCtx ctx) {
			var (self, gs) = ctx;
			var grp = ctx.InvadersOn( ctx.Target ); // !!! could gold this into TargetSpace context

			// replace 1 city with 2 exploreres.
			ReplaceInvaderWithExplorer( grp, Invader.City, 2 );
			// replace 1 town with 1 explorer
			ReplaceInvaderWithExplorer( grp, Invader.Town, 1 );
			// replace 1 dahan with 1 explorer.
			ReplaceDahanWithExplorer( gs, grp );

			// if you have 2 fire 2 water 3 animal
			if(self.Elements.Contains("2 fire,2 water,3 animal" )) {
				// before pushing, explorers and city/town do damage to each other
				int damageFromExplorers = grp[InvaderSpecific.Explorer];
				int damageToExplorers = grp[Invader.City]*3 + grp[Invader.Town]*2;
				await grp.SmartDamageToTypes(damageFromExplorers,Invader.City,Invader.Town);
				await grp.SmartDamageToTypes( damageToExplorers, Invader.Explorer );
			}

			// Push all explorers from target land to as many different lands as possible
			await ctx.PushUpToNInvaders( ctx.Target, int.MaxValue,Invader.Explorer);
		}

		static void ReplaceInvaderWithExplorer( InvaderGroup grp, Invader oldInvader, int replaceCount ) {
			var specific = grp.FilterBy( oldInvader ).OrderByDescending( x => x.Health ).FirstOrDefault();
			if(specific != null) {
				grp.Adjust( specific, -1 );
				grp.Adjust( InvaderSpecific.Explorer, replaceCount );
			}
		}

		static void ReplaceDahanWithExplorer( GameState gs, InvaderGroup grp ) {
			if(gs.HasDahan( grp.Space )) { 
				gs.AdjustDahan(grp.Space,-1);
				grp.Adjust( InvaderSpecific.Explorer, 1 );
			}
		}

	}
}
