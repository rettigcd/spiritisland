using System.Drawing;
using System.Windows.Forms;

namespace SpiritIsland.WinForms;

class AdversaryRect(AdversaryConfig _adversary) : IPaintableRect, IClickableLocation {

	public float? WidthRatio => 234f/148f; // image dimensions

	/// <summary> Records where it was painted, so we can click on it. </summary>
	public Rectangle Bounds {get; private set;}

	public void Click(){
		IAdversary adv = ConfigureGameDialog.GameBuilder.BuildAdversary( _adversary );
		MessageBox.Show( adv.Describe() );
	}
	public bool Contains( Point point ) => Bounds.Contains(point);

	public void Paint(Graphics graphics, Rectangle bounds) {
		using Bitmap flag = ResourceImages.Singleton.AdversaryFlag( _adversary.Name );
		Bounds = bounds.FitBoth(flag.Size);
		graphics.DrawImage( flag, Bounds );
	}
}
