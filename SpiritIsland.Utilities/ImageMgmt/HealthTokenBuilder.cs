using System;
using System.Drawing;

namespace SpiritIsland.WinForms;

class HealthTokenBuilder {
	public static Bitmap GetHealthTokenImage( HumanToken ht ) {

		Bitmap orig = ResourceImages.Singleton.GetImage( ht.Class.Img );

		Func<Color, Color> colorConverter = ht.Class.Variant switch {
			// p => Color.FromArgb( p.A, p.R / 2, p.G / 2, p.B / 2 ); // halfScale
			// p => Color.FromArgb( p.A, 255 - p.R, 255 - p.G, 255 - p.B ); // invert
			// p => Color.FromArgb( p.A, p.R / 2, p.G / 2, p.B * 2 / 3 ); // red green
			TokenVariant.Dreaming => new HslColorAdjuster( new HSL( 240, 75, 40 ) ).GetNewColor,
			TokenVariant.Frozen => new HslColorAdjuster( new HSL( 0, 45, 40 ) ).GetNewColor,
			_ => ( x ) => x
		};
		new PixelAdjustment( colorConverter ).Adjust( orig );

		using var g = Graphics.FromImage( orig );

		// If Full Health is different than standard, show it
		if(ht.FullHealth != ht.Class.ExpectedHealthHint) {
			using var font = UseGameFont( orig.Height / 2 );
			StringFormat center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
			g.DrawString( ht.FullHealth.ToString(), font, Brushes.White, new RectangleF( 0, 0, orig.Width, orig.Height ), center );
		}

		// Draw Damage slashes
		if(0 < ht.FullDamage) {
			int dX = orig.Width / ht.FullHealth;
			int lx = dX / 2;
			// Normal Damage
			if(0 < ht.Damage)
				using(Pen redSlash = new Pen( Color.FromArgb( 128, Color.Red ), 30f )) {
					for(int i = 0; i < ht.Damage; ++i) {
						g.DrawLine( redSlash, lx, orig.Height, lx + dX, 0 );
						lx += dX;
					}
				}
			// Dream Damage
			if(0 < ht.DreamDamage)
				using(Pen dreamSlash = new Pen( Color.FromArgb( 128, Color.MidnightBlue ), 30f )) { // or maybe steal blue
					for(int i = 0; i < ht.DreamDamage; ++i) {
						g.DrawLine( dreamSlash, lx, orig.Height, lx + dX, 0 );
						lx += dX;
					}
				}
		}

		return orig;
	}
	static Font UseGameFont( float fontHeight ) => ResourceImages.Singleton.UseGameFont( fontHeight );
}