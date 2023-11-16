namespace SpiritIsland;

/// <summary>
/// Marks executable Executable options
/// </summary>
[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class InnateTierAttribute : Attribute, IDrawableInnateTier {

	public InnateTierAttribute( string elementText, string description, int group = 0 ) {
		Elements = ElementCounts.Parse( elementText );
		Description = description;
		Group = group;
	}

	/// <summary>
	/// Non-executable.  Called from dirived class
	/// </summary>
	protected InnateTierAttribute(string elementText, string description ) {
		Elements = ElementCounts.Parse( elementText );
		Description = description;
		Group = null;
	}

	public virtual string ThresholdString => Elements.BuildElementString(); // overridden by Volcano

	public ElementCounts Elements { get; }

	public string Description { get; }

	// Element attributes are evaluated against their group so each group can have 1 method to activate.
	// Null for non-execution groups
	public int? Group { get; }

	string IOption.Text => Elements.BuildElementString() + " - " + Description;

	public virtual bool IsActive( ElementCounts activatedElements ) => activatedElements.Contains( Elements );

}

[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
public class DisplayOnlyAttribute : InnateTierAttribute {

	/// <summary>
	/// Create a Display-only action because execution group is null.
	/// </summary>
	public DisplayOnlyAttribute( string elementText, string description )
		// Call the display-only base constructor
		:base(elementText,description) { }

}

public interface IDrawableInnateTier : IOption {

	/// <summary> The threshold to display to the left of the description </summary>
	/// <remarks> Not using .Elements directly because some thresholds are not elements.</remarks>
	string ThresholdString { get; }

	string Description { get; }

	ElementCounts Elements { get; }

	bool IsActive( ElementCounts activatedElements );
}
