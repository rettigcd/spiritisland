namespace SpiritIsland.Maui;

/// <summary> Displays the # of 1 element. </summary>
public class ElementModel : ObservableModel {

	#region Static factory

	static public ElementModel[] FromDict(CountDictionary<Element> dict) 
		=> ElementList.AllElements
			.Where(el=>0<dict[el])
			.Select(el=>new ElementModel(el, dict[el]))
			.ToArray();
	#endregion Static factory

	#region Observable properties 

	// !!! Can these just be Readonly instead of observable?
	public Element Element   { get => _element; private set => SetProp( ref _element, value ); }
	public int Count         { get => _count;   private set => SetProp( ref _count,   value ); } 

	#endregion Observable properties 

	public ElementModel(Element el,int count=0) {
		Element = el;
		Count = count;
	}

	Element _element;
	int _count;
}
