namespace SpiritIsland.Basegame;

public class ElementalBoon {

	[MinorCard( "Elemental Boon", 1),Fast,AnySpirit]
	[Instructions( "Target Spirit gains 3 different Elements of their choice. If you target another Spirit, you also gain the chosen Elements." ), Artist( Artists.MoroRogers )]
	static public async Task Act( TargetSpiritCtx ctx ) {

		// Target Spirit gains 3 _different_ Elements of their choice
		Element[] elements = await Gain3DifferentElements( ctx.Other );

		// if you target another spirit, you also gain the chosen elements
		if(ctx.Other != ctx.Self)
			ctx.Self.Elements.AddRange( elements );
	}

	static async Task<Element[]> Gain3DifferentElements( Spirit spirit ) {
		var selected = await spirit.SelectElementsEx( 3, ElementList.AllElements );
		spirit.Elements.AddRange( selected );
		return selected;
	}

}