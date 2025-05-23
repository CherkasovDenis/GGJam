﻿using DG.Tweening;
using GGJam.Childhood.Scripts.Mother;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Note : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
	public event System.Action<Note> OnNoteReleased;
	public Button Button;
	public AudioClip AudioClip;
	public int ClipOffset;
	public string Text;

	private Tween flightSequence;
	private bool isDragging = false;
	private Vector2 offset;
	private RectTransform rectTransform;
	[SerializeField]
	private SongHint _songHint;
	private bool saved;
	private Tween tween;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		StartRandomFlight();
		_songHint.MainObj.alpha = 0;
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
		OnNoteReleased?.Invoke(this);
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

	public void ResetNote()
	{
		Button.image.DOFade(1, 0f);
		transform.localScale = Vector3.one;
		StartRandomFlight();
		if (saved)
		{
			_songHint.MainObj.alpha = 1;
			tween = _songHint.MainObj.DOFade(0, 5f);
		}
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

	public void SaveChecked()
	{
		saved = true;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		tween?.Kill();
		if (saved)
			tween = _songHint.MainObj.DOFade(1, .5f);
	}
	public void OnPointerExit(PointerEventData eventData)
	{
		tween?.Kill();
		if (saved)
			tween = _songHint.MainObj.DOFade(0, 3f);
	}
}