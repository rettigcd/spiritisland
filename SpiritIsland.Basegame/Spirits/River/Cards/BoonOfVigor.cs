namespace SpiritIsland.Basegame;

public class BoonOfVigor {

	public const string Name = "Boon of Vigor";
	[SpiritCard(BoonOfVigor.Name, 0, Element.Sun,Element.Water,Element.Plant),Fast,AnySpirit]
	static public Task ActionAsync( TargetSpiritCtx ctx){
		if(ctx.Self == ctx.Other)
			ctx.Self.Energy++;
		else
			ctx.Other.Energy += ctx.Other.InPlay.Count;
		return Task.CompletedTask;
	}

}