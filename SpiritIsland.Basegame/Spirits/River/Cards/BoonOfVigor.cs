namespace SpiritIsland.Basegame;

public class BoonOfVigor {

	public const string Name = "Boon of Vigor";

	[SpiritCard(BoonOfVigor.Name, 0, Element.Sun,Element.Water,Element.Plant),Fast,AnySpirit]
	[Instructions( "If you target yourself, gain 1 Energy. If you target another Spirit, they gain 1 Energy per Power Card they played this turn."), Artist(Artists.NolanNasser)]
	static public Task ActAsync( TargetSpiritCtx ctx){
		if(ctx.Self == ctx.Other)
			ctx.Self.Energy++;
		else
			ctx.Other.Energy += ctx.Other.InPlay.Count;
		return Task.CompletedTask;
	}

}
