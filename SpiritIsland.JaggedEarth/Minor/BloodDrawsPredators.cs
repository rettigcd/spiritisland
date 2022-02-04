namespace SpiritIsland.JaggedEarth;
	
public class BloodDrawsPredators{ 
		
	[MinorCard("Blood Draws Predators",1,Element.Sun,Element.Fire,Element.Water,Element.Animal),Fast, FromPresence(1)]
	static public Task ActAsync( TargetSpaceCtx ctx ){

		// After the next time Invaders are Desttroyed in target land:
		bool used = false;
		ctx.GameState.Tokens.TokenRemoved.ForRound.Add( async ( args ) => {
			if(used || args.Space != ctx.Space ) return;
			used = true;
			// Add 1 Beast,
			await ctx.Beasts.Add(1);

			// Then 1 Damage per Beast (max. 3 Damage)
			await ctx.DamageInvaders( ctx.Beasts.Count);
		} );

		return Task.CompletedTask;
	}

}