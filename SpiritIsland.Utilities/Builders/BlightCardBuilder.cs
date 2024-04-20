namespace SpiritIsland;

/// <summary>
/// Generates Blight Card Images
/// </summary>
static class BlightCardBuilder {

	static public Bitmap BuildBlighted( BlightCard card ) {
		Bitmap bitmap = new( 7*64, 7*89 ); // std dim are 64mm x 89
		Graphics graphics = Graphics.FromImage( bitmap );
		graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
		using StringFormat alignCenter = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

		var bounds = new Rectangle( 0, 0, bitmap.Width, bitmap.Height );
		using(var cracks = ResourceImages.Singleton.CardTexture("cracks.jpg"))
			using(var brush = new TextureBrush(cracks))
				graphics.FillRoundedRectangle( brush, bounds, 25 );

		var inner = bounds.InflateBy( -15 );
		var (titleRect, (bottomRect, _)) = inner.SplitVerticallyByWeight( 0, .18f, .82f );

		// BG blight-photo
		using(var blightedBg = ResourceImages.Singleton.CardTexture("blighted_bg.jpg"))
			using(var brush = new TextureBrush(blightedBg))
				graphics.FillPath( brush, bottomRect.RoundCorners(15,false,false,true,true) );


		// Title
		if(card is StillHealthyBlightCard)
			HealthyHeader( graphics, titleRect, "STILL HEALTHY" );
		else
			BlightedHeader( graphics, titleRect );


		// Card Title & Description
		const int sideMargin = 20;
		var config = new ConfigWrappingLayout { ElementDimension = 20, EmSize = 20, IconDimension = 20, HorizontalAlignment = Align.Center };
		var layout = new WrappingLayout( config, new Size( bottomRect.Width - sideMargin * 2, 25 ), graphics );
		layout.AppendLine( card.Name, FontStyle.Bold );
		layout.AppendLine( card.Description, FontStyle.Regular );
		layout.FinalizeBounds();
		var textRect = new Rectangle(bottomRect.Left, bottomRect.Top, bottomRect.Width, layout.Size.Height+sideMargin);
		using(var brush = new SolidBrush(ColorString.Parse("c1b6b3")))
			graphics.FillRectangle(brush, textRect);
		layout.CenterDrawingSpace(textRect.Size);
		layout.Adjust(sideMargin,bottomRect.Y);
		layout.Paint(graphics);

		var bottomConfig = new ConfigWrappingLayout { ElementDimension = 24, EmSize = 24, IconDimension = 24, HorizontalAlignment = Align.Center };
		var l2 = new WrappingLayout( bottomConfig, new Size( bottomRect.Width - sideMargin * 2, 28 ), graphics );
		l2.AppendLine( $"{card.Side2BlightPerPlayer} Blight per player.", FontStyle.Bold);
		l2.AppendLine( "", FontStyle.Regular );
		l2.AppendLine( "Any Blight removed from", FontStyle.Regular );
		l2.AppendLine( "the board returns here.", FontStyle.Regular );
		l2.AppendLine( "", FontStyle.Regular );

		l2.AppendLine( "If there is ever NO Blight here,", FontStyle.Regular );
		if(card is StillHealthyBlightCard){
			l2.AppendLine( "draw a new Blight Card.", FontStyle.Regular );
			l2.AppendLine( "It comes into play already flipped.", FontStyle.Bold );
		}else
			l2.AppendLine( "players lose.", FontStyle.Regular );

		l2.FinalizeBounds();
		var remainingRect = new Rectangle(textRect.Left,textRect.Bottom,textRect.Width,bottomRect.Height-textRect.Height);
		l2.CenterDrawingSpace(remainingRect.Size);
		l2.Adjust(sideMargin,remainingRect.Y);
		l2.Paint(graphics);

		// black border
		using(var pen = new Pen( Color.Black, 2 ))
			graphics.DrawPath( pen, inner.RoundCorners( 15 ) );

		return bitmap;

	}

	static void BlightedHeader( Graphics graphics, Rectangle titleRect ) {
		using StringFormat alignCenter = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
		using(var parchment = ResourceImages.Singleton.CardTexture( "parchment.jpg" ))
		using(var brush = new TextureBrush( parchment ))
			graphics.FillPath( brush, titleRect.RoundCorners( 15, true, true, false, false ) );
		using Font headerFont = ResourceImages.Singleton.UseGameFont( titleRect.Height * .65f );
		graphics.DrawString( "BLIGHTED ISLAND", headerFont, Brushes.Black, titleRect, alignCenter );
	}


	/// <summary>
	/// Builds the starting-side of the Blight Card.
	/// </summary>
	static public Bitmap BuildHealthy() {
		Bitmap bitmap = new( 7 * 64, 7 * 89 ); // std dim are 64mm x 89
		Graphics graphics = Graphics.FromImage( bitmap );
		graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
		using StringFormat alignCenter = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

		var bounds = new Rectangle( 0, 0, bitmap.Width, bitmap.Height );
		using(var cracks = ResourceImages.Singleton.CardTexture( "cracks.jpg" ))
		using(var brush = new TextureBrush( cracks ))
			graphics.FillRoundedRectangle( brush, bounds, 25 );

		var inner = bounds.InflateBy( -15 );
		var (healthyRect, (descRect, _)) = inner.SplitVerticallyByWeight( 0, .15f, .75f );

		// Healthy ISLAND - Title
		HealthyHeader( graphics, healthyRect, "HEALTHY ISLAND" );

		// Draw Instructions
		// -- Background --
		using(var img = ResourceImages.Singleton.CardTexture( "healthy_bg.jpg" ))
		using(var hbgBrush = new TextureBrush( img ))
			graphics.FillPath( hbgBrush, descRect.RoundCorners( 15, false, false, false, false ) );

		// -- Text --
		const int sideMargin = 25;
		var config = new ConfigWrappingLayout { ElementDimension = 30, EmSize = 30, IconDimension = 30, HorizontalAlignment = Align.Center };
		var layout = new WrappingLayout( config, new Size( 400 - sideMargin * 2, 30 ), graphics );
		layout.AppendLine( "2 Blight per player", FontStyle.Bold );
		layout.AppendLine( "", FontStyle.Bold );
		layout.AppendLine( "Any Blight removed from the board returns here.", FontStyle.Regular );
		layout.AppendLine( "", FontStyle.Bold );
		layout.AppendLine( "If there is ever NO Blight here, flip this card.", FontStyle.Regular );
		layout.FinalizeBounds();
		layout.CenterDrawingSpace( descRect.Size );
		layout.Adjust( sideMargin, descRect.Top );
		layout.Paint( graphics );

		// black border
		using(var pen = new Pen( Color.Black, 2 ))
			graphics.DrawPath( pen, inner.RoundCorners( 15 ) );

		return bitmap;
	}

	static void HealthyHeader( Graphics graphics, Rectangle healthyRect, string title ) {
		using StringFormat alignCenter2 = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
		using(var brush = new SolidBrush( ColorString.Parse( "c9d889" ) ))
			graphics.FillPath( brush, healthyRect.RoundCorners( 15, true, true, false, false ) );
		using Font headerFont = ResourceImages.Singleton.UseGameFont( healthyRect.Height * .62f );
		graphics.DrawString( title, headerFont, Brushes.DarkGreen, healthyRect, alignCenter2 );
	}
}