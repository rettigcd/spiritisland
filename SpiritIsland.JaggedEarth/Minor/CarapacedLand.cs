namespace SpiritIsland.JaggedEarth;

public class CarapacedLand{ 
		
	[MinorCard("Carapaced Land",0,Element.Earth,Element.Plant,Element.Animal),Fast, Range0Or1ForTargetingBeast]
	static public async Task ActAsync( TargetSpaceCtx ctx ){

		// Defend 3
		ctx.Defend( 3 );
		// if you have 2 earth: Defend +3
		if(await ctx.YouHave("2 earth"))
			ctx.Defend( 3 );
	}
}

// If targeting a land with beast, this Power has +1 range.
public class Range0Or1ForTargetingBeast : FromPresenceAttribute {

	public Range0Or1ForTargetingBeast() : base(0, Target.Any ) {}

	public override async Task<object> GetTargetCtx( string powerName, SelfCtx ctx, TargetingPowerType powerType ) {

		var space = await ctx.Self.TargetsSpace( powerType, ctx.GameState, powerName+": Target Space"
			, sourceCriteria
			, new TargetCriteria( range, TargetFilter)
			, new TargetCriteria( range+1, Target.Beast ) // extend 1 for beast
		);
		return space == null ? null : ctx.Target(space);
	}

}