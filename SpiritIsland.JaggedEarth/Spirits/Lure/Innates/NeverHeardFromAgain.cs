namespace SpiritIsland.JaggedEarth;

[InnatePower("Never Heard from Again","If this Power destroyes any explorer, 1 fear.  If this Power destroys 5 or more explorer, +1 fear.")]
[Slow,FromPresence(0,Target.Inland)]
[RepeatIf("6 plant")]
public class NeverHeardFromAgain {

	// If this Power destroys any explorer, 1 Fear
	// if this Power destroys 5 or more explorer, +1 fear
	static int CalcFearFromExplorerDeath( int destroyCount ) {
		return 5 <= destroyCount ? 2
			: 1 <= destroyCount ? 1
			: 0;
	}


	[InnateOption("1 fire,3 air","Add 1 badland",0)]
	static public async Task Option1(TargetSpaceCtx ctx ) {
		// add 1 badland
		await ctx.Badlands.Add(1);
	}

	[InnateOption("2 plant","Destroy up to 2 explorer per badlands/beast/disease/wilds.",1)]
	static public async Task Option2( TargetSpaceCtx ctx ) {
		// 2 plant - destroy up to 2 explorer per badland/beast/disease/wilds
		int destroyCount = await DestroyFromBadlandsBeastDiseaseWilds( ctx );
		ctx.AddFear( CalcFearFromExplorerDeath( destroyCount ) );
	}

	[InnateOption("4 plant,1 animal","2 Damage",1)]
	static public async Task Option3( TargetSpaceCtx ctx ) {
		int preExplorerCount = ctx.Tokens.Sum( Invader.Explorer );
		await DestroyFromBadlandsBeastDiseaseWilds( ctx );
		await ctx.DamageInvaders(2);
		int destroyedExplorers = ctx.Tokens.Sum( Invader.Explorer ) - preExplorerCount;
		ctx.AddFear( CalcFearFromExplorerDeath( destroyedExplorers ) );
	}

	static async Task<int> DestroyFromBadlandsBeastDiseaseWilds( TargetSpaceCtx ctx ) {
		int srcCount = ctx.Badlands.Count + ctx.Beasts.Count + ctx.Disease.Count + ctx.Wilds.Count;
		int destroyCount = Math.Min( srcCount * 2, ctx.Tokens.Sum( Invader.Explorer ) );
		await ctx.Invaders.DestroyNOfClass( destroyCount, Invader.Explorer );
		return destroyCount;
	}

}