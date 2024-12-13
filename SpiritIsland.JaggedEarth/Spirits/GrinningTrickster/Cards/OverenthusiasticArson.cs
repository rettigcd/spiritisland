namespace SpiritIsland.JaggedEarth;

public class OverenthusiasticArson {

	public const string Name = "Overenthusiastic Arson";

	[SpiritCard(OverenthusiasticArson.Name,1,Element.Fire,Element.Air), Fast, FromPresence(1)]
	[Instructions( "Destroy 1 Town. Discard the top card of the Minor Power Deck. If it provides Fire: 1 Fear, 2 Damage, and add 1 Blight." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsymc(TargetSpaceCtx ctx ) { 

		// Destroy 1 town
		await ctx.Invaders.DestroyNOfClass(1,Human.Town);

		// discard the top card of the minor power deck.
		var card = GameState.Current.MinorCards.FlipNext();

		// Show the card to the user
		_ = await ctx.Self.SelectPowerCard(OverenthusiasticArson.Name + " turned up:", 1, new PowerCard[] { card }, CardUse.Accept, Present.Always );

		// IF it provides fire:
		if(card.Elements.Contains( Element.Fire )) {
			// 1 fear
			await ctx.AddFear(1);

			// 2 damage,
			await ctx.DamageInvaders(2);

			// add 1 blight
			await ctx.AddBlight(1);
		}
	}

}