namespace SpiritIsland.BranchAndClaw;
public class TeethGleamFromDarkness {

	[SpiritCard("Teeth Gleam From Darkness",1,Element.Moon,Element.Plant,Element.Animal),Slow,FromPresenceIn(Target.Jungle,1,Target.NoBlight)]
	[Instructions( "1 Fear. Add 1 Beasts. -or- If target land has both Beasts and Invaders: 3 Fear." ), Artist( Artists.MoroRogers )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {

		await ctx.SelectActionOption(
			new SpaceCmd("1 fear, add 1 beast", ctx => { ctx.AddFear(1); ctx.Beasts.Add(1); } ),
			new SpaceCmd("3 fear", ctx => ctx.AddFear(3) )
				.OnlyExecuteIf( x => ctx.Tokens.HasInvaders() && x.Beasts.Any )
		);

	}

}