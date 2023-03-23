namespace SpiritIsland.Basegame;

public class IndomitableClaim {

	public const string Name = "Indomitable Claim";

	[MajorCard( IndomitableClaim.Name, 4, Element.Sun, Element.Earth ),Fast,FromPresence( 1 )]
	[Instructions( "Add 1 Presence in target land even if you normally could not due to land type. Defend 20. -If you have- 2 Sun, 3 Earth: 3 Fear if Invaders are present. Invaders skip all Actions in target land this turn." ), Artist( Artists.JorgeRamos )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// add 1 presence in target land even if you normally could not due to land type.
		await ctx.PlacePresenceHere();

		// Defend 20
		ctx.Defend(20);

		// if you have 2 sun, 3 earth,
		if(await ctx.YouHave("2 sun,3 earth" )) {

			// 3 fear if invaders are present,
			if(ctx.HasInvaders)
				ctx.AddFear(3);

			// Invaders skip all actions in target land this turn.
			ctx.Tokens.SkipAllInvaderActions(Name);
		}
	}

}