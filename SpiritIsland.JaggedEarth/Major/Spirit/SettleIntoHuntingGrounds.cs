namespace SpiritIsland.JaggedEarth;

public class SettleIntoHuntingGrounds {

	[MajorCard("Settle Into Hunting-Grounds",3,Element.Moon,Element.Fire,Element.Plant,Element.Animal), Fast, Yourself]
	public static async Task ActAsync(SelfCtx ctx ) {

		// Your presence may count as badlands and beast.
		ctx.GameState.Tokens.Dynamic.ForRound.Register( ctx.Self.Presence.CountOn, Token.Badlands );
		ctx.GameState.Tokens.Dynamic.ForRound.Register( ctx.Self.Presence.CountOn, Token.Beast );
		// (Decide per presence, per action) ... Not doing this bit exactly, both are always present, but can't be destroyed.

		// your presence cannot move.
		ctx.Self.Presence.CanMove = false;
		ctx.GameState.TimePasses_ThisRound.Push( _ => {
			ctx.Self.Presence.CanMove = true; 
			return Task.CompletedTask;
		} );

		// if you have 2 plant 3 animal:
		if( await ctx.YouHave("2 plant,3 animal" )){
			// 2 fear and
			ctx.AddFear(2);
			// 2 damamge in one of your lands.
			var space = await ctx.Decision( new Select.ASpace("2 damage", ctx.Self.Presence.SpaceStates.Downgrade(), Present.Always ));
			if( space != null )
				await ctx.Target(space).DamageInvaders( 2 );
		}
	}

}