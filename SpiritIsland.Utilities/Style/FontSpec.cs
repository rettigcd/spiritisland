namespace SpiritIsland;

public abstract class FontSpec {

	static public implicit operator FontSpec(Font font) => new BorrowedFont(font);
	static public implicit operator FontSpec(float scaler) => new GameFont(scaler);
	static public implicit operator FontSpec(string format) => new CustomFont(format);

 	public abstract ResourceMgr<Font> GetResourceMgr(Rectangle bounds);

	class GameFont(float scale) : FontSpec {
 		public override ResourceMgr<Font> GetResourceMgr(Rectangle bounds) => new ResourceMgr<Font>( ResourceImages.Singleton.UseGameFont(bounds.Height*scale), true );
	}
	class CustomFont : FontSpec {

		public CustomFont(string format){
			string[] parts = format.Split(';');
			_fontFamily = parts[0];
			_scale = (parts.Length < 2) ? 1f : float.Parse(parts[1]);
			_fontStyle = (parts.Length < 3) ? FontStyle.Regular : ParseStyle( parts[2] );
		}
		static FontStyle ParseStyle(string styleStr){
			FontStyle style = default;
			var parts = styleStr.Split('|');
			foreach(var part in parts){
					style |= part switch {
					"bold" => FontStyle.Bold,
					"underline" => FontStyle.Underline,
					"italic" => FontStyle.Italic,
					"strikeout" => FontStyle.Strikeout,
					_ => FontStyle.Regular
				};
			}
			return style;
		}
 		public override ResourceMgr<Font> GetResourceMgr(Rectangle bounds){
			var font = new Font( _fontFamily, bounds.Height*_scale, _fontStyle, GraphicsUnit.Pixel );
			return new ResourceMgr<Font>( font, true );
		}
		readonly string _fontFamily;
		readonly float _scale;
		readonly FontStyle _fontStyle;
	}
	
	class BorrowedFont(Font font) : FontSpec {
 	 	public override ResourceMgr<Font> GetResourceMgr(Rectangle bounds) => new ResourceMgr<Font>( font, false);
 	}

}