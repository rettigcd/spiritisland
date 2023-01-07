namespace SpiritIsland.JaggedEarth;

public class VoiceOfCommand {

	[MajorCard("Voice of Command",3,Element.Sun,Element.Air), Fast, FromSacredSite(1,Target.Dahan)]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {

		// if you have 3 sun 2 air:
		if( await ctx.YouHave("3 sun,2 air"))
			// first gather up to 2 explorer/town/dahan
			await ctx.GatherUpTo(2, Invader.Explorer_Town.Plus(TokenType.Dahan) );

		// 1 damage per dahan/explorer, to towns/cities only
		await ctx.DamageInvaders( 
			ctx.Dahan.CountAll + ctx.Tokens.Sum( Invader.Explorer ),
			Invader.Town_City
		);

		// Defend 2.
		ctx.Defend(2);

		// During Ravage Actions, explorer fight alongside dahan. (Deal/take Damage at the same time, and to/from the same source.)
		ctx.GameState.ModifyRavage( ctx.Space, cfg =>{ 
			cfg.IsAttacker = (token) => token.Class.IsOneOf( Invader.Town_City );
			cfg.IsDefender = (token) => token.Class.IsOneOf( Invader.Explorer, TokenType.Dahan);
		});
	}

}