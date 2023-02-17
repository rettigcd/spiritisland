namespace SpiritIsland.JaggedEarth;
	
public class BloodDrawsPredators{ 

	const string Name = "Blood Draws Predators";

	[MinorCard(Name,1,Element.Sun,Element.Fire,Element.Water,Element.Animal),Fast, FromPresence(1)]
	static public Task ActAsync( TargetSpaceCtx ctx ){

		// After the next time Invaders are Destroyed in target land:
		TokenRemovedHandlerAsync mod = null; // initialized 1st so method can refer to it.
		mod = new TokenRemovedHandlerAsync( async ( args ) => {
			if(args.Reason != RemoveReason.Destroyed || !args.Removed.Class.IsOneOf(Human.Invader)) return;
			args.From.Adjust(mod,-1); // remove token

			// Add 1 Beast,
			await ctx.Beasts.Add( 1 );

			// Then 1 Damage per Beast (max. 3 Damage)
			await ctx.DamageInvaders( ctx.Beasts.Count );
		} );
		ctx.Tokens.Adjust( mod, 1 );

		return Task.CompletedTask;
	}

}