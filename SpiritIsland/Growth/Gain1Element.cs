namespace SpiritIsland;

/// <remarks>Lure of the Deep Wilderness - Air/Plant/Moon, Fractured Days</remarks>
public class Gain1Element : GrowthActionFactory {

	public Element[] ElementOptions { get; } // public for drawing

	public Gain1Element(params Element[] elementOptions ) {
		this.ElementOptions = elementOptions;
	}

	public override async Task ActivateAsync( SelfCtx ctx ) {
		var element = ElementOptions.Length == 1 ? ElementOptions[0]
			: await ctx.Self.SelectElementEx( "Gain element", ElementOptions );
		ctx.Self.Elements[element]++;
	}

	public override string Name => "GainElement("+ElementOptions.Select(x=>x.ToString()).Join(",")+")";
}