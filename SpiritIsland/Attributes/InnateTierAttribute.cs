namespace SpiritIsland;

/// <summary>
/// Marks executable Executable options
/// </summary>
[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class InnateTierAttribute : Attribute, IDrawableInnateTier {

	public InnateTierAttribute( string elementText, string description, int group = 0 ) {
		Elements = ElementStrings.Parse( elementText );
		Description = description;
		Group = group;
	}

	/// <summary>
	/// Non-executable.  Called from dirived class
	/// </summary>
	protected InnateTierAttribute(string elementText, string description ) {
		Elements = ElementStrings.Parse( elementText );
		Description = description;
		Group = null;
	}

	public virtual string ThresholdString => Elements.BuildElementString(); // overridden by Volcano

	public CountDictionary<Element> Elements { get; }

	public string Description { get; }

	// Element attributes are evaluated against their group so each group can have 1 method to activate.
	// Null for non-execution groups
	public int? Group { get; }

	string IDrawableInnateTier.Text => Elements.BuildElementString() + " - " + Description;

	public virtual bool IsActive( ElementMgr spiritElements ) => spiritElements.ContainsRaw( Elements );

}

/// <summary>
/// Create a Display-only action because execution group is null.
/// </summary>
[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
public class DisplayOnlyAttribute( string elementText, string description ) : InnateTierAttribute(elementText,description) {
}

// !!! I am 90% certain that the Innate Options are never presented as OPTIONS and that IOption can be removed here. 
// !!! Also, InnateTierBtn, should not register with the ClickContainer, nor be IAmEnableable
public interface IDrawableInnateTier {

	string Text {get;}

	/// <summary> The threshold to display to the left of the description </summary>
	/// <remarks> Not using .Elements directly because some thresholds are not elements.</remarks>
	string ThresholdString { get; }

	string Description { get; }

	CountDictionary<Element> Elements { get; }

	bool IsActive( ElementMgr activatedElements );
}
