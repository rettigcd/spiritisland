using SpiritIsland;
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
			this.images = ResourceImages.Singleton;

			this.presenceColor = presenceColor ?? throw new ArgumentNullException( nameof( presenceColor ) );

			InitElementDisplayOrder( spirit );

			optionProvider.NewDecision += OptionProvider_OptionsChanged;
		}

		void InitElementDisplayOrder( Spirit spirit ) {
			static Element[] Highest( InnatePower power ) => power.GetTriggerThresholds()
				.OrderByDescending( list => list.Length )
				.First();

			int i = 0;
			foreach(var innate in spirit.InnatePowers)
				foreach(var el in Highest( innate ))
					if(!elementOrder.ContainsKey( el )) elementOrder[el] = i++;
			foreach(Element el in Enum.GetValues( typeof( Element ) ))
				if(!elementOrder.ContainsKey( el )) elementOrder[el] = i++;
		}

		#endregion

		public event Action<IOption> OptionSelected;

		#region Draw

		protected override void OnPaint( PaintEventArgs pe ) {
			base.OnPaint( pe );
			if(spirit != null)
				DrawSpirit( pe.Graphics );
		}


		void DrawSpirit( Graphics graphics ) {

			hotSpots.Clear();

			using Font simpleFont = new( "Arial", 8, FontStyle.Bold, GraphicsUnit.Point );

			using Brush coverBrush = new SolidBrush( Color.FromArgb( 128, Color.Gray ) );
			Brush currentBrush = Brushes.Yellow;
			using Pen highlightPen = new( Color.Red, 8f );

			// Load Presence image
			using Bitmap presence = images.GetPresenceIcon(presenceColor);

			// calc slot width and presence height
			int maxLength = Math.Max( spirit.Presence.CardPlays.TotalCount, spirit.Presence.Energy.TotalCount ) + 2; // +2 for energy & Destroyed

			// Calc Presence and coin widths
			float usableWidth = (Width - 2 * margin);
			float slotWidth = usableWidth / maxLength;
			float presenceWidth = slotWidth * 0.9f;
			SizeF presenceSize = new SizeF( presenceWidth, presenceWidth * presence.Height / presence.Width );

			int y = margin;
			// Image
			var imageSz = DrawSpiritImage( graphics, margin, y );

			var painter = new GrowthPainter(graphics);
			var growthHeight = painter.Paint(spirit.GrowthOptions,margin+imageSz.Width,y,Width-imageSz.Width-margin*2);
			// highlight growth
			foreach(var (opt,rect) in painter.layout.EachGrowth()) {
				if(growthOptions.Contains( opt )) {
					hotSpots.Add(opt,rect);
					graphics.DrawRectangle(highlightPen,rect.ToInts());
				}
			}
			// highlight growth - action
			foreach(var (opt, rect) in painter.layout.EachAction()) {
				if(growthActions.Contains( opt )) {
					if(!hotSpots.ContainsKey(opt))
						hotSpots.Add( opt, rect ); // sometimes growth reuses object, only show highlight the first 1 - for now.
					graphics.DrawRectangle( highlightPen, rect.ToInts() );
				}
			}


			y += Math.Max(imageSz.Height,(int)growthHeight);
			y += margin;

			// Energy
			var trackPainter = new EnergyTrackPainter( graphics, spirit, presence, presenceSize, simpleFont, highlightPen, trackOptions, hotSpots );
			y += trackPainter.DrawEnergyRow( slotWidth, margin, y, usableWidth ).Height;
			y += margin;

			// Cards
			y += (int)trackPainter.DrawCardPlayTrack( slotWidth, margin, y );
			y += margin;
			y += margin;

			// Innates
			float x = margin;
			int maxHeight = 0;
			int innateWidth = (Width - 3 * margin) / 2; // 3 margins => left, center, right
			foreach(InnatePower power in spirit.InnatePowers ) {
				var sz = DrawInnates( graphics, power, highlightPen, x, y, innateWidth );
				x += (sz.Width + margin);
				maxHeight = Math.Max( maxHeight, (int)sz.Height );
			}
			y += (maxHeight + margin);

			// activated elements
			DrawActivatedElements( graphics, y );

		}

		Size DrawSpiritImage( Graphics graphics, int x, int y ) {
			var image = spiritImage ??= LoadSpiritImage();
			SpiritLocation = new Rectangle(x,y,180,120);
			graphics.DrawImage(image, SpiritLocation );
			return SpiritLocation.Size;
		}
		Rectangle SpiritLocation;

		SizeF DrawInnates( Graphics graphics, InnatePower power, Pen highlightPen, float x, float y, float width ) {

			// Draw Dynamic Inates
			using var innatePainter = new InnatePainter( graphics, width );
			RectangleF bounds = innatePainter.Paint( spirit, power, x, y ); // Calcs Metrics and paints
			if(innateOptions.Any( x => x.Name == power.Name )) {
				graphics.DrawRectangle( highlightPen, bounds.ToInts() );
				hotSpots.Add( innateOptions.Single( x => x.Name == power.Name ), bounds.ToInts() );
			}
			return bounds.Size;
		}

		void DrawActivatedElements( Graphics graphics, float y ) {
			y += 20;
			const float elementSize = 40f;
			var elements = spirit.Elements; // cache, don't recalculate
			float x = margin;

			var orderedElements = elements.Keys.OrderBy( el => elementOrder[el] );
			foreach(var element in orderedElements) {
				var rect = new RectangleF(x,y,elementSize,elementSize);
				graphics.DrawImage( GetElementImage( element ), rect );
				graphics.DrawCount( rect, elements[element]);
				x += elementSize;
				x += 15;
			}
		}

		Image LoadSpiritImage() {
			string filename = spirit.Text.Replace( ' ', '_' );
			return Image.FromFile( $".\\images\\spirits\\{filename}.png" );
		}
		Image spiritImage;

		//Image GetInnateImage( string innateCardName ) {
		//	if(!innateImages.ContainsKey( innateCardName )) {
		//		string filename = innateCardName.Replace( ' ', '_' ).Replace( "'", "" ).ToLower();
		//		Image image = Image.FromFile( $".\\images\\innates\\{filename}.jpg" );
		//		innateImages.Add( innateCardName, image );
		//	}
		//	return innateImages[innateCardName];
		//}

		Image GetElementImage( Element element ) {

			if(!elementImages.ContainsKey( element )) {
				string filename = "Simple_" + element.ToString().ToLower();
				Image image = ResourceImages.Singleton.GetTokenIcon(filename); //  Image.FromFile( $".\\images\\tokens\\{filename}.png" );
				elementImages.Add( element, image );
			}
			return elementImages[element];
		}

		#endregion

		#region UI event handlers

		void OptionProvider_OptionsChanged( IDecision decision ) {

			trackOptions = decision.Options.OfType<Track>().ToArray();

			innateOptions = decision.Options
				.OfType<InnatePower>()
				.ToArray();

			growthOptions = decision.Options.OfType<GrowthOption>().ToArray();
			growthActions = decision.Options.OfType<GrowthActionFactory>().ToArray();

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

			if(SpiritLocation.Contains(clientCoord))
				MessageBox.Show(this.spirit.SpecialRules);
		}

		IOption HitTest( Point clientCoord ) {
			return hotSpots.Keys.FirstOrDefault(key=>hotSpots[key].Contains(clientCoord));
		}

		#endregion

		#region private fields

		ResourceImages images;

		const int margin = 10;

		string presenceColor;
		Track[] trackOptions;
		InnatePower[] innateOptions;
		GrowthOption[] growthOptions;
		GrowthActionFactory[] growthActions;
		Spirit spirit;

		readonly Dictionary<Element, Image> elementImages = new();
		readonly Dictionary<Element, int> elementOrder = new();

		readonly Dictionary<IOption, RectangleF> hotSpots = new();

		#endregion

	}

}
