using System.Drawing;

namespace SpiritIsland.WinForms {

	class IconDrawer {

		readonly Graphics _graphics;
		readonly CachedImageDrawer _cachedImageDrawer;

		public IconDrawer( Graphics graphics, CachedImageDrawer imageDrawer ) {
			_graphics = graphics;
			_cachedImageDrawer = imageDrawer;
		}

		public void DrawTheIcon( IconDescriptor icon, RectangleF bounds ) {

			if(icon == null) {
				_graphics.FillRectangle( Brushes.Black, bounds );
				return;
			}

			// if we have a sub-image, reduce the main bounds to accommodate it
			const float subImageReduction = .8f;
			RectangleF mainBounds = (icon.Sub == null) ? bounds
				: new RectangleF(bounds.X,bounds.Y,bounds.Width* subImageReduction, bounds.Height* subImageReduction );

			// -- Main - Background
			if(icon.BackgroundImg != default) {
				_graphics.DrawImageFitBoth( _cachedImageDrawer.GetImage(icon.BackgroundImg), mainBounds );
			}

			// -- Main - Content --
			if(icon.Text != null || icon.ContentImg != default) {

				var contentBounds = new RectangleF(
					mainBounds.X, 
					mainBounds.Y + mainBounds.Height * 0.22f,  // should be 20% but it looks a little high
					mainBounds.Width, 
					mainBounds.Height*.6f // 60%
				);
				// Content - Text
				if(icon.Text != null) {
					Font font = ResourceImages.Singleton.UseGameFont( contentBounds.Height );
					StringFormat center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
					_graphics.DrawString( icon.Text, font, Brushes.Black, contentBounds, center );
				}

				// Content - Images
				if(icon.ContentImg != default) {
					if(icon.ContentImg2 != default) {
						const float scale = .75f;
						float w = contentBounds.Width * scale, h = contentBounds.Height * scale;
						var cb1 = new RectangleF(contentBounds.X,contentBounds.Y,w,h);
						var cb2 = new RectangleF( contentBounds.X+ contentBounds.Width*(1f - scale), contentBounds.Y+contentBounds.Height* (1f - scale), w, h );
						_graphics.DrawImageFitBoth( _cachedImageDrawer.GetImage( icon.ContentImg ), cb1 );
						_graphics.DrawImageFitBoth( _cachedImageDrawer.GetImage( icon.ContentImg2 ), cb2 );
					} else
						_graphics.DrawImageFitBoth( _cachedImageDrawer.GetImage( icon.ContentImg ), contentBounds );
				}

			}

			// -- Super --
			if(icon.Super != null) {
				float dim = bounds.Width * .4f;
				// drawing this at the bottom instead of the top because presence is covering top
				var superRect = new RectangleF(bounds.X,bounds.Bottom-dim, dim, dim );
				DrawTheIcon( icon.Super, superRect );
			}

			// -- Sub - (additional action) --
			if(icon.Sub != null) {
				// put the subRect in the bottom right corner
				float subDim = bounds.Width * .5f;
				var subRect = new RectangleF( bounds.Right - subDim, bounds.Bottom - subDim, subDim, subDim );
				DrawTheIcon( icon.Sub, subRect );
			}
			// -- Big Sub --
			if(icon.BigSub != null) {
				// put the subRect in the bottom right corner
				float subDim = bounds.Width;
				var subRect = new RectangleF( bounds.Right - subDim/2, bounds.Bottom - subDim*3/4, subDim, subDim );
				DrawTheIcon( icon.BigSub, subRect );
			}
		}

	}


}
