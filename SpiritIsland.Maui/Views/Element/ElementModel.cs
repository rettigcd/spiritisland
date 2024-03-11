namespace SpiritIsland.Maui;

public class ElementModel : ObservableModel {

	#region Static factory

	static public ElementModel[] FromDict(CountDictionary<Element> dict) 
		=> ElementList.AllElements
			.Where(el=>0<dict[el])
			.Select(el=>new ElementModel(el, dict[el]))
			.ToArray();
	#endregion Static factory

	#region Observable properties 

	public Element Element   { get => _element; set => SetProp( ref _element, value ); }
	public int Count         { get => _count;   set => SetProp( ref _count,   value ); } 
//	public Color SelectColor { get => _color;   set => SetProp( ref _color,   value ); }

	#endregion Observable properties 

	public ElementModel(Element el,int count=0) {
		Element = el;
		Count = count;
//		SelectColor = Colors.Transparent;
	}

	Element _element;
	int _count;
	// Color _color;
}
