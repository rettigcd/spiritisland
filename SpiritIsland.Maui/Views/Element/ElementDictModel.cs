namespace SpiritIsland.Maui;

public class ElementDictModel( CountDictionary<Element> elementsDict ) : IEquatable<ElementDictModel> {

	public ElementModel[] Elements { get; } = ElementModel.FromDict(elementsDict);

	#region GetHashCode / Equals

	static public bool operator==(ElementDictModel x, ElementDictModel y) => ReferenceEquals(x, y) || x is not null && x.Equals(y);
	static public bool operator!=(ElementDictModel x, ElementDictModel y) => !(x==y);

	public override bool Equals(object? obj) => Equals(obj as ElementDictModel);

	public bool Equals(ElementDictModel? other) => ReferenceEquals(other,this) || other is not null && other._str == _str;

	public override int GetHashCode() => _str.GetHashCode();

	readonly string _str = elementsDict.BuildElementString();

	#endregion GetHashCode / Equals
}