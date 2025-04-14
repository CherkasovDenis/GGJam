using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
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
		public TMP_Text Number;
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
		[SerializeField]
		private CanvasGroup _restartHint;

		private Dictionary<Note, SongHint> _hints = new Dictionary<Note, SongHint>();
		private int _currentActiveNote;

		private int _lastNote = 0;

		private readonly UniTaskCompletionSource _noteWait = new UniTaskCompletionSource();

		private void Awake()
		{
			_audioTimelineService.NotePlay += NotePlayed;
			foreach (var note in _notes)
				note.OnNoteReleased += NoteOnOnNoteReleased;
		}

		private void NotePlayed(Note note)
		{
			if (_currentActiveNote == 0)
			{
				return;
			}

			_hints[note].MainObj.gameObject.SetActive(false);
			_currentActiveNote--;

			var i = Array.IndexOf(_notes, note);

			var correctNote = i == _lastNote;

			if (!correctNote)
			{
				_lastNote = 0;
				_hints.Clear();
				_currentActiveNote = 0;
				Blink();
			}

			if (correctNote)
				_lastNote++;

			if (_lastNote == _notes.Length)
			{
				_audioTimelineService.mainAudioSource.DOFade(0, 1f).OnComplete(() => _audioTimelineService.StopPlayback());
				_noteWait.TrySetResult();
			}
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
				note.SaveChecked();
				note.transform.DOScale(Vector3.zero, .5f)
					.SetEase(Ease.InBack)
					.OnComplete((() =>
					{
						var songHint = _songHints[_currentActiveNote - 1];
						songHint.MainObj.gameObject.SetActive(true);
						songHint.MainObj.alpha = 0;
						songHint.MainObj.DOFade(1, .5f);
						songHint.Text.text = note.Text;
						var i = Array.IndexOf(_notes, note);
						songHint.Number.text = (i + 1).ToString();
						_hints[note] = songHint;
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
			_currentActiveNote++;

			_audioTimelineService.EnqueueClip(note);
		}

		private void Blink()
		{
			_audioTimelineService.ResetAll();

			_restartHint.DOFade(1, 2).OnComplete(() => { _restartHint.DOFade(0, 2); });

			DOTween.Sequence()
				.Join(_eyeBlink[0].rectTransform.DOAnchorMin(new Vector2(0, .5f), .5f))
				.Join(_eyeBlink[1].rectTransform.DOAnchorMax(new Vector2(1, .5f), .5f))
				.AppendCallback(() =>
				{
					foreach (var note1 in _notes)
					{
						note1.ResetNote();
					}

					foreach (var songHint in _songHints)
					{
						songHint.MainObj.gameObject.SetActive(false);
					}
				})
				.Join(_eyeBlink[0].rectTransform.DOAnchorMin(new Vector2(0, 1), .5f))
				.Join(_eyeBlink[1].rectTransform.DOAnchorMax(new Vector2(1, 0), .5f))
				.Play();
		}
	}
}