﻿using SpiritIsland;
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
			this.images = ResourceImages.Singleton;

			if(spiritImage != null) {
				spiritImage.Dispose();
				spiritImage = null;
			}

			this.presenceColor = presenceColor ?? throw new ArgumentNullException( nameof( presenceColor ) );

			optionProvider.NewDecision += OptionProvider_OptionsChanged;
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

			//=============
			// == Growth ==
			//=============
			int growthHeight = Width / 6;
			int usableWidth = Width - margin * 2;

			var x = ClientRectangle.InflateBy(-margin).SplitVertically( margin, 0.16f, 0.5f, .84f );
			var growthBounds = x[0];
			var presenceBounds = x[1];
			var innateBounds = x[2];
			var elementBounds = x[3];

			graphics.DrawRectangle(Pens.Red,x[0]);
			graphics.DrawRectangle(Pens.Green,x[1]);
			graphics.DrawRectangle(Pens.Blue,x[2]);
			graphics.DrawRectangle(Pens.Black,x[3]);

			//var growthBounds = new Rectangle( margin, margin, usableWidth, growthHeight );
			//var presenceBounds = new Rectangle( margin, growthBounds.Bottom + margin, usableWidth, growthHeight * 2 );
			//var innateBounds = new Rectangle( margin, presenceBounds.Bottom + margin * 2, usableWidth, growthHeight * 2 );
			//var elementBounds = new Rectangle( margin, innateBounds.Bottom + margin, usableWidth, growthHeight );

			Draw_GrowthRow( graphics, growthBounds );
			Draw_PresenceTracks( graphics, simpleFont, highlightPen, presenceBounds );
			Draw_Innates( graphics, innateBounds );
			Draw_Elements( graphics, elementBounds );




		}

		private void Draw_Elements( Graphics graphics, Rectangle elementBounds ) {
			int yy = elementBounds.Y;
			// activated elements
			DrawActivatedElements( graphics, spirit.Elements, ref yy );
			if(spirit is ShiftingMemoryOfAges smoa)
				DrawActivatedElements( graphics, smoa.PreparedElements, ref yy );
		}

		private void Draw_Innates( Graphics graphics, Rectangle innateBounds ) {
			// Innates
			float x = innateBounds.Left;
			int maxHeight = 0;
			int innateWidth = (innateBounds.Width - margin) / 2; // 3 margins => left, center, right
			int y = innateBounds.Y;
			foreach(InnatePower power in spirit.InnatePowers) {
				var sz = DrawSingleInnate( graphics, power, x, y, innateWidth );
				x += (sz.Width + margin);
				maxHeight = Math.Max( maxHeight, (int)sz.Height );
			}
		}

		void Draw_PresenceTracks( Graphics graphics, Font simpleFont, Pen highlightPen, Rectangle bounds ) {
			using Bitmap presence = images.GetPresenceIcon( presenceColor );

			// calc slot width and presence height
			int maxLength = Math.Max( spirit.Presence.CardPlays.TotalCount, spirit.Presence.Energy.TotalCount ) + 2; // +2 for energy & Destroyed

			// Energy
			int energyRowHeight = bounds.Height / 2;
			float slotWidth = bounds.Width / maxLength;
			float presenceWidth = slotWidth * 0.9f;
			SizeF presenceSize = new SizeF( presenceWidth, presenceWidth * presence.Height / presence.Width );

			var trackPainter = new EnergyTrackPainter( graphics, spirit, presence, presenceSize, simpleFont, highlightPen, trackOptions, hotSpots );

			// Energy / Turn
			trackPainter.DrawEnergyRow( slotWidth, bounds.X, bounds.Y, bounds.Width, energyRowHeight );

			// Destroyed
			int y = bounds.Y + energyRowHeight + margin;
			DrawDestroyed( graphics, highlightPen, presence, slotWidth, presenceSize, ClientRectangle.Width - 2.5f * slotWidth, y + slotWidth * .5f );

			// Card Plays / Turn
			trackPainter.DrawCardPlayTrack( slotWidth, margin, y );
		}

		void Draw_GrowthRow( Graphics graphics, Rectangle bounds ) {
			var imgBounds = new Rectangle( bounds.X, bounds.Y, bounds.Height * 3 / 2, bounds.Height );
			var growthBounds = new Rectangle( bounds.X + imgBounds.Width + margin, bounds.Y, bounds.Width - imgBounds.Width - margin, bounds.Height );

			// Draw
			DrawSpiritImage( graphics, imgBounds );
			var growthPainter = new GrowthPainter( graphics );
			growthPainter.Paint( spirit.GrowthOptions, growthBounds );

			// highlight growth
			using Pen highlightPen = new( Color.Red, 8f );
			foreach(var (opt, rect) in growthPainter.layout.EachGrowth()) {
				if(growthOptions.Contains( opt )) {
					hotSpots.Add( opt, rect );
					graphics.DrawRectangle( highlightPen, rect.ToInts() );
				}
			}

			// highlight growth - action
			foreach(var (opt, rect) in growthPainter.layout.EachAction()) {
				if(growthActions.Contains( opt )) {
					if(!hotSpots.ContainsKey( opt ))
						hotSpots.Add( opt, rect ); // sometimes growth reuses object, only show highlight the first 1 - for now.
					graphics.DrawRectangle( highlightPen, rect.ToInts() );
				}
			}
		}

		void DrawDestroyed( Graphics graphics, Pen highlightPen, Bitmap presence, float slotWidth, SizeF presenceSize, float x, float cardY ) {
			if(spirit.Presence.Destroyed == 0) return;

			var rect = new Rectangle( (int)(x + slotWidth / 2), (int)(cardY), (int)presenceSize.Width, (int)presenceSize.Height );

			// Highlight Option
			if(trackOptions.Contains( Track.Destroyed )) {
				graphics.DrawEllipse( highlightPen, rect );
				hotSpots.Add( Track.Destroyed, rect );
			}

			// Presence & Red X
			graphics.DrawImage( presence, rect );
			using var redX = images.GetIcon( "red-x" );
			graphics.DrawImage( redX, rect.X, rect.Y, rect.Width * 2 / 3, rect.Height * 2 / 3 );
			// count
			graphics.DrawCount( rect, spirit.Presence.Destroyed );
		}

		void DrawSpiritImage( Graphics graphics, Rectangle bounds ) {
			var image = spiritImage ??= LoadSpiritImage();
			SpiritLocation = bounds.FitBoth(image.Size);
			graphics.DrawImage(image, SpiritLocation );
		}
		Rectangle SpiritLocation;

		SizeF DrawSingleInnate( Graphics graphics, InnatePower power, float x, float y, float width ) {
			bool isActive = innateOptions.Any( x => x.Name == power.Name );

			// Draw Dynamic Innates
			using var innatePainter = new InnatePainter( graphics, width );

			InnateMetrics metrics = innatePainter.CalcMetrics( power, x, y );

			innatePainter.DrawFromMetrics( power, metrics, spirit.Elements, isActive );

			if(isActive)
				hotSpots.Add( innateOptions.Single( x => x.Name == power.Name ), metrics.TotalInnatePowerBounds.ToInts() );

			return metrics.TotalInnatePowerBounds.Size;
		}

		void DrawActivatedElements( Graphics graphics, CountDictionary<Element> elements, ref int y ) {
			y += 20;
			const float elementSize = 40f;
			float x = margin;

			var orderedElements = elements.Keys.OrderBy( el => (int)el );
			foreach(var element in orderedElements) {
				var rect = new RectangleF(x,y,elementSize,elementSize);
				graphics.DrawImage( GetElementImage( element ), rect );
				graphics.DrawCount( rect, elements[element]);
				x += elementSize;
				x += 15;
			}
			y += (int)elementSize;
		}

		Image LoadSpiritImage() {
			string filename = spirit.Text.Replace( ' ', '_' );
			return Image.FromFile( $".\\images\\spirits\\{filename}.jpg" );
		}
		Image spiritImage;

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

		ResourceImages images;

		const int margin = 10;

		string presenceColor;
		Track[] trackOptions;
		InnatePower[] innateOptions;
		GrowthOption[] growthOptions;
		GrowthActionFactory[] growthActions;
		Spirit spirit;

		readonly Dictionary<Element, Image> elementImages = new();

		readonly Dictionary<IOption, RectangleF> hotSpots = new();

		#endregion

	}

}
