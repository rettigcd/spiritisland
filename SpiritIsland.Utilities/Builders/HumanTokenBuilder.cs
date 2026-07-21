namespace SpiritIsland;

static class HumanTokenBuilder {

	public static Bitmap Build( HumanToken ht ) {

		Bitmap orig = ResourceImages.Singleton.GetImg( ht.HumanClass.Img );

		using var g = Graphics.FromImage( orig );

		// If Full Health is different than standard, show it
		if(ht.FullHealth != ht.HumanClass.ExpectedHealthHint) {
			using var font = UseGameFont( orig.Height / 2 );
			StringFormat center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
			var brush = ht.HasTag(Human.Dahan) ? Brushes.White : Brushes.DarkGray;
			g.DrawString( ht.FullHealth.ToString(), font, brush, new RectangleF( 0, 0, orig.Width, orig.Height ), center );
		}

		// Draw Damage slashes
		if(0 < ht.Damage) {
			int dX = orig.Width / ht.FullHealth;
			int lx = dX / 2;
			using(Pen redSlash = new Pen( Color.FromArgb( 128, Color.Red ), 30f )) {
				for(int i = 0; i < ht.Damage; ++i) {
					g.DrawLine( redSlash, lx, orig.Height, lx + dX, 0 );
					lx += dX;
				}
			}
		}

		return orig;
	}

	#region private methods

	static Font UseGameFont( float fontHeight ) => ResourceImages.Singleton.UseGameFont( fontHeight );

	#endregion
}