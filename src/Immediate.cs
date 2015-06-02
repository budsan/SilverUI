using UnityEngine;
using UnityEngine.UI;

using System;
using System.Net;
using System.Collections.Generic;

namespace Silver
{
	namespace UI
	{
		public class Immediate
		{
			protected enum FieldType
			{
				None,
				VerticalLayout,
				HorizontalLayout,
				LabelField,
				Separator,
				ButtonText,
				ButtonImage,
				LineSeparator,
				Spacer,
				StringField,
				AddressIPField,
				IntField,
				FloatField,
				Toggle,
				Vector2Field,
				Vector3Field,
				QuaternionField,
				EnumField,
				EnumMaskField,
				Popup
			}

			[Flags]
			public enum FlagMask : int
			{
				None = 0,
				NoInteractable = 1 << 0,
				NoFieldLabel = 1 << 1
			}

			protected delegate void FieldUpdateFunction(object value);

			protected class FieldCache
			{
				public string name;
				public FieldType type = FieldType.None;
				public GameObject gameobject = null;
				public FlagMask lastMask = FlagMask.None;

				public bool changed = false;
				public object lastValue = null;
				private FieldUpdateFunction updateField = null;
				public FieldUpdateFunction UpdateField
				{
					set
					{
						updateField = value;
					}
				}

				private List<object> extraParams = new List<object>();

				private FieldCache parent = null;
				private int nestedIterator = -1;
				private List<FieldCache> nestedFields = null;
				private bool nestedRebuilt = false;

				public void BeginNested()
				{
					nestedRebuilt = false;
					nestedIterator = 0;

					if (nestedFields == null)
						nestedFields = new List<FieldCache>();
				}

				public FieldCache GetCurrentNested()
				{
					if (nestedIterator < 0)
						return null;

					FieldCache current = null;
					if (nestedIterator < nestedFields.Count)
					{
						current = nestedFields[nestedIterator];
					}
					else
					{
						do
						{
							current = new FieldCache();
							nestedFields.Add(current);
							current.parent = this;
						}
						while (nestedIterator >= nestedFields.Count);
					}

					return current;
				}

				public void NextNested()
				{
					if (nestedIterator < 0)
						return;

					nestedIterator++;
				}

				public void EndNested()
				{
					if (nestedIterator < 0)
						return;

					nestedIterator++;
					if (nestedIterator < nestedFields.Count)
					{
						for (int i = nestedIterator; i < nestedFields.Count; i++)
							nestedFields[i].Clear();

						nestedFields.RemoveRange(nestedIterator, nestedFields.Count - nestedIterator);
					}

					if (gameobject != null && nestedRebuilt)
					{
						int it = nestedFields.Count;
						while((--it) > 0)
						{
							if (nestedFields[it] != null && nestedFields[it].gameobject != null)
							{
								Transform tr = nestedFields[it].gameobject.transform;
								tr.SetParent(gameobject.transform, false);
								tr.SetAsFirstSibling();
							}
							
						}
					}
					
				}

				public void Update(object newValue)
				{
					if (!changed &&
						updateField != null && newValue != null &&
						(lastValue == null || !lastValue.Equals(newValue)))
					{
						updateField(newValue);
						lastValue = newValue;
					}

					changed = false;
				}

				public void Clear()
				{
					if (gameobject != null)
					{
						Helper.DestroyUI(gameobject);
						gameobject = null;
					}

					nestedIterator = -1;

					if (nestedFields != null)
						nestedFields.Clear();

					UpdateField = null;
					extraParams.Clear();
				}

				public void ResetSaved()
				{
					changed = false;

					foreach (FieldCache field in nestedFields)
						field.ResetSaved();
				}

				public bool NeedRebuild(string _name, FieldType _type)
				{
					if (name != _name || _type != type)
					{
						Clear();
						name = _name;
						type = _type;

						if (parent != null)
							parent.nestedRebuilt = true;

						return true;
					}

					return false;
				}

				public bool NeedRebuild(FieldType _type)
				{
					if (_type != type)
					{
						Clear();
						name = "Undefined";
						type = _type;

						if (parent != null)
							parent.nestedRebuilt = true;

						return true;
					}

					return false;
				}

				//return true if changed
				public bool SetExtraParam(int index, object param)
				{
					if(!(index < extraParams.Count && extraParams[index].Equals(param)))
					{
						while (index >= extraParams.Count)
							extraParams.Add(null);

						extraParams[index] = param;
						return true;
					}

					return false;
				}
			}

			public struct LayoutElementDescription
			{
				public float? flexibleHeight;
				public float? flexibleWidth;
				public bool ignoreLayout;
				public float? minHeight;
				public float? minWidth;
				public float? preferredHeight;
				public float? preferredWidth;

				public void Reset()
				{
					flexibleHeight = null;
					flexibleWidth = null;
					ignoreLayout = false;
					minHeight = null;
					minWidth = null;
					preferredHeight = null;
					preferredWidth = null;
				}

				public void SetLayout(LayoutElement element)
				{
					element.ignoreLayout = ignoreLayout;

					if (flexibleHeight.HasValue)
						element.flexibleHeight = flexibleHeight.Value;

					if (flexibleWidth.HasValue)
						element.flexibleWidth = flexibleWidth.Value;

					if (minHeight.HasValue)
						element.minHeight = minHeight.Value;

					if (minWidth.HasValue)
						element.minWidth = minWidth.Value;

					if (preferredHeight.HasValue)
						element.preferredHeight = preferredHeight.Value;

					if (preferredWidth.HasValue)
						element.preferredWidth = preferredWidth.Value;
				}
			}

			private LayoutElementDescription? nextLayoutElement = null;
			public LayoutElementDescription NextLayoutElement
			{
				set
				{
					nextLayoutElement = value;
				}
			}

			private bool SetNextLayoutElement(LayoutElement element)
			{
				if (nextLayoutElement.HasValue)
				{
					if (element != null)
						nextLayoutElement.Value.SetLayout(element);

					nextLayoutElement = null;
					return true;
				}

				return false;
			}

			public struct EnumString : IComparable
			{
				public string[] names;
				public int selected;

				public string SelectedName
				{
					get
					{
						if (names == null || names.Length == 0 || selected >= names.Length || selected < 0)
							return null;
						return names[selected];
					}
				}

				public string name
				{
					get
					{
						if (names != null && selected < names.Length && selected >= 0)
							return names[selected];

						return "";
					}
				}

				public static implicit operator EnumString(string[] _v)
				{
					EnumString enumString = new EnumString();
					enumString.names = _v;
					return enumString;
				}

				public override int GetHashCode()
				{
					return base.GetHashCode();
				}

				public override bool Equals(System.Object obj)
				{
					return CompareTo(obj) == 0;
				}

				public int CompareTo(object blah)
				{
					if (!(blah is EnumString))
						return 1;

					EnumString obj = (EnumString)blah;
					if (obj.names == null && names == null)
						return 0;
					else if (obj.names == null && names != null)
						return 1;
					else if (obj.names != null && names == null)
						return -1;

					if (obj.names.Length < names.Length)
						return -1;
					else if (obj.names.Length > names.Length)
						return 1;

					if (obj.selected < selected)
						return -1;
					else if (obj.selected > selected)
						return 1;

					if (obj.names == names)
						return 0;

					for (int i = 0; i < names.Length; i++)
					{
						int comp = obj.names[i].CompareTo(names[i]);
						if (comp != 0)
							return comp;
					}

					return 0;
				}
			}

			private GameObject parent = null;
			public GameObject Parent
			{
				get
				{
					return parent;
				}
				set
				{
					parent = value;
				}
			}

			private FieldCache root = null;
			private FieldCache Root
			{
				get
				{
					if (root == null)
					{
						root = new FieldCache();
						if (Parent != null)
						{
							foreach (Transform child in Parent.transform)
								Helper.DestroyUI(child.gameObject);
						}
					}

					return root;
				}
			}

			public GameObject gameObject
			{
				get
				{
					return Root.gameobject;
				}
			}

			protected Stack<FieldCache> context = new Stack<FieldCache>();
			protected bool IsRefreshingValues
			{
				get
				{
					return context.Count > 0;
				}
			}

			protected FieldCache CurrentField()
			{
				if (context.Count == 0)
					return Root;
				else
					return context.Peek().GetCurrentNested();
			}

			protected GameObject CurrentParent()
			{
				if (context.Count == 0)
					return Parent;
				else
					return context.Peek().gameobject;
			}

			protected FieldCache NextField()
			{
				if (context.Count > 0)
				{
					context.Peek().NextNested();
				}

				return CurrentField();
			}

			//---------------------------------------------------------------//

			protected GameObject CreateInputFieldStructure(FieldCache field, string name, FlagMask mask)
			{
				field.gameobject = Helper.CreateGUIGameObject("Field " + name, CurrentParent());
				Helper.SetRectTransform(field.gameobject, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0);

				LayoutElement element = field.gameobject.AddComponent<LayoutElement>();
				if (!SetNextLayoutElement(element))
				{
					element.preferredHeight = Helper.FontSizeWithMargins();
					element.flexibleWidth = 1;
				}

				bool NoFieldLabel = (mask & FlagMask.NoFieldLabel) != 0;
				if (!NoFieldLabel)
				{
					GameObject goLabel = Helper.CreateGUIGameObject("Label " + name, field.gameobject);
					Helper.SetRectTransform(goLabel, 0, 0, 0.3f, 1, 1, 1, -20, 0, 0, -8.0f);
					{
						Text txt = goLabel.AddOrGetComponent<Text>();
						txt.text = name;
						txt.fontSize = Helper.FontSize();
						txt.font = Resources.Instance.GetFontContent();
						txt.color = Helper.ColorText;
					}
				}
				
				GameObject goLeft = Helper.CreateGUIGameObject("Left " + name, field.gameobject);
				Helper.SetRectTransform(goLeft, NoFieldLabel ? 0.0f : 0.3f, 0, 1, 1, 1, 1, 0, 0, 0, 0);

				field.lastValue = null;
				field.changed = false;

				return goLeft;
			}

			protected void Vector_SetupInputText(GameObject label, GameObject field, string labelText, float min, float max)
			{
				Text text = label.AddOrGetComponent<Text>();
				text.fontSize = Helper.FontSize();
				text.font = Resources.Instance.GetFontContent();
				text.alignment = TextAnchor.MiddleLeft;
				text.color = Helper.ColorText;
				text.text = labelText;

				RectTransform rect = field.GetComponent<RectTransform>();
				RectTransform rectLabel = label.AddOrGetComponent<RectTransform>();

				rect.anchorMin = rectLabel.anchorMin = new Vector2(min, 0);
				rect.anchorMax = rectLabel.anchorMax = new Vector2(max, 1);

				rect.pivot = new Vector2(1, 0.5f);
				rectLabel.pivot = new Vector2(0, 0.5f);

				rect.sizeDelta = new Vector2(-(text.preferredWidth + (12)), 0);
				rectLabel.sizeDelta = Vector3.zero;

				rect.anchoredPosition = Vector3.zero;
				rectLabel.anchoredPosition = new Vector2(6, 0);
			}

			static protected bool Vector2_SetInputText(string x, string y, Image iX, Image iY, FieldCache field, bool IsRefreshingValues)
			{
				float floatResX, floatResY;
				if (CheckFloatField(x, out floatResX, iX) && CheckFloatField(y, out floatResY, iY) && !IsRefreshingValues)
				{
					field.lastValue = (object)new Vector2(floatResX, floatResY);
					field.changed = true;
					return true;
				}

				return false;
			}

			static protected bool Vector3_SetInputText(string x, string y, string z, Image iX, Image iY, Image iZ, FieldCache field, bool IsRefreshingValues)
			{
				float floatResX, floatResY, floatResZ;
				if (CheckFloatField(x, out floatResX, iX) && CheckFloatField(y, out floatResY, iY) && CheckFloatField(z, out floatResZ, iZ) && !IsRefreshingValues)
				{
					field.lastValue = (object)new Vector3(floatResX, floatResY, floatResZ);
					field.changed = true;
					return true;
				}

				return false;
			}

			static protected bool Quaternion_SetInputText(string x, string y, string z, Image iX, Image iY, Image iZ, FieldCache field, bool IsRefreshingValues)
			{
				float floatResX, floatResY, floatResZ;
				if (CheckFloatField(x, out floatResX, iX) && CheckFloatField(y, out floatResY, iY) && CheckFloatField(z, out floatResZ, iZ) && !IsRefreshingValues)
				{
					field.lastValue = (object)Quaternion.Euler(floatResX, floatResY, floatResZ);
					field.changed = true;
					return true;
				}

				return false;
			}

			protected static Color rightFieldColor = Color.white;
			protected static Color wrongFieldColor = new Color(1.0f, 0.8f, 0.8f);

			static protected bool CheckIpText(string value, Image fieldImage)
			{
				try
				{
					IPAddress.Parse(value);
					fieldImage.color = rightFieldColor;
				}
				catch
				{
					fieldImage.color = wrongFieldColor;
					return false;
				}

				return true;
			}

			static protected bool CheckIntField(string value, out int result, Image fieldImage)
			{
				if (int.TryParse(value, out result))
				{
					fieldImage.color = rightFieldColor;
					return true;
				}
				else
				{
					fieldImage.color = wrongFieldColor;
					return false;
				}
			}

			static protected bool CheckFloatField(string value, out float result, Image fieldImage)
			{
				if (float.TryParse(value, out result))
				{
					fieldImage.color = rightFieldColor;
					return true;
				}
				else
				{
					fieldImage.color = wrongFieldColor;
					return false;
				}
			}

			static protected void CheckButtonFlags(FieldCache buttonCache, FlagMask mask)
			{
				if (buttonCache.lastMask != mask)
				{
					FlagMask diff = buttonCache.lastMask ^ mask;
					if ((diff & FlagMask.NoInteractable) != 0)
					{
						Button but = buttonCache.gameobject.GetComponent<Button>();
						but.interactable = (mask & FlagMask.NoInteractable) == 0;
					}

					buttonCache.lastMask = mask;
				}
			}

			//---------------------------------------------------------------//

			private int changeCheckCount = -1;
			public void BeginChangeCheck()
			{
				changeCheckCount = 0;
			}

			private void ChangeCheckAddOne()
			{
				if (changeCheckCount >= 0)
				{
					changeCheckCount++;
				}
			}

			public bool EndChangeCheck()
			{
				bool res = changeCheckCount > 0;
				changeCheckCount = -1;
				return res;
			}

			//---------------------------------------------------------------//

			protected void CheckNextLayout(FieldCache currentField)
			{
				if (currentField.gameobject != null)
				{
					if (nextLayoutElement.HasValue)
					{
						LayoutElement element = currentField.gameobject.GetComponent<LayoutElement>();
						SetNextLayoutElement(element);
					}	
				}
				else
				{
					if (nextLayoutElement.HasValue)
						nextLayoutElement = null;
				}
			}

			//---------------------------------------------------------------//

			public delegate void OnDrawGUIDelegate();

			public void VerticalLayout(OnDrawGUIDelegate onDraw)
			{
				FieldCache layout = NextField();
				if (layout.NeedRebuild(FieldType.VerticalLayout))
				{
					layout.gameobject = Helper.CreateGUIGameObject("VerticalLayout", CurrentParent());
					Helper.SetRectTransform(layout.gameobject, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0);
					VerticalLayoutGroup vertical = layout.gameobject.AddComponent<VerticalLayoutGroup>();
					vertical.childForceExpandWidth = true;
					vertical.childForceExpandHeight = false;
					vertical.spacing = (2.0f);

					if (context.Count == 0)
					{
						ContentSizeFitter fitter = layout.gameobject.AddComponent<ContentSizeFitter>();
						fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
						fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

						RectOffset padding = vertical.padding;
						padding.left = (int)(10.0f);
						padding.right = (int)(10.0f);
						padding.top = (int)(10.0f);
						padding.bottom = (int)(10.0f);
						vertical.padding = padding;
					}
					else
					{
						LayoutElement element = layout.gameobject.AddComponent<LayoutElement>();
						element.preferredWidth = Helper.FontSizeWithMargins();
					}
				}

				context.Push(layout);
				layout.BeginNested();

				Exception innerException = null;
				try
				{
					if (onDraw != null)
						onDraw();
				}
				catch(Exception ex)
				{
					innerException = ex;
				}

				while (context.Count > 0 && context.Peek() != layout)
				{
					FieldCache otherLayout = context.Pop();
					Debug.LogError(Enum.GetName(otherLayout.type.GetType(), otherLayout.type) + " wasn't ended correctly. Popping.");
				}
				
				if (context.Count > 0)
				{
					layout.EndNested();
					context.Pop();
				}
				
				if (innerException != null)
					throw innerException;
			}

			public void HorizontalLayout(OnDrawGUIDelegate onDraw)
			{
				BeginHorizontalLayout();

				Exception innerException = null;
				try
				{
					if (onDraw != null)
						onDraw();
				}
				catch (Exception ex)
				{
					innerException = ex;
				}

				EndHorizontalLayout();

				if (innerException != null)
					throw innerException;
			}


			public void BeginHorizontalLayout()
			{
				FieldCache layout = NextField();
				if (layout.NeedRebuild(FieldType.HorizontalLayout))
				{
					layout.gameobject = Helper.CreateGUIGameObject("HorizontalLayout", CurrentParent());
					Helper.SetRectTransform(layout.gameobject, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0);
					HorizontalLayoutGroup vertical = layout.gameobject.AddComponent<HorizontalLayoutGroup>();
					vertical.childForceExpandWidth = false;
					vertical.childForceExpandHeight = true;
					vertical.spacing = (4.0f);

					if (context.Count == 0)
					{
						ContentSizeFitter fitter = layout.gameobject.AddComponent<ContentSizeFitter>();
						fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
						fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

						RectOffset padding = vertical.padding;
						padding.left = (int)(10.0f);
						padding.right = (int)(10.0f);
						padding.top = (int)(10.0f);
						padding.bottom = (int)(10.0f);
						vertical.padding = padding;
					}
					else
					{
						LayoutElement element = layout.gameobject.AddComponent<LayoutElement>();
						if (!SetNextLayoutElement(element))
							element.preferredWidth = Helper.FontSizeWithMargins();
					}
				}

				context.Push(layout);
				layout.BeginNested();
			}

			public void EndHorizontalLayout()
			{
				FieldCache layout;
				while (context.Count > 0)
				{
					layout = context.Pop();
					if (layout.type == FieldType.HorizontalLayout)
					{
						layout.EndNested();
						break;
					}
					else
					{
						Debug.LogError(Enum.GetName(layout.type.GetType(), layout.type) + " wasn't ended correctly. Popping.");
					}
				}
			}

			public void LabelField(string value)
			{
				LabelField(value, 18);
			}

			public void TitleField(string value)
			{
				LabelField(value, 24);
			}

			public void LabelField(string value, int size)
			{
				FieldCache label = NextField();
				if (label.NeedRebuild(FieldType.LabelField))
				{
					label.gameobject = Helper.CreateGUIGameObject("Title " + value, CurrentParent());
					Helper.SetRectTransform(label.gameobject, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0);

					Text text = Helper.AddText(label.gameobject, value);
					text.fontSize = Helper.FontSize(size);
					text.alignment = TextAnchor.MiddleLeft;

					label.SetExtraParam(0, (object)value);
					label.SetExtraParam(1, (object)size);
				}

				bool changed = false;
				changed = label.SetExtraParam(0, (object)value) || changed;
				changed = label.SetExtraParam(1, (object)size) || changed;

				if (changed)
				{
					Text text = label.gameobject.GetComponent<Text>();
					if (text != null) {
						text.text = value;
						text.fontSize = Helper.FontSize(size);
					}
				}

				CheckNextLayout(label);
			}

			//\todo look at how unity implements foldout. In theory, it's a sideways triangle with a label when it's not expanded. When it's expanded, the triangle is downwards and everything nested is shown with a little offset to the left
			public void Foldout(bool isExpanded, string title)
			{

			}

			public void Separator(int spaces = 0)
			{
				FieldCache separator = NextField();
				if (separator.NeedRebuild(FieldType.Separator))
				{
					separator.gameobject = Helper.CreateGUIGameObject("Separator", CurrentParent());
					Helper.SetRectTransform(separator.gameobject, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0);

					LayoutElement element = separator.gameobject.AddComponent<LayoutElement>();
					if (!SetNextLayoutElement(element)) {
						element.preferredHeight = spaces * Helper.FontSizeWithMargins();
						element.flexibleWidth = 1;
					}

					separator.SetExtraParam(0, (object)spaces);
				}

				if (separator.SetExtraParam(0, (object)spaces))
				{
					LayoutElement element = separator.gameobject.GetComponent<LayoutElement>();
					if (element != null && !SetNextLayoutElement(element))
					{
						element.preferredHeight = spaces * Helper.FontSizeWithMargins();
						element.flexibleWidth = 1;
					}
				}

				CheckNextLayout(separator);
			}

			public bool Button(string text, FlagMask mask = FlagMask.None)
			{
				FieldCache buttonCache = NextField();
				if (buttonCache.NeedRebuild(FieldType.ButtonText))
				{
					buttonCache.gameobject = Helper.CreateButton(CurrentParent(), "Button " + text, text, () =>
					{
						buttonCache.changed = true;
					});

					Text innerText = buttonCache.gameobject.transform.Find(Helper.NameButtonText).gameObject.GetComponent<Text>();
					LayoutElement element = buttonCache.gameobject.AddComponent<LayoutElement>();
					if (!SetNextLayoutElement(element)) {
						element.preferredWidth = innerText.preferredWidth + (16.0f);
						//layout.flexibleWidth = 1;
						element.minHeight = Helper.FontSizeWithMargins();
					}

					buttonCache.SetExtraParam(0, (object)text);
				}

				if (buttonCache.SetExtraParam(0, (object)text))
				{
					buttonCache.gameobject.name = "Button " + text;
					Text innerText = buttonCache.gameobject.transform.Find(Helper.NameButtonText).gameObject.GetComponent<Text>();
					innerText.text = text;
					LayoutElement element = buttonCache.gameobject.AddComponent<LayoutElement>();
					if (!SetNextLayoutElement(element))
					{
						element.preferredWidth = innerText.preferredWidth + (16.0f);
						//layout.flexibleWidth = 1;
						element.minHeight = Helper.FontSizeWithMargins();
					}
				}

				CheckNextLayout(buttonCache);
				CheckButtonFlags(buttonCache, mask);

				bool pressed = buttonCache.changed;
				buttonCache.changed = false;

				if (pressed)
					ChangeCheckAddOne();

				return pressed;
			}

			public bool Button(Sprite image, FlagMask mask = FlagMask.None)
			{
				FieldCache buttonCache = NextField();
				if (buttonCache.NeedRebuild(image.name, FieldType.ButtonImage))
				{
					buttonCache.gameobject = Helper.CreateButton(CurrentParent(), "Button " + image.name, image, () =>
					{
						buttonCache.changed = true;
					});

					LayoutElement element = buttonCache.gameobject.AddComponent<LayoutElement>();
					if (!SetNextLayoutElement(element)) {
						element.minWidth = Helper.FontSizeWithMargins();
						element.minHeight = Helper.FontSizeWithMargins();
					}

					buttonCache.SetExtraParam(0, (object)image);
				}

				if (buttonCache.SetExtraParam(0, (object)image))
				{
					buttonCache.gameobject.name = "Button " + image.name;
					Image innerImage = buttonCache.gameobject.transform.Find(Helper.NameButtonImage).gameObject.GetComponent<Image>();
					innerImage.sprite = image;
				}

				CheckNextLayout(buttonCache);
				CheckButtonFlags(buttonCache, mask);

				bool pressed = buttonCache.changed;
				buttonCache.changed = false;

				if (pressed)
					ChangeCheckAddOne();

				return pressed;
			}

			public void LineSeparator()
			{
				FieldCache separator = NextField();
				if (separator.NeedRebuild("LineSeparator", FieldType.LineSeparator))
				{
					separator.gameobject = Helper.CreateGUIGameObject("LineSeparator", CurrentParent());
					Helper.SetRectTransform(separator.gameobject, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0);

					LayoutElement element = separator.gameobject.AddComponent<LayoutElement>();
					if (!SetNextLayoutElement(element))
						element.preferredHeight = (2.0f);

					Image image = separator.gameobject.AddComponent<Image>();
					image.color = new Color(0, 0, 0, 0.25f);
				}

				CheckNextLayout(separator);
			}

			public void FlexibleSpace()
			{
				FieldCache separator = NextField();
				if (separator.NeedRebuild("Spacer", FieldType.Spacer))
				{
					separator.gameobject = Helper.CreateGUIGameObject("Spacer", CurrentParent());
					Helper.SetRectTransform(separator.gameobject, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0);

					LayoutElement element = separator.gameobject.AddComponent<LayoutElement>();
					if (!SetNextLayoutElement(element))
					{
						element.flexibleWidth = 1;
						element.flexibleHeight = 1;
					}
				}

				CheckNextLayout(separator);
			}

			public string StringField(string name, string value, FlagMask mask = FlagMask.None)
			{
				FieldCache field = NextField();
				if (field.NeedRebuild(name, FieldType.StringField))
				{
					GameObject goLeft = CreateInputFieldStructure(field, name, mask);
					InputField inField = Helper.CreateInputField(goLeft, name, "", "Enter text...").GetComponent<InputField>();
					Image fieldImage = inField.gameObject.GetComponent<Image>();

					inField.onValueChange.AddListener((string newValue) =>
					{
						if (!IsRefreshingValues)
						{
							field.lastValue = (object)newValue;
							field.changed = true;
						}
					});

					field.UpdateField = (object newValue) =>
					{
						inField.text = (string)newValue;
					};

					return value;
				}

				CheckNextLayout(field);
				field.Update(value);
				return (string)field.lastValue;
			}

			public string AddressIPField(string name, string value, FlagMask mask = FlagMask.None)
			{
				FieldCache field = NextField();
				if (field.NeedRebuild(name, FieldType.AddressIPField))
				{
					GameObject goLeft = CreateInputFieldStructure(field, name, mask);
					InputField inField = Helper.CreateInputField(goLeft, name, "", "Enter text...").GetComponent<InputField>();
					Image fieldImage = inField.gameObject.GetComponent<Image>();

					inField.onValueChange.AddListener((string newValue) =>
					{
						if (CheckIpText(newValue, fieldImage) && !IsRefreshingValues)
						{
							field.lastValue = (object)newValue;
							field.changed = true;
						}
					});

					field.UpdateField = (object newValue) =>
					{
						inField.text = (string)newValue;
					};

					return value;
				}

				if (field.changed)
					ChangeCheckAddOne();

				CheckNextLayout(field);
				field.Update(value);
				return (string)field.lastValue;
			}

			public int IntField(string name, int value, FlagMask mask = FlagMask.None)
			{
				FieldCache field = NextField();
				if (field.NeedRebuild(name, FieldType.IntField))
				{
					GameObject goLeft = CreateInputFieldStructure(field, name, mask);
					InputField inField = Helper.CreateInputField(goLeft, name, "", "Enter an integer...").GetComponent<InputField>();
					Image fieldImage = inField.gameObject.GetComponent<Image>();

					inField.onValueChange.AddListener((string newValue) =>
					{
						int intRes;
						if (CheckIntField(newValue, out intRes, fieldImage) && !IsRefreshingValues)
						{
							field.lastValue = (object)intRes;
							field.changed = true;
						}
					});

					field.UpdateField = (object newValue) =>
					{
						inField.text = newValue.ToString();
					};

					return value;
				}

				if (field.changed)
					ChangeCheckAddOne();

				CheckNextLayout(field);
				field.Update(value);
				return (int)field.lastValue;
			}

			public float FloatField(string name, float value, FlagMask mask = FlagMask.None)
			{
				FieldCache field = NextField();
				if (field.NeedRebuild(name, FieldType.FloatField))
				{
					GameObject goLeft = CreateInputFieldStructure(field, name, mask);
					InputField inField = Helper.CreateInputField(goLeft, name, "", "Enter a float...").GetComponent<InputField>();
					Image fieldImage = inField.gameObject.GetComponent<Image>();

					inField.onEndEdit.AddListener((string newValue) =>
					{
						float floatRes;
						if (CheckFloatField(newValue, out floatRes, fieldImage) && !IsRefreshingValues)
						{
							field.lastValue = (object)floatRes;
							field.changed = true;
						}
					});

					inField.onValueChange.AddListener((string newValue) =>
					{
						float floatRes;
						CheckFloatField(newValue, out floatRes, fieldImage);
					});

					field.UpdateField = (object newValue) =>
					{
						inField.text = newValue.ToString();
					};

					return value;
				}

				if (field.changed)
					ChangeCheckAddOne();

				CheckNextLayout(field);
				field.Update(value);
				return (float)field.lastValue;
			}

			public bool Toggle(string name, bool value, FlagMask mask = FlagMask.None)
			{
				FieldCache field = NextField();
				if (field.NeedRebuild(name, FieldType.Toggle))
				{
					GameObject goLeft = CreateInputFieldStructure(field, name, mask);
					Toggle toggle = Helper.CreateToggle(goLeft, 20, (bool)value).GetComponent<Toggle>();
	
					field.UpdateField = (object newValue) =>
					{
						toggle.isOn = (bool)newValue;
					};

					toggle.onValueChanged.AddListener((bool newValue) =>
					{
						if (!IsRefreshingValues)
						{
							field.lastValue = (object)newValue;
							field.changed = true;
						}
					});

					return value;
				}

				if (field.changed)
					ChangeCheckAddOne();

				CheckNextLayout(field);
				field.Update(value);
				return (bool)field.lastValue;
			}

			public Vector2 Vector2Field(string name, Vector2 value, FlagMask mask = FlagMask.None)
			{
				FieldCache field = NextField();
				if (field.NeedRebuild(name, FieldType.Vector2Field))
				{
					GameObject goLeft = CreateInputFieldStructure(field, name, mask);

					GameObject goLabelX = Helper.CreateGUIGameObject("Label X " + name, goLeft);
					GameObject goInputX = Helper.CreateInputField(goLeft, "X " + name, "");
					Vector_SetupInputText(goLabelX, goInputX, "X", 0, 0.33f);

					GameObject goLabelY = Helper.CreateGUIGameObject("Label Y " + name, goLeft);
					GameObject goInputY = Helper.CreateInputField(goLeft, "Y " + name, "");
					Vector_SetupInputText(goLabelY, goInputY, "Y", 0.33f, 0.66f);

					InputField ifieldX = goInputX.GetComponent<InputField>();
					InputField ifieldY = goInputY.GetComponent<InputField>();

					Image fieldImageX = ifieldX.gameObject.GetComponent<Image>();
					Image fieldImageY = ifieldX.gameObject.GetComponent<Image>();

					ifieldX.onEndEdit.AddListener((string newValue) => { Vector2_SetInputText(newValue, ifieldY.text, fieldImageX, fieldImageY, field, IsRefreshingValues); });
					ifieldY.onEndEdit.AddListener((string newValue) => { Vector2_SetInputText(ifieldX.text, newValue, fieldImageX, fieldImageY, field, IsRefreshingValues); });
					ifieldX.onValueChange.AddListener((string newValue) => { Vector2_SetInputText(newValue, ifieldY.text, fieldImageX, fieldImageY, field, true); });
					ifieldY.onValueChange.AddListener((string newValue) => { Vector2_SetInputText(ifieldX.text, newValue, fieldImageX, fieldImageY, field, true); });

					field.UpdateField = (object newValue) =>
					{
						Vector2 v = (Vector2)newValue;
						ifieldX.text = v.x.ToString();
						ifieldY.text = v.y.ToString();
					};

					return value;
				}

				if (field.changed)
					ChangeCheckAddOne();

				CheckNextLayout(field);
				field.Update(value);
				return (Vector2)field.lastValue;
			}

			public Vector3 Vector3Field(string name, Vector3 value, FlagMask mask = FlagMask.None)
			{
				FieldCache field = NextField();
				if (field.NeedRebuild(name, FieldType.Vector3Field))
				{
					GameObject goLeft = CreateInputFieldStructure(field, name, mask);

					GameObject goLabelX = Helper.CreateGUIGameObject("Label X " + name, goLeft);
					GameObject goInputX = Helper.CreateInputField(goLeft, "X " + name, "");
					Vector_SetupInputText(goLabelX, goInputX, "X", 0, 0.33f);

					GameObject goLabelY = Helper.CreateGUIGameObject("Label Y " + name, goLeft);
					GameObject goInputY = Helper.CreateInputField(goLeft, "Y " + name, "");
					Vector_SetupInputText(goLabelY, goInputY, "Y", 0.33f, 0.66f);

					GameObject goLabelZ = Helper.CreateGUIGameObject("Label Z " + name, goLeft);
					GameObject goInputZ = Helper.CreateInputField(goLeft, "Z " + name, "");
					Vector_SetupInputText(goLabelZ, goInputZ, "Z", 0.66f, 1);

					InputField ifieldX = goInputX.GetComponent<InputField>();
					InputField ifieldY = goInputY.GetComponent<InputField>();
					InputField ifieldZ = goInputZ.GetComponent<InputField>();

					Image fieldImageX = ifieldX.gameObject.GetComponent<Image>();
					Image fieldImageY = ifieldX.gameObject.GetComponent<Image>();
					Image fieldImageZ = ifieldX.gameObject.GetComponent<Image>();

					ifieldX.onEndEdit.AddListener((string newValue) => { Vector3_SetInputText(newValue, ifieldY.text, ifieldZ.text, fieldImageX, fieldImageY, fieldImageZ, field, IsRefreshingValues); });
					ifieldY.onEndEdit.AddListener((string newValue) => { Vector3_SetInputText(ifieldX.text, newValue, ifieldZ.text, fieldImageX, fieldImageY, fieldImageZ, field, IsRefreshingValues); });
					ifieldZ.onEndEdit.AddListener((string newValue) => { Vector3_SetInputText(ifieldX.text, ifieldY.text, newValue, fieldImageX, fieldImageY, fieldImageZ, field, IsRefreshingValues); });
					ifieldX.onValueChange.AddListener((string newValue) => { Vector3_SetInputText(newValue, ifieldY.text, ifieldZ.text, fieldImageX, fieldImageY, fieldImageZ, field, true); });
					ifieldY.onValueChange.AddListener((string newValue) => { Vector3_SetInputText(ifieldX.text, newValue, ifieldZ.text, fieldImageX, fieldImageY, fieldImageZ, field, true); });
					ifieldZ.onValueChange.AddListener((string newValue) => { Vector3_SetInputText(ifieldX.text, ifieldY.text, newValue, fieldImageX, fieldImageY, fieldImageZ, field, true); });

					field.UpdateField = (object newValue) =>
					{
						Vector3 v = (Vector3)newValue;
						ifieldX.text = v.x.ToString();
						ifieldY.text = v.y.ToString();
						ifieldZ.text = v.z.ToString();
					};

					return value;
				}

				if (field.changed)
					ChangeCheckAddOne();

				CheckNextLayout(field);
				field.Update(value);
				return (Vector3)field.lastValue;
			}

			public Quaternion QuaternionField(string name, Quaternion value, FlagMask mask = FlagMask.None)
			{
				FieldCache field = NextField();
				if (field.NeedRebuild(name, FieldType.QuaternionField))
				{
					GameObject goLeft = CreateInputFieldStructure(field, name, mask);

					GameObject goLabelX = Helper.CreateGUIGameObject("Label X " + name, goLeft);
					GameObject goInputX = Helper.CreateInputField(goLeft, "X " + name, "");
					Vector_SetupInputText(goLabelX, goInputX, "X", 0, 0.33f);

					GameObject goLabelY = Helper.CreateGUIGameObject("Label Y " + name, goLeft);
					GameObject goInputY = Helper.CreateInputField(goLeft, "Y " + name, "");
					Vector_SetupInputText(goLabelY, goInputY, "Y", 0.33f, 0.66f);

					GameObject goLabelZ = Helper.CreateGUIGameObject("Label Z " + name, goLeft);
					GameObject goInputZ = Helper.CreateInputField(goLeft, "Z " + name, "");
					Vector_SetupInputText(goLabelZ, goInputZ, "Z", 0.66f, 1);

					InputField ifieldX = goInputX.GetComponent<InputField>();
					InputField ifieldY = goInputY.GetComponent<InputField>();
					InputField ifieldZ = goInputZ.GetComponent<InputField>();

					Image fieldImageX = ifieldX.gameObject.GetComponent<Image>();
					Image fieldImageY = ifieldX.gameObject.GetComponent<Image>();
					Image fieldImageZ = ifieldX.gameObject.GetComponent<Image>();

					ifieldX.onEndEdit.AddListener((string newValue) => { Quaternion_SetInputText(newValue, ifieldY.text, ifieldZ.text, fieldImageX, fieldImageY, fieldImageZ, field, IsRefreshingValues); });
					ifieldY.onEndEdit.AddListener((string newValue) => { Quaternion_SetInputText(ifieldX.text, newValue, ifieldZ.text, fieldImageX, fieldImageY, fieldImageZ, field, IsRefreshingValues); });
					ifieldZ.onEndEdit.AddListener((string newValue) => { Quaternion_SetInputText(ifieldX.text, ifieldY.text, newValue, fieldImageX, fieldImageY, fieldImageZ, field, IsRefreshingValues); });
					ifieldX.onValueChange.AddListener((string newValue) => { Quaternion_SetInputText(newValue, ifieldY.text, ifieldZ.text, fieldImageX, fieldImageY, fieldImageZ, field, true); });
					ifieldY.onValueChange.AddListener((string newValue) => { Quaternion_SetInputText(ifieldX.text, newValue, ifieldZ.text, fieldImageX, fieldImageY, fieldImageZ, field, true); });
					ifieldZ.onValueChange.AddListener((string newValue) => { Quaternion_SetInputText(ifieldX.text, ifieldY.text, newValue, fieldImageX, fieldImageY, fieldImageZ, field, true); });

					field.UpdateField = (object newValue) =>
					{
						Vector3 v = ((Quaternion)newValue).eulerAngles;
						ifieldX.text = v.x.ToString();
						ifieldY.text = v.y.ToString();
						ifieldZ.text = v.z.ToString();
					};

					return value;
				}

				if (field.changed)
					ChangeCheckAddOne();

				CheckNextLayout(field);
				field.Update(value);
				return (Quaternion)field.lastValue;
			}

			public Enum FieldEnum(string name, Enum value, FlagMask mask = FlagMask.None)
			{
				FieldCache field = NextField();
				if (field.NeedRebuild(name, FieldType.EnumField))
				{
					GameObject goLeft = CreateInputFieldStructure(field, name, mask);
					DropdownEnum dropdownEnum = goLeft.AddOrGetComponent<DropdownEnum>();
					if (Parent != null)
						dropdownEnum.Dropdown.OverlayParent = Parent;

					dropdownEnum.CurrentEnum = value;
					dropdownEnum.OnChanged = () =>
					{
						if (!IsRefreshingValues)
						{
							field.lastValue = (object)dropdownEnum.CurrentEnum;
							field.changed = true;
						}
					};

					field.UpdateField = (object newValue) =>
					{
						dropdownEnum.CurrentEnum = (Enum)newValue;
					};

					return value;
				}

				if (field.changed)
					ChangeCheckAddOne();

				CheckNextLayout(field);
				field.Update(value);
				return (Enum)field.lastValue;
			}

			public Enum FieldEnumMask(string name, Enum value, FlagMask mask = FlagMask.None)
			{
				FieldCache field = NextField();
				if (field.NeedRebuild(name, FieldType.EnumMaskField))
				{
					GameObject goLeft = CreateInputFieldStructure(field, name, mask);
					Type enumType = value.GetType();

					DropdownMask dropdownMask = goLeft.AddOrGetComponent<DropdownMask>();
					if (Parent != null)
						dropdownMask.Dropdown.OverlayParent = Parent;

					dropdownMask.SetupEnum(value.GetType());
					dropdownMask.Mask = Convert.ToInt32(value);
					dropdownMask.OnChanged = () =>
					{
						if (!IsRefreshingValues)
						{
							field.lastValue = (object)Enum.ToObject(enumType, dropdownMask.Mask);
							field.changed = true;
						}
					};

					field.UpdateField = (object newValue) =>
					{
						dropdownMask.Mask = Convert.ToInt32(newValue);
					};

					return value;
				}

				if (field.changed)
					ChangeCheckAddOne();

				CheckNextLayout(field);
				field.Update(value);
				return (Enum)field.lastValue;
			}

			public int Popup(string name, int selected, string[] names, FlagMask mask = FlagMask.None)
			{
				EnumString enumString;
				enumString.names = names;
				enumString.selected = selected;

				return Popup(name, enumString, mask);
			}

			public int Popup(string name, EnumString value, FlagMask mask = FlagMask.None)
			{
				FieldCache field = NextField();
				if (field.NeedRebuild(name, FieldType.Popup))
				{
					GameObject goLeft = CreateInputFieldStructure(field, name, mask);
					Dropdown dropdown = goLeft.AddOrGetComponent<Dropdown>();
					if (Parent != null)
						dropdown.OverlayParent = Parent;

					dropdown.OnSelectedChanged = () =>
					{
						value.selected = dropdown.SelectedItem;
						field.lastValue = (object)value;
						field.changed = true;
					};

					field.UpdateField = (object newValue) =>
					{
						EnumString enumString = (EnumString)newValue;
						List<DropdownItem> items = new List<DropdownItem>();
						if (enumString.names != null)
						{
							foreach (string valueName in enumString.names)
							{
								items.Add(new DropdownItem(valueName));
							}
						}

						dropdown.Items = items.ToArray();
						dropdown.SelectedItem = enumString.selected;
					};

					field.Update(value);
				}

				if (field.changed)
					ChangeCheckAddOne();

				CheckNextLayout(field);
				field.Update(value);
				return ((EnumString)field.lastValue).selected;
			}
		}
	}
}
