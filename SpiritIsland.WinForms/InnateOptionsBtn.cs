using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms;


public class InnateOptionsBtn : IButton {

	#region constructor

	public InnateOptionsBtn( Spirit spirit, IDrawableInnateOption innateOption ) {
		_spirit = spirit;
		_innateOption = innateOption;
	}

	#endregion

	public Rectangle Bounds { get; private set; }

	public void Paint( Graphics graphics, bool enabled ) {

		if(enabled) {
			ControlPaint.DrawButton( graphics, Bounds.InflateBy(3), ButtonState.Normal );
			using Pen highlightPen = new( Color.Red, 2f ); 
			graphics.DrawRectangle( highlightPen, Bounds.InflateBy( 1 ) );
		}

		if( _innateOption.IsActive( _spirit.Elements ) )
			graphics.FillRectangle( Brushes.PeachPuff, Bounds );

		using Image img = UsingImage;
		graphics.DrawImage( img, Bounds );

	}

	public InnateOptionsBtn SetPosition( float emSize, Size rowSize, Point topLeft ) {
		_emSize = emSize;
		_rowSize = rowSize;
		using Image img = UsingImage;
		Bounds = new Rectangle( topLeft.X, topLeft.Y, rowSize.Width, rowSize.Width * img.Height / img.Width );
		return this;
	}

	void IButton.SyncDataToDecision( IDecision _ ) { }

	#region private

	Image UsingImage => ResourceImages.Singleton.GetInnateOption( _innateOption, _emSize, _rowSize );

	float _emSize;
	Size _rowSize;

	readonly Spirit _spirit;
	readonly IDrawableInnateOption _innateOption;

	#endregion

}
