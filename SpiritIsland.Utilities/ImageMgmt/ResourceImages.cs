using SpiritIsland.Tests.Core;
using SpiritIsland.Utilities.ImageMgmt;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SpiritIsland.WinForms;

public class ResourceImages {

	static public readonly ResourceImages Singleton = new ResourceImages();

	readonly Assembly _assembly;

	ResourceImages() {
		_assembly = typeof(ResourceImages).Assembly; // System.Reflection.Assembly.GetExecutingAssembly();
		Fonts = new PrivateFontCollection();
		LoadFont( "leaguegothic-regular-webfont.ttf" );
		LoadFont( "playsir-regular.otf" );
	}

	#region Fonts

	readonly PrivateFontCollection Fonts;

	public Font UseGameFont( float fontHeight ) => new Font( Fonts.Families[0], Math.Max(4,fontHeight), GraphicsUnit.Pixel );

	public Font UseInvaderFont( float fontHeight ) => new Font( Fonts.Families[1], fontHeight, GraphicsUnit.Pixel );

	void LoadFont(string file) {
		string resource = "SpiritIsland.Utilities." + file;
		using Stream? fontStream = _assembly.GetManifestResourceStream( resource ) 
			?? throw new ArgumentException($"No font stream found for {file}");

		// read the fond data into a buffer
		byte[] fontdata = new byte[fontStream.Length];
		fontStream.Read( fontdata, 0, (int)fontStream.Length );

		// copy the bytes to the unsafe memory block
		IntPtr data = Marshal.AllocCoTaskMem( (int)fontStream.Length );
		Marshal.Copy( fontdata, 0, data, (int)fontStream.Length );

		// pass the font to the font collection
		Fonts.AddMemoryFont( data, (int)fontStream.Length );
		// free up the unsafe memory
		Marshal.FreeCoTaskMem( data );
	}

	#endregion

#pragma warning disable CA1822 // Mark members as static
	public Image LoadSpiritImage( string spiritText ) {
		string filename = spiritText.Replace( ' ', '_' );
		return Image.FromFile( $".\\images\\spirits\\{filename}.jpg" );
	}
#pragma warning restore CA1822 // Mark members as static

	public Bitmap GetPresenceImage( string img )    => GetResourceImage( $"presence.{img}.png" );
	public Bitmap GetAdversaryFlag( string adv )    => GetResourceImage($"adversaries.{adv}.png" );
	public Bitmap GetImage( Element el )            => GetImage(el.GetTokenImg() );
	public Bitmap GetImage( Img img )               => GetResourceImage( ToResource( img ) );
	public Bitmap Strife()                          => GetResourceImage("tokens.strife.png");
	public Bitmap Fear()                            => GetResourceImage("tokens.fear.png");
	public Bitmap FearCardBack()                    => GetResourceImage("tokens.fearcard.png");
	public Bitmap RedX()                            => GetResourceImage("icons.red-x.png");
	public Bitmap Hourglass()                       => GetResourceImage("icons.hourglass.png");
	public Bitmap TerrorLevel( int terrorLevel )    => GetResourceImage($"icons.TerrorLevel{terrorLevel}.png" );
	public Bitmap GetInvaderCard(string text)       => GetResourceImage($"invaders.{text}");

	public Bitmap Texture( string texture ) => GetResourceImage( $"textures.{texture}" );

	public Image GetInvaderCard( InvaderCard card ) {
		if( !card.Flipped )
			return GetInvaderCardBack( card.InvaderStage );

		string key = $"invaders\\{card.Text}.png";
		if(_cache.Contains( key )) return _cache.Get( key );

		Bitmap image = InvaderCardBuilder.BuildInvaderCard( card );
		_cache.Add( key, image );
		return image;
	}

	Image GetInvaderCardBack( int stage ) {
		string key = $"invaders\\back_{stage}.png";
		if(_cache.Contains( key )) return _cache.Get( key );

		Bitmap image = InvaderCardBuilder.BuildInvaderCardBack( stage );
		_cache.Add( key, image );
		return image;
	}

	readonly static bool saveSpace = false; // for fear card images
	readonly static bool clipCorners = true;
	static string FearKey( IFearCard fearCard ) => $"fear\\{fearCard.Text}." + (saveSpace ? "jpg" : "png");
	public Image GetFearCard( IFearCard card ) {
		InitFearCard( card );
		Image img = _cache.Get( FearKey(card) );
		if(saveSpace && clipCorners) {
			Bitmap noCornerBitmap = new Bitmap( img.Width, img.Height );
			using Graphics graphics = Graphics.FromImage( noCornerBitmap );
			Brush brush = new TextureBrush( img );
			graphics.FillPath( brush, new Rectangle(new Point(0,0),img.Size).RoundCorners(18));
			brush.Dispose();
			img.Dispose();
			img = noCornerBitmap;
		}
		return img;
	}

	public void InitFearCard( IFearCard card ) {
		string key = FearKey( card );
		if(_cache.Contains( key )) return;
		using Bitmap img = (Bitmap)FearCardImageBuilder.Build( card );
		_cache.Add( key, img );
	}

	public async Task<Image> GetPowerCard( PowerCard card ) {
		try {
			ImageDiskCache _cache = new ImageDiskCache();
			string key = $"PowerCard\\{card.Name}.png";
			if(_cache.Contains( key )) return _cache.Get( key );

			Bitmap image = (Bitmap)await PowerCardImageBuilder.Build( card ); // don't dispose, we are returning it
			_cache.Add( key, image );
			return image;
		} catch(Exception ex) {
			_ = ex.ToString();
			throw;
		}
	}

	public Image GetBlightCard( IBlightCard card ) {
		string key = "blight\\" + card.Name + ".png";
		if(_cache.Contains( key )) return _cache.Get( key );

		Bitmap bitmap = BlightCardBuilder.BuildBlighted( card );

		// save
		_cache.Add( key, bitmap );
		return bitmap;
	}

	public async Task<Image> GetCardImage( PowerCard card ) {
		string key = $"power_card_pic\\{card.Name}.png";
		if(_cache.Contains( key )) return _cache.Get( key );
		Bitmap bitmap = await CardDownloader.GetImage( card.Name );
		_cache.Add( key, bitmap );
		return bitmap;
	}


	public Image GetHealthBlightCard() {
		string key = "blight\\healthy.png";
		if(_cache.Contains( key )) return _cache.Get( key );

		Bitmap bitmap = BlightCardBuilder.BuildHealthy();

		// save
		_cache.Add( key, bitmap );
		return bitmap;
	}

	public Image GetGhostImage( Img img ) {

		string key = $"ghosts\\{img}.png";
		if(_cache.Contains( key )) return _cache.Get( key );

		Bitmap image = GetImage( img );
		new PixelAdjustment( MakePartiallyTransparent ).Adjust( image );

		_cache.Add( key, image );
		return image;
	}

	public Image FearGray() { 
		string key = "fear_gray.png";
		if(_cache.Contains( key )) return _cache.Get( key );
		Bitmap image = Fear();
		new PixelAdjustment( LowerContrast ).Adjust( image );
		_cache.Add( key, image );
		return image;
	}
	static Color LowerContrast( Color x ) => Color.FromArgb( x.A, x.R / 4 + 96, x.G / 4 + 96, x.B / 4 + 96 );
	static Color MakePartiallyTransparent( Color x ) => Color.FromArgb( Math.Min( (byte)92, x.A ), x );

	public Brush UseSpaceBrush( Space space ) {
		Terrain terrain 
			= space.IsWetland ? Terrain.Wetland
			: space.IsJungle ? Terrain.Jungle
			: space.IsMountain ? Terrain.Mountain
			: space.IsSand ? Terrain.Sands
			: (space.IsOcean || space.IsDestroyed) ? Terrain.Ocean
			: Terrain.None; // throw new ArgumentException( $"No terrain found for {space.Text}", nameof( space ) );
		return UseTerrainBrush( terrain );
	}

	public Brush UseTerrainBrush( Terrain terrain ) {
		using Image image = UseTerrainImage( terrain );
		return new TextureBrush( image );
	}

	public Image UseTerrainImage( Terrain terrain ) {
		string terrainName = terrain switch {
			Terrain.Wetland => "wetlands",
			Terrain.Jungle => "jungle",
			Terrain.Mountain => "mountains",
			Terrain.Sands => "sand",
			Terrain.Ocean => "ocean",
			Terrain.None => "none",
			_ => throw new ArgumentException($"{terrain} not mapped"),
		};

		HSL? terrainColor = terrain switch {
			Terrain.Wetland => new HSL( 184, 40, 45 ),
			Terrain.Jungle => new HSL( 144, 60, 40 ),
			Terrain.Mountain => new HSL( 45, 10, 33 ),
			Terrain.Sands => new HSL( 38, 50, 40 ),
			_ => null
		};

		// No HSL, use original
		static string SrcPath( string terrainName ) => $".\\images\\{terrainName}.jpg";
		if( terrainColor == null ) return Image.FromFile( SrcPath(terrainName) );

		// Check Cache
		string key = $"{terrainName} {terrainColor}.jpg";
		if(_cache.Contains(key)) return _cache.Get(key);

		// Build it, & cache it
		Bitmap image = (Bitmap)Image.FromFile( SrcPath( terrainName ) );

		new PixelAdjustment( new HslColorAdjuster( terrainColor ).GetNewColor ).Adjust( (Bitmap)image );
		_cache.Add(key, image);
		return image;
	}

	public Image GetGeneralInstructions( string description, float textEmSize, Size rowSize ) {
		if(50<description.Length) description = description[..50];
		string key = "innateOptions\\gi_" + description.Replace(' ','_') +".png";
		if(_cache.Contains(key)){
			var image = _cache.Get(key);
			if( rowSize.Width < image.Width) return image;
			image.Dispose(); // do small - regenerate
		}
		using var tempBitmap = new Bitmap(rowSize.Width, rowSize.Width); // Height is wrong
		using Graphics graphics = Graphics.FromImage(tempBitmap);
		graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

		var config = new ConfigWrappingLayout {
			EmSize = textEmSize,
			ElementDimension = (int)(textEmSize * 2.4f),
			IconDimension = (int)(textEmSize * 1.8f),
			HorizontalAlignment = Align.Near
		};

		var layout = new WrappingLayout( config, rowSize, graphics );
		layout.Append( description, FontStyle.Regular );
		layout.FinalizeBounds();
		layout.Paint(graphics);

		Bitmap bitmap = tempBitmap.Clone( new Rectangle( new Point(0,0), layout.Size ), tempBitmap.PixelFormat );

		_cache.Add( key, bitmap );
		return bitmap;
	}


	public Image GetInnateOption( IDrawableInnateTier innateOption, float emSize, Size rowSize ) {
		string key = "innateOptions\\" + innateOption.Text.Replace( ' ', '_' ).Replace( '/', '_' ).Replace( '.', '_' ) + ".png";
		if(_cache.Contains( key )) {
			var image = _cache.Get( key );
			if(rowSize.Width < image.Width) return image;
			image.Dispose(); // do small - regenerate
		}

		using Bitmap tempBitmap = new Bitmap( rowSize.Width, rowSize.Width*2 ); // Height is wrong
		using Graphics graphics = Graphics.FromImage( tempBitmap );
		graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

		var config = new ConfigWrappingLayout {
			EmSize = emSize,
			ElementDimension = (int)(emSize * 2.4f),
			IconDimension = (int)(emSize * 1.8f),
			HorizontalAlignment = Align.Near,
			Indent = rowSize.Height / 2,
		};
		// Elements Thresholds
		var layout = new WrappingLayout( config, rowSize, graphics );

		// top overhand margin
		layout.AddIconRowOverflowHeight(); // create top buffer for over hanging enlarged tokens

		// Elements in bold
		layout.Append( innateOption.ThresholdString, FontStyle.Bold );

		// Tab
		layout.Tab( 2, FontStyle.Bold );

		// Text
		layout.Append( innateOption.Description, FontStyle.Regular );

		layout.AddIconRowOverflowHeight(); // create bottom buffer for over hanging enlarged tokens

		layout.FinalizeBounds();

		layout.Paint(graphics);

		Bitmap bitmap = tempBitmap.Clone( new Rectangle( new Point(0,0), layout.Size ), tempBitmap.PixelFormat );

		_cache.Add( key, bitmap );
		return bitmap;
	}

	public Image GetTrack( IconDescriptor icon ) {
		string key = "track\\" + GetKey( icon ) + ".png";
		if(_cache.Contains(key)) return _cache.Get(key);

		const int dimension = 200;
		Bitmap bitmap = new Bitmap( dimension, dimension );
		RectangleF bounds = new RectangleF(0,0,dimension,dimension );
		using Graphics graphics = Graphics.FromImage( bitmap );
		graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

		using var imgCache = new ImgMemoryCache();
		new IconDrawer( graphics, imgCache ).DrawTheIcon( icon, bounds );

		_cache.Add( key, bitmap );
		return bitmap;
	}

	static string GetKey( IconDescriptor icon ) {
		var items = new List<string>();
		if(!string.IsNullOrEmpty(icon.Text)) items.Add( icon.Text );
		if(icon.BackgroundImg != default) items.Add( icon.BackgroundImg.ToString() );

		if(icon.ContentImg != default) items.Add( "1-" + icon.ContentImg.ToString() );
		if(icon.ContentImg2 != default) items.Add( "2-" + icon.ContentImg2.ToString() );
		if(icon.Super != default) items.Add( "Sup(" + GetKey( icon.Super ) + ")" );
		if(icon.Sub != default) items.Add( "Sub(" + GetKey( icon.Sub ) + ")" );
		if(icon.BigSub != default) items.Add( "(" + GetKey( icon.BigSub ) + ")" );
		return string.Join( " ", items );
	}


	readonly ImageDiskCache _cache = new ImageDiskCache();

	#region private

	public Image GetTokenImage( IToken token ) {
		return token is HumanToken ht ? HumanTokenBuilder.Build( ht )
			: token.GetType().Name == "ManyMindsBeast" ? GetManyMindsBeast()
			: GetImage( token.Img );
	}

	Image GetManyMindsBeast() {
		string key = "many-minds-beast.png";
		if(_cache.Contains(key)) return _cache.Get(key);

		Bitmap img = GetImage(Img.Beast);
		using Graphics graphics = Graphics.FromImage( img );

		const int sacredSiteYellow = 60;
		new PixelAdjustment(new HslColorAdjuster(new HSL( sacredSiteYellow, 40,50)).GetNewColor).Adjust(img);
		// graphics.FillEllipse(Brushes.Purple, new Rectangle(0,0,img.Width,img.Height).InflateBy(-img.Width/3 ));

		_cache.Add(key,img);
		return img;
	}

	static readonly Bitmap Invisible = new Bitmap( 1, 1 );
	public Bitmap GetResourceImage( string? filename ) {
		if(filename is null) return Invisible;
		Stream imgStream = _assembly.GetManifestResourceStream( "SpiritIsland.Utilities.images."+filename )
			?? throw new ArgumentException($"No resource image found for {filename}");
		return new Bitmap( imgStream );
	}
	public Bitmap GetNoSymbol() => GetResourceImage( "icons.NoSymbol.png" );

	static string? ToResource( Img image ) => image switch {
		Img.RedX => "icons.NoSymbol.png",
		Img.Starlight_AssignElement => "icons.AssignElement.png",
		Img.CardPlay => "icons.cardplay.png",
		Img.Reclaim1 => "icons.reclaim 1.png",
		Img.ReclaimAll => "icons.ReclaimAll.png",
		Img.ReclaimHalf => "icons.Reclaim_Half.png",
		Img.CardPlayPlusN => "icons.Cardplayplus.png",
		Img.PlusOneRange => "icons.PlusOneRange.png",
		Img.GainCard => "icons.GainCard.png",
		Img.Icon_Major => "icons.major.png",
		Img.MovePresence => "icons.MovePresence.png",

		Img.Pushfromocean => "icons.Pushfromocean.png",
		Img.GatherToOcean => "icons.Gathertoocean.png",
		Img.Damage_2 => "icons.Damage_2.png",
		Img.Oneenergyfire => "icons.Oneenergyfire.png",

		Img.Land_Gather_Beasts  => "icons.Land_Gather_Beasts.png",
		Img.Land_Gather_Blight  => "icons.Land_Gather_Blight.png",
		Img.Land_Gather_Dahan   => "icons.Land_Gather_Dahan.png",
		Img.Land_Push_Town_City => "icons.Land_Push_Town-City.png",
		Img.Land_Push_Incarna   => "icons.Land_Push_Incarna.png",
		Img.Land_Push_Dahan     => "icons.Land_Push_Dahan.png",

		Img.GainEnergyEqualToCardPlays => "icons.GainEnergyEqualToCardPlays.png",
		Img.RangeArrow => "icons.Range.png",
		Img.MoveArrow => "icons.Moveicon.png",

		Img.Stone_Minor => "icons.minor.png",
		Img.ShiftingMemory_PrepareEl => "icons.PrepareElement.png",
		Img.ShiftingMemory_Discard2Prep => "icons.DiscardElementsForCardPlay.png",
		Img.Discard2 => "icons.Discard2Cards.png",
		Img.Starlight_GrowthOption1 => "icons.so1.png",
		Img.Starlight_GrowthOption2 => "icons.so2.png",
		Img.Starlight_GrowthOption3 => "icons.so3.png",
		Img.Starlight_GrowthOption4 => "icons.so4.png",
		Img.FracturedDays_Gain2Time => "icons.Gain2time.png",
		Img.FracturedDays_Gain1Timex2 => "icons.Gain1timex2.png",
		Img.FracturedDays_Gain1Timex3 => "icons.Gain1timex3.png",
		Img.FracturedDays_DrawDtnw => "icons.Daysthatneverweregrowthicon.png",


		Img.Coin         => "tokens.coin.png",
		Img.Token_Sun    => "tokens.Simple_sun.png",
		Img.Token_Moon   => "tokens.Simple_moon.png",
		Img.Token_Fire   => "tokens.Simple_fire.png",
		Img.Token_Air    => "tokens.Simple_air.png",
		Img.Token_Water  => "tokens.Simple_water.png",
		Img.Token_Plant  => "tokens.Simple_plant.png",
		Img.Token_Earth  => "tokens.Simple_earth.png",
		Img.Token_Animal => "tokens.Simple_animal.png",
		Img.Token_Any    => "tokens.Simple_any.png",

		Img.City     => "tokens.city.png",
		Img.Town     => "tokens.town.png",
		Img.Explorer => "tokens.explorer.png",
		Img.Dahan    => "tokens.dahan.png",
		Img.Blight   => "tokens.blight.png",
		Img.Beast    => "tokens.beast.png",
		Img.Wilds    => "tokens.wilds.png",
		Img.Disease  => "tokens.disease.png",
		Img.Badlands => "tokens.badlands.png",
		Img.Vitality => "tokens.vitality.png",
		Img.Quake    => "tokens.quake.png",
		Img.Defend   => "tokens.defend1orange.png",
		Img.Isolate  => "tokens.isolateorange.png",

		Img.Icon_Sun    => "icons.Simple_sun.png",
		Img.Icon_Moon   => "icons.Simple_moon.png",
		Img.Icon_Fire   => "icons.Simple_fire.png",
		Img.Icon_Air    => "icons.Simple_air.png",
		Img.Icon_Water  => "icons.Simple_water.png",
		Img.Icon_Plant  => "icons.Simple_plant.png",
		Img.Icon_Earth  => "icons.Simple_earth.png",
		Img.Icon_Animal => "icons.Simple_animal.png",

		Img.Icon_Dahan              => "icons.Dahanicon.png",
		Img.Icon_Invaders           => "icons.Invaders.png",
		Img.Icon_Coastal            => "icons.Coastal.png",
		Img.Icon_PresenceOrWilds    => "icons.wildsorpresence.png",
		Img.Icon_NoBlight           => "icons.Noblight.png",
		Img.Icon_Ocean              => "icons.Ocean.png",
		Img.Icon_TownCityOrBlight   => "icons.TownCityOrBlight.png",
		Img.Icon_Blight             => "icons.Blighticon.png",
		Img.Icon_Beast              => "icons.Beasticon.png",
		Img.Icon_Vitality           => "icons.Vitality.png",
		Img.Icon_Quake              => "icons.Quake.png",
		Img.Icon_Fear               => "icons.Fearicon.png",
		Img.OpenTheWays             => "icons.open-the-ways.png",
		Img.Icon_Wilds              => "icons.Wildsicon.png",
		Img.Icon_Fast               => "icons.Fasticon.png",
		Img.Icon_Presence           => "icons.Presenceicon.png",
		Img.Icon_Sacredsite         => "icons.Sacredsite.png",
		Img.Icon_Slow               => "icons.Slowicon.png",
		Img.Icon_Disease            => "icons.Diseaseicon.png",
		Img.Icon_Strife             => "icons.Strifeicon.png",
		Img.Icon_Badlands           => "icons.Badlands.png",
		Img.Icon_DestroyedPresence  => "icons.Destroyedpresence.png",
		Img.Icon_City               => "icons.Cityicon.png",
		Img.Icon_Town               => "icons.Townicon.png",
		Img.Icon_Explorer           => "icons.Explorericon.png",
		Img.Icon_Checkmark          => "icons.Checkmark.png",
		Img.Icon_Play               => "icons.Play.png",

		Img.Icon_Sands				=> "icons.Sandsland.png",
		Img.Icon_Mountain			=> "icons.Mountainland.png",
		Img.Icon_Jungle				=> "icons.Jungle.png",
		Img.Icon_Wetland			=> "icons.Wetlandland.png",
		Img.Icon_Spirit             => "icons.Spiriticon.png",
		Img.Icon_Discard            => "icons.Discard1Card.png",

		Img.Deck_Hand               => "hand.png",
		Img.Deck_Played             => "inplay.png",
		Img.Deck_Discarded          => "discard.png",
		Img.Deck_DaysThatNeverWere_Major => "major_inverted.png",
		Img.Deck_DaysThatNeverWere_Minor => "minor_inverted.png",

		Img.OrCurlyBefore => "icons.OR_Curly.png",
		Img.OrCurlyAfter => "icons.OR_Curly_180.png",

		Img.Icon_Incarna             => "icons.Incarna.png",
		Img.BoDDYS_Incarna           => "incarna.BoDDYS.png",
		Img.BoDDYS_Incarna_Empowered => "incarna.BoDDYS+.png",
		Img.EEB_Incarna              => "incarna.EEB.png",
		Img.EEB_Incarna_Empowered    => "incarna.EEB+.png",
		Img.L_Incarna                => "incarna.L.png",
		Img.S_Incarna                => "incarna.S.png",
		Img.S_Incarna_Empowered      => "incarna.S+.png",
		Img.T_Incarna                => "incarna.T.png",
		Img.TRotJ_Incarna            => "incarna.TRotJ.png",
		Img.TRotJ_Incarna_Empowered  => "incarna.TRotJ+.png",
		Img.WVKD_Incarna             => "incarna.WVKD.png",
		Img.WVKD_Incarna_Empowered   => "incarna.WVKD+.png",

		Img.Icon_EndlessDark => "icons.EndlessDark.png",

		Img.Icon_ImpendingCard => "icons.Impending.png",

		Img.None => null,
		_ => throw new System.ArgumentOutOfRangeException( nameof( image ), image.ToString() ),
	};

	#endregion

}
