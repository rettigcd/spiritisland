﻿using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class DissolveTheBondsOfKinship {
		
		[MajorCard("Dissolve the Bonds of Kinship",4,Speed.Slow,Element.Fire,Element.Earth,Element.Animal)]
		[FromPresence(1)]
		static public async Task ActAsync(TargetSpaceCtx ctx) {

			// replace 1 city with 2 exploreres.
			ReplaceInvaderWithExplorer( ctx.Invaders, Invader.City, 2 );

			// replace 1 town with 1 explorer
			ReplaceInvaderWithExplorer( ctx.Invaders, Invader.Town, 1 );

			// replace 1 dahan with 1 explorer.
			ReplaceDahanWithExplorer( ctx );

			// if you have 2 fire 2 water 3 animal
			if(ctx.YouHave("2 fire,2 water,3 animal" )) {
				// before pushing, explorers and city/town do damage to each other
				int damageFromExplorers = ctx.Invaders[Invader.Explorer[1]];
				int damageToExplorers = ctx.Invaders.Tokens.Sum(Invader.City)*3 + ctx.Invaders.Tokens.Sum(Invader.Town)*2;
				await ctx.Invaders.SmartDamageToTypes(damageFromExplorers,Invader.City,Invader.Town);
				await ctx.Invaders.SmartDamageToTypes( damageToExplorers, Invader.Explorer );
			}

			// Push all explorers from target land to as many different lands as possible
			await ctx.Push( int.MaxValue,Invader.Explorer);
		}

		static void ReplaceInvaderWithExplorer( InvaderGroup grp, TokenGroup oldInvader, int replaceCount ) {
			// !!! when replacing, need to divy up pre-existing damage
			var counts = grp.Tokens;
			var specific = counts.OfType( oldInvader ).OrderByDescending( x => x.Health ).FirstOrDefault();
			if(specific != null) {
				counts.Adjust( specific, -1 );
				counts.Adjust( Invader.Explorer[1], replaceCount );
			}
		}

		static void ReplaceDahanWithExplorer(TargetSpaceCtx ctx ) {
			// !!! when replacing, need to divy up pre-existing damage
			if(ctx.HasDahan) { 
				ctx.AdjustDahan(-1);
				ctx.Invaders.Tokens.Adjust( Invader.Explorer[1], 1 );
			}
		}

	}
}
