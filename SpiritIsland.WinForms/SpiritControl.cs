using SpiritIsland;
using SpiritIsland.JaggedEarth;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms {

	public partial class SpiritControl : Control {

		#region Constructor / Init

		public SpiritControl() {
			InitializeComponent();

			this.BackColor = Color.LightYellow;
			this.Cursor = Cursors.Default;
		}

		public void Init( Spirit spirit, string presenceColor, IHaveOptions optionProvider ) {
			this.spirit = spirit;

			if(spiritImage != null) {
				spiritImage.Dispose();
				spiritImage = null;
			}

			this.presenceColor = presenceColor ?? throw new ArgumentNullException( nameof( presenceColor ) );

			optionProvider.NewDecision += OptionProvider_OptionsChanged;
		}


		#endregion

		public event Action<IOption> OptionSelected;

		protected override void OnPaint( PaintEventArgs pe ) {
			base.OnPaint( pe );
			if(spirit != null)
				DrawSpirit( pe.Graphics );
		}

		void DrawSpirit( Graphics graphics ) {

			Rectangle bounds = FitClientBounds();

			// Layout
			var spiritLayout = new SpiritLayout( graphics, spirit, bounds, margin );

			// == Record Hotspots ==
			hotSpots.Clear();
			// Growth Options/Actions
			foreach(var opt in selectableGrowthOptions)
				hotSpots.Add(opt,spiritLayout.growthLayout[opt]);
			foreach(var act in selectableGrowthActions)
				hotSpots.Add(act,spiritLayout.growthLayout[act]);
			// Presence
			foreach(var track in clickableTrackOptions)
				hotSpots.Add( track, spiritLayout.trackLayout.ClickRectFor(track) );
			// Innates
			foreach(var power in innateOptions)
				hotSpots.Add( power, spiritLayout.innateLayouts[power].TotalInnatePowerBounds );


			// Draw
			DrawSpiritImage( graphics, spiritLayout.imgBounds );
			new GrowthPainter( graphics ).Paint( spiritLayout.growthLayout, selectableGrowthOptions, selectableGrowthActions );
			new PresenceTrackPainter( spirit, spiritLayout.trackLayout, clickableTrackOptions, graphics ).Paint(presenceColor);
			Draw_Innates( graphics, spiritLayout );
			Draw_Elements( graphics, spiritLayout );
		}

		Rectangle FitClientBounds() {
			const float windowRatio = 1.05f;
			var bounds = ClientRectangle;
			if(bounds.Width > bounds.Height / windowRatio) {
				int widthClip = bounds.Width - (int)(bounds.Height / windowRatio);
				bounds.X += widthClip;
				bounds.Width -= widthClip;
			} else
				bounds.Height = (int)(bounds.Width * windowRatio);
			return bounds;
		}

		#region Draw Growth

		void DrawSpiritImage( Graphics graphics, Rectangle bounds ) {
			var image = spiritImage ??= LoadSpiritImage();
			SpiritLocation = bounds.FitBoth(image.Size);
			graphics.DrawImage(image, SpiritLocation );
		}

		Image LoadSpiritImage() {
			string filename = spirit.Text.Replace( ' ', '_' );
			return Image.FromFile( $".\\images\\spirits\\{filename}.jpg" );
		}

		Image spiritImage;
		Rectangle SpiritLocation;

		#endregion


		#region Draw Innates

		/// <returns>height used</returns>
		void Draw_Innates( Graphics graphics, SpiritLayout spiritLayout ) {
			using var innatePainter = new InnatePainter( graphics );
			foreach(var power in spirit.InnatePowers)
				innatePainter.DrawFromMetrics( power, spiritLayout.innateLayouts[power], spirit.Elements, innateOptions.Contains(power));
		}

		#endregion

		#region Draw Elements

		void Draw_Elements( Graphics graphics, SpiritLayout spiritLayout ) {
			// activated elements
			DrawActivatedElements( graphics, spirit.Elements, spiritLayout.Elements );
			int skip = spirit.Elements.Keys.Count; 
			if(skip>1) skip++; // add a space
			if(spirit is ShiftingMemoryOfAges smoa)
				DrawActivatedElements( graphics, smoa.PreparedElements, spiritLayout.Elements, skip );
		}


		void DrawActivatedElements( Graphics graphics, CountDictionary<Element> elements, ElementLayout elLayout, int skip=0 ) {

			var orderedElements = elements.Keys.OrderBy( el => (int)el );
			int idx = skip;
			foreach(var element in orderedElements) {
				var rect = elLayout.Rect(idx++);
				graphics.DrawImage( GetElementImage( element ), rect );
				graphics.DrawCount( rect, elements[element]);
			}
		}

		Image GetElementImage( Element element ) {

			if(!elementImages.ContainsKey( element )) {
				Image image = ResourceImages.Singleton.GetToken( element );
				elementImages.Add( element, image );
			}
			return elementImages[element];
		}

		#endregion

		#region UI event handlers

		void OptionProvider_OptionsChanged( IDecision decision ) {

			clickableTrackOptions = decision.Options.OfType<Track>().ToArray();

			innateOptions = decision.Options
				.OfType<InnatePower>()
				.ToArray();

			selectableGrowthOptions = decision.Options.OfType<GrowthOption>().ToArray();
			selectableGrowthActions = decision.Options.OfType<GrowthActionFactory>().ToArray();

			this.Invalidate();
		}

		protected override void OnSizeChanged( EventArgs e ) {
			base.OnSizeChanged( e );
			this.Invalidate();
		}

		protected override void OnMouseMove( MouseEventArgs e ) {
			base.OnMouseMove( e );
			Point clientCoord = this.PointToClient( Control.MousePosition );
			Cursor = HitTest( clientCoord ) != null ? Cursors.Hand : Cursors.Arrow;
		}

		protected override void OnClick( EventArgs e ) {
			base.OnClick( e );
			Point clientCoord = this.PointToClient( Control.MousePosition );
			var option = HitTest( clientCoord );
			if(option != null)
				OptionSelected?.Invoke(option);

			if(SpiritLocation.Contains( clientCoord )) {
				string msg = this.spirit.SpecialRules.Select(r=>r.ToString()).Join("\r\n\r\n");
				MessageBox.Show( msg );
			}
		}

		IOption HitTest( Point clientCoord ) {
			return hotSpots.Keys.FirstOrDefault(key=>hotSpots[key].Contains(clientCoord));
		}

		#endregion

		#region private fields

		const int margin = 10;

		string presenceColor;
		Track[] clickableTrackOptions;
		InnatePower[] innateOptions;
		GrowthOption[] selectableGrowthOptions;
		GrowthActionFactory[] selectableGrowthActions;
		Spirit spirit;

		readonly Dictionary<Element, Image> elementImages = new();

		readonly Dictionary<IOption, RectangleF> hotSpots = new();

		#endregion

	}

	public class SpiritLayout {

		public SpiritLayout(Graphics graphics, Spirit spirit, Rectangle bounds, int margin ) {
			var rects = bounds.InflateBy(-margin).SplitVerticallyByWeight( margin, 200f, 360f, 420f, 60f );
			Calc_GrowthRow(spirit,rects[0],margin);
			trackLayout = new PresenceTrackLayout(rects[1],spirit,margin);
			Calc_Innates( spirit, graphics, rects[2],margin );
			Elements = new ElementLayout(rects[3]);
		}

		public Rectangle imgBounds; // Picutre of spirit
		public GrowthLayout growthLayout; // Growth
		public PresenceTrackLayout trackLayout; // presenct tracks
		public Dictionary<InnatePower, InnateLayout> innateLayouts = new Dictionary<InnatePower,InnateLayout>();
		public ElementLayout Elements;


		void Calc_GrowthRow( Spirit spirit, Rectangle bounds, int margin ) {
			// Calc: Layout (image & growth)
			imgBounds = new Rectangle( bounds.X, bounds.Y, bounds.Height * 3 / 2, bounds.Height );
			var growthBounds = new Rectangle( bounds.X + imgBounds.Width + margin, bounds.Y, bounds.Width - imgBounds.Width - margin, bounds.Height );
			growthLayout = new GrowthLayout(spirit.GrowthOptions, growthBounds);
		}

		int Calc_Innates( Spirit spirit, Graphics graphics, Rectangle bounds, int margin ) {

			// Calc: Layout
			int maxHeight = 0;
			var innateBounds = bounds.SplitHorizontallyByWeight(margin,1f,1f); // split equally
			for(int i=0;i<spirit.InnatePowers.Length;++i) {
				var singleBounds = innateBounds[i];
				var power = spirit.InnatePowers[i];
				var layout = new InnateLayout( power, singleBounds.X, singleBounds.Y, singleBounds.Width, graphics );
				innateLayouts[power] = layout;
				maxHeight = Math.Max( layout.TotalInnatePowerBounds.Height, maxHeight );
			}

			return maxHeight;
		}


	}

	public class ElementLayout {
		public ElementLayout(Rectangle bounds ) {
			x = bounds.X;
			y = bounds.Y;
			elementSize = bounds.Height;
			step = elementSize + bounds.Height/10;
		}
		public Rectangle Rect(int index) => new Rectangle(x+step*index,y,elementSize,elementSize);

		readonly int x;
		readonly int y;
		readonly int step;
		readonly int elementSize;
	}


}
