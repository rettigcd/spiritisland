﻿using SpiritIsland.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms {

    public partial class SpiritControl : Control {

		public SpiritControl() {
			InitializeComponent();
			this.BackColor = Color.LightYellow;
			this.Cursor = Cursors.Default;
		}

		public void Init( Spirit spirit, IHaveOptions optionProvider ) {
			this.spirit = spirit;

			elPos = new Dictionary<Element, int>();
			int i=0;
			foreach(var innate in spirit.InnatePowers)
				foreach(var el in Highest(innate))
					if(!elPos.ContainsKey(el)) elPos[el] = i++;
			foreach(Element el in Enum.GetValues(typeof(Element)) )
				if(!elPos.ContainsKey( el )) elPos[el] = i++;

			optionProvider.OptionsChanged += OptionProvider_OptionsChanged;
		}
		Dictionary<Element, int> elPos;

		static Element[] Highest(InnatePower power) => power.GetTriggerThresholds()
            .OrderByDescending(list=>list.Length)
            .First();

        void OptionProvider_OptionsChanged( IOption[] obj ) {
			var tracks = obj.OfType<Track>().ToArray();
			trackOptions = new HashSet<Track>( tracks );
			innateOptions = obj
				.OfType<IActionFactory>()
				.Select( x => x.Original )
				.OfType<InnatePower>()
				.ToArray();
			this.Invalidate();
		}

		protected override void OnPaint( PaintEventArgs pe ) {
			base.OnPaint( pe );
			if(spirit != null)
				DrawSpirit( pe.Graphics );
		}

		protected override void OnSizeChanged( EventArgs e ) {
			base.OnSizeChanged( e );
			this.Invalidate();
		}

		void DrawSpirit( Graphics graphics ) {

			using Font simpleFont = new( "Arial", 8, FontStyle.Bold, GraphicsUnit.Point );

			using Font font = new( "Arial", 18f );
			using Brush coverBrush = new SolidBrush( Color.FromArgb( 128, Color.Gray ) );
			Brush currentBrush = Brushes.Yellow;
			using Pen highlightPen = new( Color.Red, 10f );

			const float lineHeight = 60f;
			const float deltaX = 45f;

			// Energy
			float x = 10f;
			float y = 10f;
			graphics.DrawString( "Energy", simpleFont, SystemBrushes.ControlDarkDark, x, y );

			y += lineHeight;
			int revealedEnergySpaces = spirit.RevealedEnergySpaces;
			int idx = 0;
			bool highlightEnergy = trackOptions.Contains( Track.Energy );
			foreach(int en in spirit.GetEnergySequence()) {

				if(idx == revealedEnergySpaces - 1)
					graphics.FillEllipse( currentBrush, x, y, deltaX, lineHeight );

				graphics.DrawString( en.ToString(), font, SystemBrushes.ControlDarkDark, x, y );

				if(revealedEnergySpaces <= idx)
					graphics.FillEllipse( coverBrush, x, y, deltaX, lineHeight );

				if(highlightEnergy && revealedEnergySpaces == idx)
					graphics.DrawEllipse( highlightPen, x, y, deltaX, lineHeight );

				x += deltaX;
				++idx;
			}

			// Cards
			x = 10f;
			y += lineHeight;
			graphics.DrawString( "Cards", simpleFont, SystemBrushes.ControlDarkDark, x, y );

			int revealedCardSpaces = spirit.RevealedCardSpaces;
			y += lineHeight;
			idx = 0;
			bool highlightCard = trackOptions.Contains( Track.Card );
			foreach(int en in spirit.GetCardSequence()) {

				if(idx == revealedCardSpaces - 1)
					graphics.FillEllipse( currentBrush, x, y, deltaX, lineHeight );

				graphics.DrawString( en.ToString(), font, SystemBrushes.ControlDarkDark, x, y );

				if(revealedCardSpaces <= idx)
					graphics.FillEllipse( coverBrush, x, y, deltaX, lineHeight );

				if(highlightCard && revealedCardSpaces == idx)
					graphics.DrawEllipse( highlightPen, x, y, deltaX, lineHeight );

				x += deltaX;
				++idx;
			}

			// Innates
			// Innates
			x = 10f;
			y += (lineHeight * 1.25f);

			// This non-sense is because Thunderspeaker has a fast & slow option with the same name.
			string[] innateOptionNames = innateOptions.Select(x=>x.Name).ToArray();
			foreach(var name in spirit.InnatePowers.Select(i=>i.Name).Distinct()) {
				var image = GetInnateImage( name );

				int drawWidth = Width - (int)x * 2;
				int drawHeight = drawWidth * image.Height / image.Width;
				graphics.DrawImage( image, x, y, drawWidth, drawHeight );

				if(innateOptionNames.Contains( name ))
					graphics.DrawRectangle( highlightPen, x, y, drawWidth, drawHeight );

				y += drawHeight;
				y += 10;
			}

			// activated elements
			y += 20;
			const float elementSize = 50f;
			var elements = spirit.Elements; // cache, don't recalculate

			var orderedElements = elements.Keys.OrderBy( el => elPos[el] );
			foreach(var element in orderedElements) {
				if(elements[element] > 1) {
					graphics.DrawString(
						elements[element].ToString(),
						simpleFont,
						Brushes.Black
						, x, y
					);
					x += 20;
				}
				graphics.DrawImage( GetImage( element ), x, y, elementSize, elementSize );
				x += elementSize;
				x += 10;
			}

			// !Note! - If you do not specify output width/height of image, .Net will scale image based on screen DPI and image DPI
		}

		Image GetInnateImage( string innateCardName ) {
			if(!innateImages.ContainsKey( innateCardName )) {
				string filename = innateCardName.Replace( ' ', '_' ).Replace( "'", "" ).ToLower();
				Image image = Image.FromFile( $".\\images\\innates\\{filename}.jpg" );
				innateImages.Add( innateCardName, image );
			}
			return innateImages[innateCardName];
		}

		Image GetImage( Element element ) {

			if(!elementImages.ContainsKey( element )) {
				string filename = "Simple_" + element.ToString().ToLower();
				Image image = Image.FromFile( $".\\images\\tokens\\{filename}.png" );
				elementImages.Add( element, image );
			}
			return elementImages[element];
		}

		#region private fields
		HashSet<Track> trackOptions;
		InnatePower[] innateOptions;
		Spirit spirit;

		readonly Dictionary<string, Image> innateImages = new();
		readonly Dictionary<Element, Image> elementImages = new();
		#endregion

	}
}
