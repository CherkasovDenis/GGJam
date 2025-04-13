using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GGJam.Childhood.SchoolGame.Scripts
{
	public class Number : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		public event System.Action<Number> OnReleased;
		public int Value;
		private Tween flightSequence;
		private bool isDragging = false;
		private Vector2 offset;
		private RectTransform rectTransform;

		private void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
			StartRandomFlight();
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			isDragging = true;
			StopFlight();
			transform.DOScale(Vector3.one * 1.1f, .3f).SetEase(Ease.OutBack);

			// Получаем локальные координаты внутри Canvas
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent as RectTransform,
				eventData.position,
				eventData.pressEventCamera,
				out var localPoint);

			offset = rectTransform.anchoredPosition - localPoint;
		}

		public void OnDrag(PointerEventData eventData)
		{
			// Конвертируем позицию мыши в локальные координаты Canvas
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent as RectTransform,
					eventData.position,
					eventData.pressEventCamera,
					out var localPoint))
				rectTransform.anchoredPosition = localPoint + offset;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			transform.DOScale(Vector3.one, .3f).SetEase(Ease.OutQuad);
			isDragging = false;
			StartRandomFlight();
			OnReleased?.Invoke(this);
		}

		public void StartRandomFlight()
		{
			if (isDragging)
				return;

			flightSequence?.Kill();
			flightSequence = RandomFlightTween().OnComplete(StartRandomFlight);
		}

		public void StopFlight()
		{
			flightSequence?.Kill();
		}


		private Tween RandomFlightTween()
		{
			// Получаем размеры родительского RectTransform (Canvas или его дочернего контейнера)
			var parentRect = transform.parent.GetComponent<RectTransform>();
			var xRange = parentRect.rect.width * 0.5f - rectTransform.rect.width * 0.5f;
			var yRange = parentRect.rect.height * 0.5f - rectTransform.rect.height * 0.5f;

			var randomPoint = new Vector2(Random.Range(-xRange, xRange), Random.Range(-yRange, yRange));

			return rectTransform.DOAnchorPos(randomPoint, 100).SetSpeedBased().SetEase(Ease.Linear);
		}

		public void Wrong()
		{
			StopFlight();
			rectTransform.DOPunchPosition(5f * Vector3.right, 1f).OnComplete(StartRandomFlight);
		}
	}
}