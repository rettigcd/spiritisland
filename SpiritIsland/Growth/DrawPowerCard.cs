namespace SpiritIsland;

public class DrawPowerCard : GrowthActionFactory {

	public readonly int count;
	public DrawPowerCard(int count=1){ 
		this.count = count;
	}

	public override async Task ActivateAsync( SelfCtx ctx ) {
		for(int i=0;i<count;++i)
			await ctx.Self.Draw();
	}

}