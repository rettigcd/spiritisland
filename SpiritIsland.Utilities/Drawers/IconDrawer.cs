using System.Drawing;
using System.Drawing.Text;

namespace SpiritIsland.WinForms; 


public interface IconResources {
	Bitmap GetImg(Img img);
	Font UseGameFont( float fontHeight );
}

/// <summary>
/// Draws IconDescriptors on a Given Graphics object.
/// </summary>
public class IconDrawer {

	/// <summary>
	/// Generates the bitmap of the IconDesc.
	/// </summary>
	static public Bitmap BuildTrackSlot( IconDescriptor icon, IconResources resources ) {
		const int dimension = 200;
		Bitmap bitmap = new Bitmap( dimension, dimension );
		RectangleF bounds = new RectangleF( 0, 0, dimension, dimension );
		using Graphics graphics = Graphics.FromImage( bitmap );
		graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
		IconDrawer.DrawTheIcon( graphics, icon, bounds, resources );
		return bitmap;
	}


	static public void DrawTheIcon( Graphics graphics, IconDescriptor iconDescriptor, RectangleF bounds, IconResources resources ) {

		if(iconDescriptor == null) {
			graphics.FillRectangle( Brushes.Black, bounds );
			return;
		}

		void DrawImageFitBoth( Img img, RectangleF bounds ) {
			using var image = resources.GetImg( img );
			graphics.DrawImage( image, bounds.ToInts().FitBoth( image.Size ) );
		}

		// if we have a sub-image, reduce the main bounds to accommodate it
		const float subImageReduction = .8f;
		RectangleF mainBounds = iconDescriptor.Sub == null ? bounds
			: new RectangleF( bounds.X, bounds.Y, bounds.Width * subImageReduction, bounds.Height * subImageReduction );

		// -- Main - Background
		if(iconDescriptor.BackgroundImg != default)
			DrawImageFitBoth( iconDescriptor.BackgroundImg, mainBounds );

		// -- Main - Content --
		if(iconDescriptor.Text != null || iconDescriptor.ContentImg != default) {

			var contentBounds = new RectangleF(
				mainBounds.X,
				mainBounds.Y + mainBounds.Height * 0.22f,  // should be 20% but it looks a little high
				mainBounds.Width,
				mainBounds.Height * .6f // 60%
			);

			// Content - Images
			if(iconDescriptor.ContentImg != default) {
				if(iconDescriptor.ContentImg2 != default) {
					const float scale = .75f;
					float w = contentBounds.Width * scale, h = contentBounds.Height * scale;
					DrawImageFitBoth( iconDescriptor.ContentImg, new RectangleF( contentBounds.X, contentBounds.Y, w, h ) );
					DrawImageFitBoth( iconDescriptor.ContentImg2, new RectangleF( contentBounds.X + contentBounds.Width * (1f - scale), contentBounds.Y + contentBounds.Height * (1f - scale), w, h ) );
				} else
					DrawImageFitBoth( iconDescriptor.ContentImg, contentBounds );
			}

			// Content - Text
			if(iconDescriptor.Text != null) {
				Font font = resources.UseGameFont( contentBounds.Height );
				using StringFormat center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
				graphics.DrawString( iconDescriptor.Text, font, Brushes.Black,
					contentBounds.InflateBy( contentBounds.Width, 0f ), // font based on height and centered, don't clip left/right
					center
				);
			}

		}

		// -- Super --
		if(iconDescriptor.Super != null) {
			float dim = bounds.Width * .4f;
			// drawing this at the bottom instead of the top because presence is covering top
			var superRect = new RectangleF( bounds.X, bounds.Bottom - dim, dim, dim );
			DrawTheIcon( graphics, iconDescriptor.Super, superRect, resources );
		}

		// -- Sub - (additional action) --
		if(iconDescriptor.Sub != null) {
			// put the subRect in the bottom right corner
			float subDim = bounds.Width * .5f;
			var subRect = new RectangleF( bounds.Right - subDim, bounds.Bottom - subDim, subDim, subDim );
			DrawTheIcon( graphics, iconDescriptor.Sub, subRect, resources );
		}
		// -- Big Sub --
		if(iconDescriptor.BigSub != null) {
			// put the subRect in the bottom right corner
			float subDim = bounds.Width;
			var subRect = new RectangleF( bounds.Right - subDim / 2, bounds.Bottom - subDim * 3 / 4, subDim, subDim );
			DrawTheIcon( graphics, iconDescriptor.BigSub, subRect, resources );
		}
	}

}
