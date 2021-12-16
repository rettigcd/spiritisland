using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	[InnatePower("Why Don't You and Them Fight"), FastButSlowIf("3 moon"), FromPresence(0,Target.Invaders)]
	class WhyDontYouAndThemFight {

		[InnateOption("3 moon","This Power may be slow.",AttributePurpose.DisplayOnly,1)]
		static public Task Option1(TargetSpaceCtx _ ) => Task.CompletedTask;

		[InnateOption("3 air","Add 1 strife.",2)]
		static public Task Option2(TargetSpaceCtx ctx ) => ctx.AddStrife();

		[InnateOption("3 sun","OR",3)]
		static public Task Option3a(TargetSpaceCtx ctx ) => Option3b(ctx);

		[InnateOption("3 fire","1 Invader and 1 dahan deal Damage to each other.",3)]
		static public async Task Option3b(TargetSpaceCtx ctx ) {
			var invaders = ctx.Tokens.Invaders().ToArray();
			if(invaders.Length == 0 || !ctx.Dahan.Any) return;
			var invader = await ctx.Decision(new Select.TokenFrom1Space("Select invader to fight 1 dahan",ctx.Space,invaders,Present.Always));

			// Calc Invader Damage
			var (damageFromInvader,newInvaderToken) = GetDamageFromInvader( ctx.Tokens, invader );
			// Calc Dahan Damage
			int damageFromDahan = 2;
			
			// Damage invader
			await ctx.Invaders.ApplyDamageTo1(damageFromDahan,newInvaderToken);

			// damage dahan
			if(damageFromInvader>=2)
				await ctx.DestroyDahan(1);

		}

		static (int,Token) GetDamageFromInvader( TokenCountDictionary tokens, Token invader ) {
			return invader is StrifedInvader si 
				? (0,tokens.RemoveStrife( si, 1 )) 
				: (invader.FullHealth,invader);
		}

		[InnateOption("3 animal","If target land has beast, 2 Damage. Otherwise, you may Gather 1 beast.",4)]
		static public async Task Option4(TargetSpaceCtx ctx ) {
			// if target land has beasts
			if(ctx.Beasts.Any)
				// 2 damage
				await ctx.DamageInvaders(2);
			else // otherwise, you may gather 1 beast
				await ctx.GatherUpTo(1,TokenType.Beast.Generic);
		}


	}

}
