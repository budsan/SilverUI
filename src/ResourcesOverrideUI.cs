using UnityEngine;
using System.Collections;

namespace Silver
{
	public class ResourcesOverrideUI : MonoBehaviour
	{
		[SerializeField]
		private Font _fontTitle = null;
		public Font FontTitle { 
			get { return _fontTitle; } 
			set { UI.Resources.Instance.FontTitle = _fontTitle = value; } 
		}

		[SerializeField]
		private Font _fontTabButton = null;
		public Font FontTabButton { 
			get { return _fontTabButton; } 
			set { UI.Resources.Instance.FontTabButton = _fontTabButton = value; } 
		}

		[SerializeField]
		private Font _fontContent = null;
		public Font FontContent {
			get { return _fontContent; } 
			set { UI.Resources.Instance.FontContent = _fontContent = value; } 
		}

		[SerializeField]
		private Font _fontContentMonospace = null;
		public Font FontContentMonospace { 
			get { return _fontContentMonospace; } 
			set { UI.Resources.Instance.FontContentMonospace = _fontContentMonospace = value; } 
		}

		[SerializeField]
		private Sprite _spriteBackground = null;
		public Sprite SpriteBackground { 
			get { return _spriteBackground; } 
			set { UI.Resources.Instance.SpriteBackground = _spriteBackground = value; } 
		}

		[SerializeField]
		private Sprite _spriteTabButton = null;
		public Sprite SpriteTabButton { 
			get { return _spriteTabButton; } 
			set { UI.Resources.Instance.SpriteTabButton = _spriteTabButton = value; } 
		}

		[SerializeField]
		private Sprite _spriteButton = null;
		public Sprite SpriteButton { 
			get { return _spriteButton; } 
			set { UI.Resources.Instance.SpriteButton = _spriteButton = value; } 
		}

		[SerializeField]
		private Sprite _spriteCheckmark = null;
		public Sprite SpriteCheckmark { 
			get { return _spriteCheckmark; } 
			set { UI.Resources.Instance.SpriteCheckmark = _spriteCheckmark = value; } 
		}

		[SerializeField]
		private Sprite _spriteField = null;
		public Sprite SpriteField { 
			get { return _spriteField; } 
			set { UI.Resources.Instance.SpriteField = _spriteField = value; } 
		}

		void Awake()
		{
			FontTitle = _fontTitle;
			FontTabButton = _fontTabButton;
			FontContent = _fontContent;
			FontContentMonospace = _fontContentMonospace;
			SpriteBackground = _spriteBackground;
			SpriteTabButton = _spriteTabButton;
			SpriteButton = _spriteButton;
			SpriteCheckmark = _spriteCheckmark;
			SpriteField = _spriteField;
		}
	}
}
