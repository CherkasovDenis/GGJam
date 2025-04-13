using System.Collections.Generic;
using GGJam.Scripts.Coma.CameraMovement;
using GGJam.Scripts.Coma.PlayerSight;
using GGJam.Scripts.Coma.Settings;
using GGJam.Scripts.Coma.Shapes;
using GGJam.Scripts.Coma.SoundWave;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GGJam.Scripts.Coma
{
    public class ComaLifetimeScope : LifetimeScope
    {
        [SerializeField]
        private CameraSettings _cameraSettings;

        [SerializeField]
        private ComaChapterSettings _comaChapterSettings;

        [SerializeField]
        private SoundWaveView _soundWaveViewPrefab;

        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private Transform _cameraTransform;

        [SerializeField]
        private ControlPointView _controlPointView;

        [SerializeField]
        private PlayerSightView _playerSightView;

        [SerializeField]
        private List<ShapeView> _shapeViews;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_cameraSettings);
            builder.RegisterInstance(_comaChapterSettings);

            builder.RegisterInstance(_soundWaveViewPrefab);

            builder.RegisterInstance(_camera);
            builder.RegisterInstance(_controlPointView);
            builder.RegisterInstance(_playerSightView);
            builder.RegisterInstance(_shapeViews);

            builder.RegisterEntryPoint<ControlPointController>(Lifetime.Scoped).WithParameter(_cameraTransform);
            builder.RegisterEntryPoint<CameraRotationController>(Lifetime.Scoped).WithParameter(_cameraTransform);
            builder.RegisterEntryPoint<PlayerSightPointController>(Lifetime.Scoped).WithParameter(_cameraTransform);

            builder.Register<ShapesModel>(Lifetime.Scoped);
            builder.RegisterEntryPoint<ShapesController>(Lifetime.Scoped).WithParameter(_cameraTransform);
        }
    }
}