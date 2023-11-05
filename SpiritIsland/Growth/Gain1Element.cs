namespace SpiritIsland;

/// <remarks>Lure of the Deep Wilderness - Air/Plant/Moon, Fractured Days</remarks>
public class Gain1Element : SpiritAction {

	public Gain1Element( params Element[] elementOptions ) 
		: base( "GainElement(" + elementOptions.Select( x => x.ToString() ).Join( "," ) + ")" )
	{
		ElementOptions = elementOptions;
	}

	public override async Task ActAsync( SelfCtx ctx ) {
		var element = ElementOptions.Length == 1 ? ElementOptions[0]
			: await ctx.Self.SelectElementEx( "Gain element", ElementOptions );
		ctx.Self.Elements[element]++;
	}

	public Element[] ElementOptions { get; } // public for drawing

}