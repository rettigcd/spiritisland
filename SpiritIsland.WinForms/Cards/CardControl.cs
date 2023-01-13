using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpiritIsland.WinForms;

public partial class CardControl : Control {

	public CardControl() {
		InitializeComponent();
		SetStyle(ControlStyles.AllPaintingInWmPaint 
			| ControlStyles.UserPaint 
			| ControlStyles.OptimizedDoubleBuffer 
			| ControlStyles.ResizeRedraw, true
		);
		Cursor = Cursors.Default;
		_data = new CardUi();
		_data.CardClicked += (x) => CardSelected?.Invoke(x);
		_data.AppearanceChanged += () => Invalidate();
	}

	public void Init( Spirit spirit,IHaveOptions iHaveOptions ) {
		iHaveOptions.NewDecision += Options_NewDecision;
		_data.SpiritCardInfo = new SpiritCardInfo( spirit );
	}

	#region Paint / Draw

	protected override void OnPaint( PaintEventArgs pe ) {
		base.OnPaint( pe );
		_data?.DrawParts( pe.Graphics );
	}

	#endregion

	void Options_NewDecision( IDecision decision ) {
		if(_data == null) return;
		_data.HandleNewDecision( decision );
		this.Invalidate();
	}

	protected override void OnResize( EventArgs e ) {
		base.OnResize( e );
		if(_data != null)
			_data.Layout = new CardLayout(new Rectangle(0, 0, Width, Height) );
	}

	protected override void OnClick( EventArgs e ) {

		var coords = PointToClient( Control.MousePosition );
		var clickAction = _data.GetClickAction(coords );
		clickAction?.Invoke();
	}

	protected override void OnMouseMove( MouseEventArgs e ) {
		base.OnMouseMove( e );

		Point coords = PointToClient( Control.MousePosition );
		this.Cursor = _data.GetClickAction( coords ) != null ? Cursors.Hand : Cursors.Default;
	}

	readonly CardUi _data;
	public event Action<PowerCard> CardSelected;
}
