using System;
using DG.Tweening;
using TextTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GGJam.Menu.Scripts.PlayButton
{
    public class PlayButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action Clicked;

        [SerializeField]
        private RectTransform _rect;

        [SerializeField]
        private Button _button;

        [SerializeField]
        private TweenManager _tweenManager;

        private Tween _progressTween;

        private bool _clicked;

        private void Start()
        {
            _button.onClick.AddListener(OnButtonClick);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_clicked)
            {
                return;
            }

            _progressTween?.Kill();
            _progressTween = DOTween.To(SetProgress, _tweenManager.Progress, 1f, 1f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_clicked)
            {
                return;
            }

            _progressTween?.Kill();
            _progressTween = DOTween.To(SetProgress, _tweenManager.Progress, 0f, 1f);
        }

        private void SetProgress(float value)
        {
            _tweenManager.Progress = value;
        }

        private void OnButtonClick()
        {
            if (_clicked)
            {
                return;
            }

            _clicked = true;

            var sequence = DOTween.Sequence();

            sequence
                .Append(_rect.DOAnchorPosY(-100, 0.3f).SetEase(Ease.InOutSine))
                .Append(_rect.DOAnchorPosY(3000, 0.5f).SetEase(Ease.Linear))
                .SetEase(Ease.OutSine)
                .OnComplete(() => Clicked?.Invoke());

            sequence.Play();
        }
    }
}