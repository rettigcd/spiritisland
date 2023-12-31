using System.Drawing;

namespace SpiritIsland;

/// <summary>
/// Generates Blight Card Images
/// </summary>
static class BlightCardBuilder {

	static public Bitmap BuildBlighted( IBlightCard card ) {
		Bitmap bitmap = new Bitmap( 400, 640 );
		Graphics graphics = Graphics.FromImage( bitmap );
		graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
		using StringFormat alignCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

		var bounds = new Rectangle( 0, 0, bitmap.Width, bitmap.Height );
		graphics.FillRoundedRectangle( Brushes.Brown, bounds, 25 );

		var (blightedRect, (titleRect, (descRect, _))) = bounds.InflateBy( -10 ).SplitVerticallyByWeight( 10, .15f, .25f, .5f );

		// Draw Blighted ISLAND
		if(card is StillHealthyBlightCard) {
			graphics.FillRoundedRectangle( Brushes.GreenYellow, blightedRect, 15 );
			using Font headerFont = ResourceImages.Singleton.UseGameFont( blightedRect.Height * .65f );
			graphics.DrawString( "STILL HEALTHY", headerFont, Brushes.Black, blightedRect, alignCenter );
		} else {
			graphics.FillRoundedRectangle( Brushes.Wheat, blightedRect, 15 );
			using Font headerFont = ResourceImages.Singleton.UseGameFont( blightedRect.Height * .65f );
			graphics.DrawString( "BLIGHTED ISLAND", headerFont, Brushes.Black, blightedRect, alignCenter );
		}
		// Draw Label
		graphics.FillRoundedRectangle( Brushes.Wheat, titleRect, 15 );
		using(Font titleFont = ResourceImages.Singleton.UseGameFont( titleRect.Height * .3f ))
			graphics.DrawString( card.Name, titleFont, Brushes.Black, titleRect, alignCenter );
		// Draw Instructions
		graphics.FillRoundedRectangle( Brushes.Wheat, descRect, 15 );
		using(Font titleFont = ResourceImages.Singleton.UseGameFont( descRect.Height * .1f ))
			graphics.DrawString( card.Description, titleFont, Brushes.Black, descRect, alignCenter );
		return bitmap;
	}

	static public Bitmap BuildHealthy() {
		Bitmap bitmap = new Bitmap( 400, 640 );
		Graphics graphics = Graphics.FromImage( bitmap );
		graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
		using StringFormat alignCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

		var bounds = new Rectangle( 0, 0, bitmap.Width, bitmap.Height );
		graphics.FillRoundedRectangle( Brushes.Brown, bounds, 25 );

		var (healthyRect, (descRect, _)) = bounds.InflateBy( -10 ).SplitVerticallyByWeight( 10, .15f, .75f );

		// Draw Healthy ISLAND
		graphics.FillRoundedRectangle( Brushes.GreenYellow, healthyRect, 15 );
		using(Font headerFont = ResourceImages.Singleton.UseGameFont( healthyRect.Height * .65f ))
			graphics.DrawString( "HEALTHY ISLAND", headerFont, Brushes.DarkGreen, healthyRect, alignCenter );
		// Draw Instructions
		const string frontInstructions = "2 blight per player.  Any blight removed from the board returns here.  If there is ever NO blight here, flip this card.";
		graphics.FillRoundedRectangle( Brushes.Wheat, descRect, 15 );
		using Font titleFont = ResourceImages.Singleton.UseGameFont( descRect.Height * .1f );
		graphics.DrawString( frontInstructions, titleFont, Brushes.Black, descRect, alignCenter );
		return bitmap;
	}

}