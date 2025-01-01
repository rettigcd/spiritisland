using SpiritIsland.An;
using System.Windows.Input;

namespace SpiritIsland.Maui;

/// <summary>
/// For Elements-as-options.  No #.,
/// </summary>
public class ElementOptionModel : BaseOptionModel {

	public ElementOptionModel(Element element):base(new ElementOption(element, false)) {
		_element = element;
	}
	readonly Element _element;
	public ImageSource Image => _element.GetTokenImg().ImgSource();
}


