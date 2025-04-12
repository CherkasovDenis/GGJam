using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GGJam.Dialogs.Scripts
{
	[Serializable]
	public struct Characters
	{
		public Character Character;
		public Sprite Sprite;
	}

	public class DialogService : MonoBehaviour
	{
		[SerializeField]
		private Image _character;
		[SerializeField]
		private TMP_Text _dialog;
		[SerializeField]
		private CanvasGroup _canvasGroup;
		[SerializeField]
		private Characters[] _allCharacters;
		[SerializeField]
		private Button _nextButton;
		[SerializeField]
		private DialogConfig[] _dialogConfigs;

		private bool _showed;

		private void Awake()
		{
			_canvasGroup.alpha = 0;
			gameObject.SetActive(false);
		}

		public void ShowWindow()
		{
			gameObject.SetActive(true);
			_canvasGroup.alpha = 0;
			_canvasGroup.DOFade(1f, 1f);
			_showed = true;
		}

		public void HideWindow()
		{
			_canvasGroup.DOFade(0f, 1f)
				.OnComplete(() =>
				{
					gameObject.SetActive(false);
					_showed = false;
				});
		}

		public async UniTask ShowDialog(string dialogKey)
		{
			var dialog = GetDialogsFromAllConfigs(dialogKey);
			if (dialog == null)
			{
				Debug.LogError($"dialog with key {dialogKey} not found");
				return;
			}

			if (!_showed)
				ShowWindow();

			SetSprite(dialog);

			SetText(dialog);

			await _nextButton.OnClickAsync();
		}

		private void SetSprite(Dialog dialog)
		{
			var sprite = GetSprite(dialog.Character);
			_character.sprite = sprite;
		}

		private void SetText(Dialog dialog)
		{
			_dialog.DOFade(0, .2f)
				.OnComplete(() =>
				{
					_dialog.text = dialog.Text;
					_dialog.DOFade(1, .2f)
					.OnComplete(() =>
					{
						_dialog.transform.DOPunchScale(0.01f * Vector3.one,
							.2f,
							1);
					});
				});
		}

		private Sprite GetSprite(Character key)
		{
			return _allCharacters.FirstOrDefault(x => x.Character == key).Sprite;
		}

		private Dialog GetDialogsFromAllConfigs(string key)
		{
			return _dialogConfigs.SelectMany(x => x.Dialogs).FirstOrDefault(x => x.Key == key);
		}
	}
}