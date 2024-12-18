namespace SpiritIsland.BranchAndClaw;

public class GrantHatredARavenousForm {

	[MajorCard( "Grant Hatred a Ravenous Form", 4, Element.Moon, Element.Fire ),Slow,FromPresence( 1 )]
	[Instructions( "For each Strife / Blight in target land, 1 Fear and 2 Damage. If this destroys all invaders in target land, add 1 Beasts. -If you have- 4 Moon, 2 Fire: Add 1 Strife in up to 3 adjacent lands." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		bool originallyHadInvaders = ctx.HasInvaders;

		// for each strife or blight in target land, 
		int count = ctx.Space.AllHumanTokens().Sum(x=>x.StrifeCount * ctx.Space[x])
			+ ctx.Blight.Count;
		// 1 fear 
		await ctx.AddFear(count);
		// and 2 damage.
		await ctx.DamageInvaders( count * 2 );

		// if this destroys all invaders in target land, add 1 beast.
		if(originallyHadInvaders && !ctx.HasInvaders)
			await ctx.Beasts.AddAsync(1);

		// if you have 4 moon, 2 fire
		if(await ctx.YouHave("4 moon,2 fire")) {
			// add 1 strife in up to 3 adjacent lands.
			List<Space> tokenSpaces = ctx.Adjacent
				.Where(s => s.HasInvaders())
				.ToList();
			for(int i = 0; 0<tokenSpaces.Count && i<3; ++i) {
				var space = await ctx.Self.SelectAsync(new A.SpaceDecision("Add Strife", tokenSpaces, Present.Done));
				if(space is null) break;
				await ctx.Target(space).AddStrife();
				tokenSpaces.Remove(space);
			}

		}
	}

}