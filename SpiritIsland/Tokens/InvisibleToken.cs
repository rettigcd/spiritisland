namespace SpiritIsland;

public class InvisibleToken : ISpaceEntity, IEntityClass {

	public InvisibleToken( string label, TokenCategory cat = TokenCategory.None ) {
		this.Label = label;
		this.Category = cat;
	}

	#region Token

	IEntityClass ISpaceEntity.Class => this;

	#endregion

	#region TokenGroup

	public string Label { get; }

	public TokenCategory Category { get; }

//	string IOption.Text => Label;

	#endregion

}