//using SpiritIsland;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using System.Windows.Forms;

//namespace SpiritIsland.WinForms {

//	public partial class SpiritControl : Control {

//		#region Constructor / Init

//		public SpiritControl() {
//			InitializeComponent();

//			this.BackColor = Color.LightYellow;
//			this.Cursor = Cursors.Default;
//		}

//		public void Init( Spirit spirit, string presenceColor, IHaveOptions optionProvider ) {
//			this.spirit = spirit;

//			this.presenceColor = presenceColor ?? throw new ArgumentNullException( nameof( presenceColor ) );

//			optionProvider.NewDecision += OptionProvider_OptionsChanged;
//		}


//		#endregion

//		SpiritLayout spiritLayout;

//		public event Action<IOption> OptionSelected;

//		protected override void OnPaint( PaintEventArgs pe ) {
//			base.OnPaint( pe );
//			if(spirit != null)
//				DrawSpirit( pe.Graphics );
//		}

//		void DrawSpirit( Graphics graphics ) {

//			Rectangle bounds = FitClientBounds(ClientRectangle);

//			// Layout
//			if(spiritLayout == null)
//				spiritLayout = new SpiritLayout( graphics, spirit, bounds, margin );

//			// == Record Hotspots ==
//			hotSpots.Clear();
//			// Growth Options/Actions
//			foreach(var opt in selectableGrowthOptions)
//				hotSpots.Add( opt, spiritLayout.growthLayout[opt] );
//			foreach(var act in selectableGrowthActions)
//				hotSpots.Add( act, spiritLayout.growthLayout[act] );
//			// Presence
//			foreach(var track in clickableTrackOptions)
//				hotSpots.Add( track, spiritLayout.trackLayout.ClickRectFor( track ) );
//			// Innates
//			foreach(var power in selectableInnateOptions)
//				hotSpots.Add( power, spiritLayout.innateLayouts[power].TotalInnatePowerBounds );

//			new SpiritPainter(spirit).Paint( graphics, spiritLayout, 
//				selectableInnateOptions, 
//				selectableGrowthOptions, 
//				selectableGrowthActions,
//				clickableTrackOptions,
//				presenceColor
//			);
//		}

//		Rectangle FitClientBounds(Rectangle bounds) {
//			const float windowRatio = 1.05f;
//			if(bounds.Width > bounds.Height / windowRatio) {
//				int widthClip = bounds.Width - (int)(bounds.Height / windowRatio);
//				bounds.X += widthClip;
//				bounds.Width -= widthClip;
//			} else
//				bounds.Height = (int)(bounds.Width * windowRatio);
//			return bounds;
//		}

//		#region UI event handlers

//		void OptionProvider_OptionsChanged( IDecision decision ) {

//			clickableTrackOptions   = decision.Options.OfType<Track>().ToArray();
//			selectableInnateOptions = decision.Options.OfType<InnatePower>().ToArray();
//			selectableGrowthOptions = decision.Options.OfType<GrowthOption>().ToArray();
//			selectableGrowthActions = decision.Options.OfType<GrowthActionFactory>().ToArray();

//			this.Invalidate();
//		}

//		protected override void OnSizeChanged( EventArgs e ) {
//			base.OnSizeChanged( e );
//			spiritLayout = null;
//			this.Invalidate();
//		}

//		protected override void OnMouseMove( MouseEventArgs e ) {
//			base.OnMouseMove( e );
//			Point clientCoord = this.PointToClient( Control.MousePosition );
//			Cursor = HitTest( clientCoord ) != null ? Cursors.Hand : Cursors.Arrow;
//		}

//		protected override void OnClick( EventArgs e ) {
//			base.OnClick( e );
//			Point clientCoord = this.PointToClient( Control.MousePosition );
//			var option = HitTest( clientCoord );
//			if(option != null)
//				OptionSelected?.Invoke(option);

//			if(spiritLayout.imgBounds.Contains( clientCoord )) {
//				string msg = this.spirit.SpecialRules.Select(r=>r.ToString()).Join("\r\n\r\n");
//				MessageBox.Show( msg );
//			}
//		}

//		IOption HitTest( Point clientCoord ) {
//			return hotSpots.Keys.FirstOrDefault(key=>hotSpots[key].Contains(clientCoord));
//		}

//		#endregion

//		#region private fields

//		const int margin = 10;

//		string presenceColor;
//		Track[] clickableTrackOptions;
//		InnatePower[] selectableInnateOptions;
//		GrowthOption[] selectableGrowthOptions;
//		GrowthActionFactory[] selectableGrowthActions;
//		Spirit spirit;

//		readonly Dictionary<IOption, RectangleF> hotSpots = new();

//		#endregion

//	}

//}
