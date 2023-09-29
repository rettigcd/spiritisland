namespace SpiritIsland.NatureIncarnate;

public class BloomingOfTheRocksAndTrees {
	const string Name = "Blooming of the Rocks and Trees";

	[SpiritCard( Name, 0, Element.Sun, Element.Air, Element.Earth, Element.Plant ), Slow, FromSacredSite(1)]
	[Instructions( "If no Blight is present, Add 1 Vitality. -or- If no Invaders are present, Add 1 Wilds. -If you have- 3 Plant: You may do both." ), Artist( Artists.AalaaYassin )]
	static async public Task ActAsync( TargetSpaceCtx ctx ) {

		var cmds = new List<IExecuteOn<TargetSpaceCtx>>();

		// If no Blight is present, Add 1 Vitality.
		if(!ctx.Tokens.Blight.Any)
			cmds.Add(Cmd.AddVitality(1));

		// OR

		// If no Invaders are present, Add 1 Wilds.
		if(!ctx.Tokens.HasInvaders())
			cmds.Add(Cmd.AddWilds(1));

		// -If you have- 3 Plant: You may do both."

		// you only have to pick if you have 2 but not enough plants
		if(cmds.Count == 2 && !await ctx.YouHave("3 plant" ))
			await Cmd.Pick1<TargetSpaceCtx>( cmds.ToArray() ).Execute( ctx );
		else
			// otherwise, just execute all of them
			foreach(IExecuteOn<TargetSpaceCtx> cmd in cmds )
				await cmd.Execute( ctx );

	}

}
