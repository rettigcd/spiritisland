namespace SpiritIsland;

/// <summary>
/// Marks executable Executable options
/// </summary>
[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class InnateOptionAttribute : Attribute, IDrawableInnateOption {

	public InnateOptionAttribute( string elementText, string description, int group = 0 ) {
		Elements = ElementCounts.Parse( elementText );
		Description = description;
		Group = group;
	}

	/// <summary>
	/// Non-executable.  Called from dirived class
	/// </summary>
	protected InnateOptionAttribute(string elementText, string description ) {
		Elements = ElementCounts.Parse( elementText );
		Description = description;
		Group = null;
	}

	public ElementCounts Elements { get; }

	public string Description { get; }

	// Element attributes are evaluated against their group so each group can have 1 method to activate.
	// Null for non-execution groups
	public int? Group { get; }

	string IOption.Text => Elements.BuildElementString() + " - " + Description;
}

[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
public class DisplayOnlyAttribute : InnateOptionAttribute {

	/// <summary>
	/// Create a Display-only action because execution group is null.
	/// </summary>
	public DisplayOnlyAttribute( string elementText, string description )
		// Call the display-only base constructor
		:base(elementText,description) { }

}

public interface IDrawableInnateOption : IOption {
	ElementCounts Elements { get; }
	string Description { get; }
}
