namespace SpiritIsland.JaggedEarth;

public class UnnervingPall {

	[SpiritCard("Unnerving Pall",1,Element.Moon,Element.Air,Element.Animal), Fast, FromPresence(0,Target.Invaders)]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// 1 fear.
		ctx.AddFear(1);

		// up to 3 Damaged Invaders do not participate in Ravage.
		var doNotParticipate = new SpaceAction(
			"up to 3 damaged Invaders do not participate in Ravage",
			SelectUpTo3DamagedInvadersToNotParticipate
		);
		// Defend 1 per presence you have in target land (when this Power is used).
		var defend = new SpaceAction(
			"Defend 1 per presence you have in target land", // (when power is used)
			ctx => ctx.Defend( ctx.Presence.Count )
		);

		await ctx.SelectActionOption( doNotParticipate, defend );

	}

	static async Task SelectUpTo3DamagedInvadersToNotParticipate( TargetSpaceCtx ctx ) {

		// Find Damaged Invaders
		var damagedInvaders = new List<ISpaceEntity>();
		foreach(var token in ctx.Tokens.InvaderTokens().Where( t => t.RemainingHealth < t.FullHealth ))
			for(int i = 0; i < ctx.Tokens[token]; ++i)
				damagedInvaders.Add( token );
		if(damagedInvaders.Count == 0)
			return;

		// Create a list to hold ones we've selected to exclude
		var skipInvaders = new List<ISpaceEntity>();
		// Select up to 3 to put in the skip-list
		int remaining = 3;
		while(remaining-- > 0 && damagedInvaders.Count > 0) {
			var decision = new Select.TokenFromManySpaces(
				"Select invader to not participate in ravage", ctx.Space,
				damagedInvaders.Distinct().Cast<IToken>(),
				Present.Done
			);
			var skip = (await ctx.Decision( decision ))?.Token;
			if(skip == null) break;
			skipInvaders.Add( skip );
			damagedInvaders.Remove( skip );
		}

		// If we selected any, remove them from the fight
		if(skipInvaders.Count > 0) {
			var cfg = ctx.Tokens.RavageBehavior;
			foreach(var s in skipInvaders)
				cfg.NotParticipating[s]++;
		}

	}

}