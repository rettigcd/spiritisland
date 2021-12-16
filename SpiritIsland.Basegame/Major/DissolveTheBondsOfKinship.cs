﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class DissolveTheBondsOfKinship {
		
		[MajorCard("Dissolve the Bonds of Kinship",4,Element.Fire,Element.Earth,Element.Animal)]
		[Slow]
		[FromPresence(1)]
		static public async Task ActAsync(TargetSpaceCtx ctx) {

			// replace 1 city with 2 exploreres.
			await ReplaceInvader.SingleInvaderWithExplorers( ctx.Self, ctx.Invaders, Invader.City, 2 );

			// replace 1 town with 1 explorer
			await ReplaceInvader.SingleInvaderWithExplorers( ctx.Self, ctx.Invaders, Invader.Town, 1 );

			// replace 1 dahan with 1 explorer.
			if( ctx.Tokens.Dahan.Remove1() != null )
				ctx.Tokens.Adjust( Invader.Explorer.Default, 1 );

			// if you have 2 fire 2 water 3 animal
			if(await ctx.YouHave("2 fire,2 water,3 animal" )) {
				// before pushing, explorers and city/town do damage to each other
				int damageFromExplorers = ctx.Invaders[Invader.Explorer[1]];
				int damageToExplorers = ctx.Invaders.Tokens.Sum(Invader.City)*3 + ctx.Invaders.Tokens.Sum(Invader.Town)*2;
				await ctx.DamageInvaders(damageFromExplorers,Invader.City,Invader.Town);
				await ctx.DamageInvaders( damageToExplorers, Invader.Explorer );
			}

			// Push all explorers from target land to as many different lands as possible
			await ctx.Push( int.MaxValue,Invader.Explorer);
		}

	}

}
