namespace SpiritIsland;

/// <remarks>User selects 1 element they want to gain.</remarks>
public class Gain1Element : SpiritAction, ICanAutoRun {

	public Gain1Element( params Element[] elementOptions ) : base()
	{
		ElementOptions = elementOptions;
	}

	public override string Description => "Gain " + ElementOptions
		.Select( x => x.ToString() )
		.Join_WithLast(", ", ", or " );

	public override async Task ActAsync( SelfCtx ctx ) {
		var element = ElementOptions.Length == 1 ? ElementOptions[0]
			: await ctx.Self.SelectElementEx( "Gain element", ElementOptions );
		ctx.Self.Elements.Add(element);
	}

	public Element[] ElementOptions { get; } // public for drawing

}