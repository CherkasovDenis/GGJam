using System;
using UnityEngine;

namespace GGJam.Scripts.Coma.Settings
{
    [Serializable]
    public class CameraSettings
    {
        public float PlayerSightOffset => _playerSightOffset;

        public float FrameMargin => _frameMargin;

        public float MaxOffset => _maxOffset;

        public float SpeedThreshold => _speedThreshold;

        public float MaxExtraEffect => _maxExtraEffect;

        public float MinEffect => _minEffect;

        public float MaxEffect => _maxEffect;

        public float SmoothTime => _smoothTime;

        public float RotationSmoothSpeed => _rotationSmoothSpeed;

        public Vector2 ControlPointClampX => _controlPointClampX;

        public Vector2 ControlPointClampY => _controlPointClampY;

        public Vector2 ControlPointClampZ => _controlPointClampZ;

        public float SoundWaveParticleDistance => _soundWaveParticleDistance;

        public Vector2 ShapesClampX => _shapesClampX;

        public Vector2 ShapesClampY => _shapesClampY;

        [SerializeField]
        private float _playerSightOffset;

        [SerializeField]
        private float _frameMargin;

        [SerializeField]
        private float _maxOffset;

        [SerializeField]
        private float _speedThreshold = 1000f;

        [SerializeField]
        private float _maxExtraEffect = 100f;

        [SerializeField]
        private float _minEffect = 0.3f;

        [SerializeField]
        private float _maxEffect = 1f;

        [SerializeField]
        private float _smoothTime = 0.2f;

        [SerializeField]
        private float _rotationSmoothSpeed = 0.5f;

        [SerializeField]
        private Vector2 _controlPointClampX = new(-50f, 50f);

        [SerializeField]
        private Vector2 _controlPointClampY = new(-20f, 50f);

        [SerializeField]
        private Vector2 _controlPointClampZ = new(0f, 50f);

        [SerializeField]
        private float _soundWaveParticleDistance = 10f;

        [SerializeField]
        private Vector2 _shapesClampX = new(-40f, 40f);

        [SerializeField]
        private Vector2 _shapesClampY = new(-20f, 20f);
    }
}