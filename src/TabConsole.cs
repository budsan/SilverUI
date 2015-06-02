using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System.Collections.Generic;
using System.Linq;

namespace Silver
{
	namespace UI
	{
		public class TabConsole : MonoBehaviour, TabbedOverlay.Tab
		{
			[HideInInspector]
			private ConsoleLog consoleLog;
			private ConsoleCommandsRepository consoleCommandsRepository;
			private TabbedOverlay overlay = null;

			private GameObject root = null;
			private InputField inputField = null;
			private Text consoleText = null;
			private bool forceSubmit = false;

			public string TabName()
			{
				return "Console";
			}

			public void OnGainFocus()
			{
				enabled = true;
			}

			public void OnLostFocus()
			{
				enabled = false;
			}

			public bool ShowVerticalScroll() { return false; }
			public bool ShowHoritzonalScroll() { return false; }

			public GameObject CreateContent()
			{
				root = Helper.CreateGUIGameObject("TabConsole");
				Helper.SetRectTransform(root, 0, 0, 1, 1, 0.5f, 0.5f, 0, 0, 0, 0);
				VerticalLayoutGroup layout = root.AddComponent<VerticalLayoutGroup>();
				layout.childForceExpandHeight = false;
				layout.padding = new RectOffset(4, 4, 4, 4);
				layout.spacing = 8.0f;

				GameObject console = Helper.CreateGUIGameObject("ConsoleText");
				RectTransform content = Helper.SetRectTransform(console, 0, 0, 1, 0, 0.5f, 0, -16.0f, 0.0f, 0, 0.0f);
				consoleText = console.AddOrGetComponent<Text>();
				consoleText.font = Resources.Instance.GetFontContentMonospace();
				consoleText.color = Color.black;
				consoleText.fontSize = Helper.FontSize();
				ContentSizeFitter fitter = console.AddComponent<ContentSizeFitter>();
				fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

				GameObject scrollview = Helper.CreateScrollView(root, content, true, false);
				Helper.SetRectTransform(scrollview, 0, 0, 1, 1, 0.5f, 1, 0, 0, 0, 0);

				Image scrollImage = content.parent.gameObject.AddComponent<Image>();
				scrollImage.type = Image.Type.Sliced;
				scrollImage.sprite = Resources.Instance.GetSpriteField();
				scrollImage.color = new Color(1, 1, 1, 0.5f);

				GameObject inputdiv = Helper.CreateGUIGameObject("CommandField", root);
				Helper.SetRectTransform(inputdiv, 0, 0, 1, 0, 0.5f, 0, 0, 0, 0, 0);
				GameObject input = Helper.CreateInputField(inputdiv, "Command", "", ">");
				inputField = input.GetComponent<InputField>();
				LayoutElement inputElement = inputdiv.AddComponent<LayoutElement>();
				inputElement.preferredHeight = 30.0f;

				return root;
			}

			private void Start()
			{
				enabled = false;
				overlay = gameObject.FindManagerOrCreateIt<TabbedOverlay>();
				overlay.AddTab(this);

				consoleLog = ConsoleLog.Instance;
				consoleCommandsRepository = ConsoleCommandsRepository.Instance;
			}

			public void OnGUI()
			{
				HandleAutocomplete();

				if (KeyDown("[enter]") || KeyDown("return"))
					forceSubmit = true;
			}

			public void Update()
			{
				if (consoleText != null)
					consoleText.text = consoleLog.log;

				Submit();
			}

			private void Submit()
			{
				if (inputField == null)
					return;

				if (forceSubmit)
				{
					string input = inputField.text;

					string[] parts = input.TrimStart().Split(' ');
					string command = parts[0];
					string[] args = parts.Skip(1).ToArray();

					consoleLog.Log("> " + input);
					if (consoleCommandsRepository.HasCommand(command))
					{
						string response = consoleCommandsRepository.ExecuteCommand(command, args);
						if (response.Length > 0)
							consoleLog.Log(response);
					}
					else
					{
						if (command.Length > 0)
							consoleLog.Log("Command \"" + command + "\" not found");
					}

					inputField.text = "";
					forceSubmit = false;
				}
			}

			private void HandleAutocomplete()
			{
				if (inputField == null || !inputField.isFocused)
					return;

				string input = inputField.text;
				if (KeyDown("Tab"))
				{
					string[] parts = input.TrimStart().Split(' ');
					if (parts.Length > 1)
						return;

					int count = 0;
					string completed = "";
					string prefix = parts[0];
					foreach (string command in consoleCommandsRepository.Autocomplete(prefix))
					{
						if (count == 1)
						{
							consoleLog.Log("> " + input);
							consoleLog.Log(completed);
						}

						if (count > 0)
						{
							consoleLog.Log(command);
							completed = CommonPrefix(command, completed);
						}
						else
							completed = command;

						count++;
					}

					if (count > 0)
					{
						input = input.Substring(0, input.Length - prefix.Length);
						input += completed;
					}

					inputField.text = input;
				}
			}

			private bool KeyDown(string key)
			{
				return Event.current.Equals(Event.KeyboardEvent(key));
			}

			private string CommonPrefix(string left, string right)
			{
				string common = "";
				int minLen = System.Math.Min(left.Length, right.Length);
				for (int i = 0; i < minLen; i++)
				{
					if (left[i] == right[i])
						common += left[i];
					else
						break;
				}

				return common;
			}
		}
	}
}
