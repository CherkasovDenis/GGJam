using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GGJam.Childhood.Scripts.Mother
{
	[Serializable]
	public class SongHint
	{
		public CanvasGroup MainObj;
		public TMP_Text Text;
	}

	public class MotherSong : MonoBehaviour
	{
		private const int MaxNote = 2;
		[SerializeField]
		private Note[] _notes;
		[SerializeField]
		private Image[] _eyeBlink;
		[SerializeField]
		private AudioTimelineService _audioTimelineService;
		[SerializeField]
		private RectTransform _motherMouth;
		[SerializeField]
		private SongHint[] _songHints;

		private int _clickedNoteCount;
		private int _currentActiveNote;

		private int _lastNote = 0;

		private readonly UniTaskCompletionSource _noteWait = new UniTaskCompletionSource();

		private void Awake()
		{
			foreach (var note in _notes)
				note.OnNoteReleased += NoteOnOnNoteReleased;
		}

		private void OnDestroy()
		{
			foreach (var note in _notes)
				note.OnNoteReleased -= NoteOnOnNoteReleased;
		}

		public async UniTask RunMiniGame()
		{
			await _noteWait.Task;
		}

		private void NoteOnOnNoteReleased(Note note)
		{
			var motherMouthRect = _motherMouth.rect;
			Vector2 localPoint = _motherMouth.InverseTransformPoint(note.GetComponent<RectTransform>().position);
			if (motherMouthRect.Contains(localPoint))
			{
				if (_currentActiveNote >= MaxNote)
					return;

				note.StopFlight();
				note.transform.DOScale(Vector3.zero, .5f)
					.SetEase(Ease.InBack)
					.OnComplete((() =>
					{
						var songHint = _songHints[_currentActiveNote - 1];
						songHint.MainObj.gameObject.SetActive(true);
						songHint.MainObj.alpha = 0;
						songHint.MainObj.DOFade(1, .5f);
						songHint.Text.text = note.Text;
					}));

				note.transform.DOJump(_motherMouth.position,
					1,
					1,
					.5f);

				ClickedNote(note);
			}
		}

		private void ClickedNote(Note note)
		{
			_clickedNoteCount++;
			_currentActiveNote++;

			var i = Array.IndexOf(_notes, note);

			var correctNote = i == _lastNote;
			_audioTimelineService.EnqueueClip(note.AudioClip, note.ClipOffset);

			if (correctNote)
				_lastNote++;

			if (_lastNote == _notes.Length)
				_noteWait.TrySetResult();

			if (_clickedNoteCount >= _notes.Length)
			{
				_clickedNoteCount = 0;
				_lastNote = 0;
				Blink();
			}
		}

		private void Blink()
		{
			var seq = DOTween.Sequence();
			foreach (var note1 in _notes)
				seq.Join(note1.ResetNote());

			DOTween.Sequence()
				.Join(_eyeBlink[0].rectTransform.DOAnchorMin(new Vector2(0, .5f), .5f))
				.Join(_eyeBlink[1].rectTransform.DOAnchorMax(new Vector2(1, .5f), .5f))
				.Append(seq)
				.Join(_eyeBlink[0].rectTransform.DOAnchorMin(new Vector2(0, 0), .5f))
				.Join(_eyeBlink[1].rectTransform.DOAnchorMax(new Vector2(1, 0), .5f));
		}
	}
}