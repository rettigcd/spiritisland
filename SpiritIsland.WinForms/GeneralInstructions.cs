using System.ComponentModel;
using System.Drawing;

namespace SpiritIsland.WinForms;

/// <summary>
/// Optional Extra text at top of Innate Powers.
/// </summary>
public class GeneralInstructions : IPaintableRect {

	public PaddingSpec Padding = PaddingSpec.None;

	public GeneralInstructions( string description, SizeF relRowSize ){
		_description = description;
		_relRowSize = relRowSize;
	}	

	public float? WidthRatio => _widthRatio ??= Size.Width *1f / Size.Height;
	public Size Size => _size ??= CalcSize();
	
	public void Paint( Graphics g, Rectangle bounds ) {
		var content = Padding.Content(bounds);
		using Image img = ResourceImages.Singleton.GetGeneralInstructions( _description, _relRowSize, content.Width );
		_size = Padding.Pad(img.Size); _widthRatio = null; // update to current
		bounds = bounds.FitBoth(img.Size);
		g.DrawImage( img, content );
	}

	Size CalcSize() {
		using Image img = ResourceImages.Singleton.GetGeneralInstructions( _description, _relRowSize );
		return Padding.Pad(img.Size);
	}

	#region private

	// Lazy
	float? _widthRatio;
	Size? _size;

	readonly string _description;
	readonly SizeF _relRowSize;

	#endregion private
}

