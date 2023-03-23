namespace SpiritIsland.BranchAndClaw;

public class HereThereBeMonsters {

	[MinorCard( "Here There Be Monsters", 0, Element.Moon, Element.Air, Element.Animal ),Slow,FromPresence( 0, Target.Inland )]
	[Instructions( "You may Push 1 Explorer / Town / Dahan. 2 Fear. If target land has any Beasts, 1 Fear." ),Artist(Artists.JoshuaWright)]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		// you may push 1 explorer / town / dahan
		await ctx.PushUpTo(1,Human.Explorer_Town.Plus(Human.Dahan));
		// 2 fear
		ctx.AddFear(2);
		// if target land has any beasts, 1 fear
		if( ctx.Beasts.Any )
			ctx.AddFear(1);
	}

}