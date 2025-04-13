using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GGJam.Menu.Scripts.Intro
{
    [Serializable]
    public class IntroSettings
    {
        public List<Image> Backgrounds => _backgrounds;

        public float BackgroundFadeDuration => _backgroundFadeDuration;

        public float BackgroundDuration => _backgroundDuration;

        public RectTransform TextRect => _textRect;

        public float TextMoveDuration => _textMoveDuration;

        public float TextEndPosition => _textEndPosition;

        [SerializeField]
        private List<Image> _backgrounds;

        [SerializeField]
        private float _backgroundFadeDuration;

        [SerializeField]
        private float _backgroundDuration;

        [SerializeField]
        private RectTransform _textRect;

        [SerializeField]
        private float _textMoveDuration;

        [SerializeField]
        private float _textEndPosition;
    }
}