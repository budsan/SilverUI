namespace Silver
{
	namespace UI
	{
		public class Resources
		{
			static private UnityEngine.Font _fontTitle = null;
			static private UnityEngine.Font _fontTabButton = null;
			static private UnityEngine.Font _fontContent = null;
			static private UnityEngine.Font _fontContentMonospace = null;

			private static UnityEngine.Sprite _spriteBackground = null;
			private static UnityEngine.Sprite _spriteTabButton = null;
			private static UnityEngine.Sprite _spriteButton = null;
			private static UnityEngine.Sprite _spriteCross = null;
			private static UnityEngine.Sprite _spriteCheckmark = null;
			private static UnityEngine.Sprite _spriteField = null;

			static private T GetBuiltInResource<T>(ref T content, string filename) where T : UnityEngine.Object
			{
				if (content == null)
					content = UnityEngine.Resources.GetBuiltinResource(typeof(T), filename) as T;

				return content;
			}

			static private T GetStaticResource<T>(ref T content, string filename) where T : UnityEngine.Object
			{
				if (content == null)
				{
					try
					{
						content = UnityEngine.Resources.Load<T>(filename);
					}
					catch (System.Exception exception)
					{
						UnityEngine.Debug.LogError(exception.Message);
					}
				}
					
				return content;
			}

			private delegate T ResourceLoaderDelegate<T>();

			static public UnityEngine.Font GetStaticFontTilte() { return GetStaticResource<UnityEngine.Font>(ref _fontTitle, "Ubuntu-B"); }
			static public UnityEngine.Font GetStaticFontTabs() { return GetStaticResource<UnityEngine.Font>(ref _fontTabButton, "Ubuntu-R"); }
			static public UnityEngine.Font GetStaticFontContent() { return GetStaticResource<UnityEngine.Font>(ref _fontContent, "Ubuntu-L"); }
			static public UnityEngine.Font GetStaticFontContentMonospace() { return GetStaticResource<UnityEngine.Font>(ref _fontContentMonospace, "UbuntuMono-R"); }

			static public UnityEngine.Sprite GetStaticSpriteBackground() { return GetStaticResource<UnityEngine.Sprite>(ref _spriteBackground, "Background"); }
			static public UnityEngine.Sprite GetStaticSpriteTabButton() { return GetStaticResource<UnityEngine.Sprite>(ref _spriteTabButton, "TabButton"); }
			static public UnityEngine.Sprite GetStaticSpriteButton() { return GetStaticResource<UnityEngine.Sprite>(ref _spriteButton, "Button"); }
			static public UnityEngine.Sprite GetStaticSpriteCross() { return GetStaticResource<UnityEngine.Sprite>(ref _spriteCross, "Cross"); }
			static public UnityEngine.Sprite GetStaticSpriteCheckmark() { return GetStaticResource<UnityEngine.Sprite>(ref _spriteCheckmark, "Checkmark"); }
			static public UnityEngine.Sprite GetStaticSpriteField() { return GetStaticResource<UnityEngine.Sprite>(ref _spriteField, "Field"); }

			//----------------------------//

			static public Resources _instance;
			static public Resources Instance
			{
				get
				{
					if (_instance == null)
						_instance = new Resources();

					return _instance;
				}
			}

			public UnityEngine.Font FontTitle = null;
			public UnityEngine.Font FontTabButton = null;
			public UnityEngine.Font FontContent = null;
			public UnityEngine.Font FontContentMonospace = null;

			public UnityEngine.Sprite SpriteBackground = null;
			public UnityEngine.Sprite SpriteTabButton = null;
			public UnityEngine.Sprite SpriteButton = null;
			public UnityEngine.Sprite SpriteCheckmark = null;
			public UnityEngine.Sprite SpriteField = null;

			private T GetResource<T>(ref T content, ResourceLoaderDelegate<T> loader) where T : UnityEngine.Object
			{
				if (content == null)
					content = loader();

				return content;
			}

			public UnityEngine.Font GetFontTilte() { return GetResource<UnityEngine.Font>(ref FontTitle, GetStaticFontTilte); }
			public UnityEngine.Font GetFontTabs() { return GetResource<UnityEngine.Font>(ref FontTabButton, GetStaticFontTabs); }
			public UnityEngine.Font GetFontContent() { return GetResource<UnityEngine.Font>(ref FontContent, GetStaticFontContent); }
			public UnityEngine.Font GetFontContentMonospace() { return GetResource<UnityEngine.Font>(ref FontContentMonospace, GetStaticFontContentMonospace); }

			public UnityEngine.Sprite GetSpriteBackground() { return GetResource<UnityEngine.Sprite>(ref SpriteBackground, GetStaticSpriteBackground); }
			public UnityEngine.Sprite GetSpriteTabButton() { return GetResource<UnityEngine.Sprite>(ref SpriteTabButton, GetStaticSpriteTabButton); }
			public UnityEngine.Sprite GetSpriteButton() { return GetResource<UnityEngine.Sprite>(ref SpriteButton, GetStaticSpriteButton); }
			public UnityEngine.Sprite GetSpriteCheckmark() { return GetResource<UnityEngine.Sprite>(ref SpriteCheckmark, GetStaticSpriteCheckmark); }
			public UnityEngine.Sprite GetSpriteField() { return GetResource<UnityEngine.Sprite>(ref SpriteField, GetStaticSpriteField); }
		}
	}
}
