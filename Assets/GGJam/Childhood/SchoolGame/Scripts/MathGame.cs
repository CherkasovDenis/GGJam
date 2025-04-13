using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GGJam.Childhood.SchoolGame.Scripts
{
	public class MathGame : MonoBehaviour
	{
		[SerializeField]
		private Image _girl;
		[SerializeField]
		private Sprite[] _girlSprites;
		[SerializeField]
		private TMP_Text _girlText;
		[SerializeField]
		private string[] _girlSpeech;
		[SerializeField]
		private Number[] _numbers;
		[SerializeField]
		private RectTransform placeholderRect;
		[SerializeField]
		private ParticleSystem _finaleParticles;

		[Header("Problem Settings")]
		[SerializeField]
		private List<CanvasGroup> mathProblems;
		[SerializeField]
		private List<Transform> mathPositions;
		[SerializeField]
		private List<int> answers;

		private int currentProblemIndex = 0;
		private UniTaskCompletionSource _mainTask = new UniTaskCompletionSource();

		private void Start()
		{
			foreach (var note in _numbers)
				note.OnReleased += ReleaseNumber;
		}
		
		public async UniTask RunMiniGame()
		{
			await _mainTask.Task;
		}
		
		public async UniTaskVoid ShowNextProblem()
		{
			var mathProblem = mathProblems[currentProblemIndex];
			await mathProblem.DOFade(0, .5f);
			mathProblem.gameObject.SetActive(false);

			for (var i = 0; i < _numbers.Length; i++)
			{
				var number = _numbers[i];
				number.transform.DOScale(Vector3.zero, .5f).SetDelay(0.01f * i);
			}

			_girl.sprite = _girlSprites[1];
			_girlText.text = _girlSpeech[currentProblemIndex];

			await _girlText.DOFade(1, 1f);

			await UniTask.WaitForSeconds(1);

			await _girlText.DOFade(0, 2f);


			currentProblemIndex++;

			if (currentProblemIndex >= mathProblems.Count)
			{
				currentProblemIndex = 0;
				Debug.Log("Все примеры решены!");
				_finaleParticles.Play();
				await UniTask.WaitForSeconds(2f);
				_mainTask.TrySetResult();
				return;
			}

			_girl.sprite = _girlSprites[0];
			
			
			var nextProblem = mathProblems[currentProblemIndex];
			nextProblem.gameObject.SetActive(true);
			nextProblem.alpha = 0;
			await nextProblem.DOFade(1, .5f);

			for (var i = 0; i < _numbers.Length; i++)
			{
				var number = _numbers[i];
				number.transform.DOScale(Vector3.one, .5f).SetDelay(0.01f * i).SetEase(Ease.OutBack);
			}

			mathProblem.gameObject.SetActive(false);
		}

		public bool CheckAnswer(int value)
		{
			// Проверяем, правильный ли ответ
			var isCorrect = value == answers[currentProblemIndex];

			return isCorrect;
		}

		public Vector2 GetPlaceholderPosition()
		{
			// Возвращаем мировую позицию placeholder'а
			return placeholderRect.position;
		}

		// Метод для получения размера placeholder'а
		public Vector2 GetPlaceholderSize()
		{
			return placeholderRect.sizeDelta;
		}

		private void ReleaseNumber(Number number)
		{
			var placeholderPos = GetPlaceholderPosition();
			var placeholderSize = GetPlaceholderSize();

			// Создаем прямоугольник для placeholder'а
			var placeholderRect = new Rect(placeholderPos - placeholderSize / 2, placeholderSize);

			// Проверяем, попадает ли текущая позиция внутрь placeholder'а
			if (placeholderRect.Contains(number.transform.position))
			{
				// Если ответ верный

				number.StopFlight();
				if (CheckAnswer(number.Value))
				{
					// Перемещаем цифру на место placeholder'а
					var mathPosition = mathPositions[currentProblemIndex];
					number.transform.SetParent(mathPosition);
					number.transform.DOLocalMove(Vector3.zero, .4f)
						.OnComplete(() =>
						{
							number.transform.DOPunchScale(.1f * Vector3.one, .2f);

							ShowNextProblem();
						});
				}
				else
				{
					number.Wrong();
				}
			}
			else
			{
				// Возвращаем цифру на исходную позицию
			}
		}
	}
}