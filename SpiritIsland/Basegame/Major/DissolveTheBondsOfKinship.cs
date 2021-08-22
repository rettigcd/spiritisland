using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class DissolveTheBondsOfKinship {
		
		[MajorCard("Dissolve the Bonds of Kinship",4,Speed.Slow,Element.Fire,Element.Earth,Element.Animal)]
		[FromPresence(1)]
		static public async Task ActAsync(TargetSpaceCtx ctx) {

			// replace 1 city with 2 exploreres.
			ReplaceInvaderWithExplorer( ctx.InvadersOn, Invader.City, 2 );
			// replace 1 town with 1 explorer
			ReplaceInvaderWithExplorer( ctx.InvadersOn, Invader.Town, 1 );
			// replace 1 dahan with 1 explorer.
			ReplaceDahanWithExplorer( ctx );

			// if you have 2 fire 2 water 3 animal
			if(ctx.Self.Elements.Contains("2 fire,2 water,3 animal" )) {
				// before pushing, explorers and city/town do damage to each other
				int damageFromExplorers = ctx.InvadersOn[InvaderSpecific.Explorer];
				int damageToExplorers = ctx.InvadersOn[Invader.City]*3 + ctx.InvadersOn[Invader.Town]*2;
				await ctx.InvadersOn.SmartDamageToTypes(damageFromExplorers,Invader.City,Invader.Town);
				await ctx.InvadersOn.SmartDamageToTypes( damageToExplorers, Invader.Explorer );
			}

			// Push all explorers from target land to as many different lands as possible
			await ctx.PowerPushUpToNInvaders( int.MaxValue,Invader.Explorer);
		}

		static void ReplaceInvaderWithExplorer( InvaderGroup grp, Invader oldInvader, int replaceCount ) {
			var specific = grp.FilterBy( oldInvader ).OrderByDescending( x => x.Health ).FirstOrDefault();
			if(specific != null) {
				grp.Adjust( specific, -1 );
				grp.Adjust( InvaderSpecific.Explorer, replaceCount );
			}
		}

		static void ReplaceDahanWithExplorer(TargetSpaceCtx ctx ) {
			if(ctx.HasDahan) { 
				ctx.AdjustDahan(-1);
				ctx.Invaders.Adjust( InvaderSpecific.Explorer, 1 );
			}
		}

	}
}
