namespace SpiritIsland;

/// <remarks>User selects 1 element they want to gain.</remarks>
public class Gain1Element( params Element[] elementOptions ) 
	: SpiritAction()
	, ICanAutoRun
{

	public override string Description => "Gain " + ElementOptions
		.Select( x => x.ToString() )
		.Join_WithLast(", ", ", or " );

	public override async Task ActAsync( Spirit self ) {
		var element = ElementOptions.Length == 1 ? ElementOptions[0]
			: await self.SelectElementEx( "Gain element", ElementOptions );
		self.Elements.Add(element);
	}

	public Element[] ElementOptions { get; } = elementOptions;

}