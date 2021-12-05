using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms {

	/// <summary>
	/// Binds together for a single painting: Graphics, Spirit(info), Layout, Clickable-Options
	/// </summary>
	class PresenceTrackPainter {

		readonly Graphics graphics;
		Bitmap coin; // transient, only exists for lifetime of Paint()

		readonly Track[] energyTrackStatii;
		readonly Track[] cardTrackStatii;
		readonly Track[] energyRevealed;
		readonly Track[] cardRevealed;

		readonly int energy;
		readonly int destroyed;

		readonly Track[] clickableTrackOptions;
		readonly PresenceTrackLayout metrics;

		public PresenceTrackPainter( 
			Spirit spirit, 
			PresenceTrackLayout metrics, 
			Track[] clickableTrackOptions,
			Graphics graphics
		) {
			this.metrics = metrics;
			this.clickableTrackOptions = clickableTrackOptions;
			this.graphics = graphics;

			var presence = spirit.Presence;
			this.energy = spirit.Energy;
			this.destroyed = spirit.Presence.Destroyed;

			energyTrackStatii = presence.GetEnergyTrackStatus().ToArray();
			energyRevealed = presence.Energy.Revealed.ToArray();
			cardTrackStatii = presence.GetCardPlayTrackStatus().ToArray();
			cardRevealed = presence.CardPlays.Revealed.ToArray();

		}

		public void Paint(string presenceColor) {

			// Bottom Layer
			PaintLabels();
			using(this.coin = ResourceImages.Singleton.GetToken( "coin" )) {
				PaintCurrentEnergy();
				PaintEnergyPerTurnRow();
			}
			PaintCardPlayTrack();

			// Middle Layer
			PaintHighlights();

			// Top Layer
			PaintPresence(presenceColor);

		}

		void PaintHighlights() {
			using Pen highlightPen = new( Color.Red, 8f );
			foreach(var track in clickableTrackOptions)
				graphics.DrawEllipse( highlightPen, metrics.SlotLookup[track].PresenceRect );
		}

		void PaintLabels() {
			using Font simpleFont = new( "Arial", 8, FontStyle.Bold, GraphicsUnit.Point );
			graphics.DrawString( "Energy", simpleFont, SystemBrushes.ControlDarkDark, metrics.EnergyTitleLocation );
			graphics.DrawString( "Cards", simpleFont, SystemBrushes.ControlDarkDark, metrics.CardPlayTitleLocation );
		}

		void PaintPresence(string presenceColor) {
			using Bitmap presenceImg = ResourceImages.Singleton.GetPresenceIcon( presenceColor );

			foreach(var track in energyTrackStatii)
				if(!energyRevealed.Contains(track))
					graphics.DrawImage( presenceImg, metrics.SlotLookup[track].PresenceRect );

			foreach(var track in cardTrackStatii)
				if(!cardRevealed.Contains(track))
					graphics.DrawImage( presenceImg, metrics.SlotLookup[track].PresenceRect );

			PaintDestroyed( presenceImg );
		}

		void PaintEnergyPerTurnRow() {
			foreach(var track in energyTrackStatii)
				DrawCoin( metrics.SlotLookup[track].TrackRect, track );

		}

		void PaintCardPlayTrack() {
			foreach(var track in this.cardTrackStatii)
				DrawCard( track, metrics.SlotLookup[track].TrackRect );
		}

		void DrawCoin( RectangleF bounds, Track track ) {

			if( track.Energy.HasValue) {
				graphics.DrawImage( coin, bounds );
				DrawTextOnCoin(ref bounds, track.Energy.Value.ToString() );
			}

			// Element
			var innerBounds = bounds.InflateBy( -bounds.Height / 5 );
			if(track.Elements.Any())
				DrawElement( ref innerBounds, track.Elements );

			// Action
			if(track.Action != null)
				DrawAction( bounds, track );

		}

		void DrawCard( Track track, RectangleF bounds ) {
			var innerBounds = bounds.InflateBy( -bounds.Height / 5 );

			// Card PLays
			if( track.CardPlay.HasValue) {
				using var bitmap = ResourceImages.Singleton.GetIcon( track.CardPlay + " cardplay" );
				graphics.DrawImageFitHeight( bitmap, bounds );
				innerBounds.X += innerBounds.Width * 0.5f;
				innerBounds.Y += innerBounds.Height * 0.5f;
			}

			// Elements
			if(track.Elements.Any())
				DrawElement( ref innerBounds, track.Elements );

			// Action
			if(track.Action != null)
				DrawAction( innerBounds, track );

		}

		void DrawTextOnCoin( ref RectangleF bounds, string txt ) {
			// Draw Text
			bounds.Y += bounds.Height * .05f; // it looks too high
			Font coinFont = new Font( ResourceImages.Singleton.Fonts.Families[0], bounds.Height * .6f, GraphicsUnit.Pixel );
			StringFormat center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
			graphics.DrawString( txt, coinFont, Brushes.Black, bounds, center );

			bounds.X += bounds.Width*.3f;
			bounds.Y += bounds.Height*.3f;
		}

		void PaintCurrentEnergy() {
			RectangleF bigCoinBounds = metrics.BigCoin;
			float slotWidth = bigCoinBounds.Width/2;
			float bigCoinWidth = slotWidth * 1.7f;
			var energyRect = new RectangleF(
				bigCoinBounds.Right - bigCoinWidth,
				bigCoinBounds.Y,
				bigCoinWidth,
				bigCoinWidth
			);
			graphics.DrawImage( coin, energyRect );
			DrawTextOnCoin( ref energyRect, energy.ToString() );
		}

		void DrawElement( ref RectangleF innerBounds, params Element[] elements ) {
			foreach(var element in elements) {
				using Image image = ResourceImages.Singleton.GetToken( element );
				graphics.DrawImageFitHeight( image, innerBounds );
				innerBounds.X += innerBounds.Width * 0.3f;
				innerBounds.Y += innerBounds.Height * 0.3f;
			}
		}

		void DrawAction( RectangleF bounds, Track track ) {
			// draw icon
			string iconName = track.Action.Name switch {
				"Reclaim(1)" => "reclaim 1",
				"Push1DahanFromLands" => "Push1dahan",
				"DrawMinorOnceAndPlayExtraCardThisTurn" => "Cardplayplusone",
				"DiscardElementsForCardPlay" => "DiscardElementsForCardPlay",
				_ => track.Action.Name
			};
			var bitmap = ResourceImages.Singleton.GetIcon( iconName );
			graphics.DrawImageFitHeight( bitmap, bounds );
		}


		void PaintDestroyed(Bitmap presenceImg) {
			Rectangle rect = metrics.Destroyed;
			int destroyedCount = destroyed;
			if(destroyedCount == 0) return;

			// Presence & Red X
			graphics.DrawImage( presenceImg, rect );
			using var redX = ResourceImages.Singleton.GetIcon( "red-x" );
			graphics.DrawImage( redX, rect.X, rect.Y, rect.Width * 2 / 3, rect.Height * 2 / 3 );

			// count
			graphics.DrawCountIfHigherThan( rect, destroyedCount );

		}

	}

	public struct SlotMetrics {
		public Rectangle PresenceRect;
		public RectangleF TrackRect;
	}

}
