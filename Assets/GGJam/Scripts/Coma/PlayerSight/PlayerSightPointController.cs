using GGJam.Scripts.Coma.Settings;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GGJam.Scripts.Coma.PlayerSight
{
    public class PlayerSightPointController : ITickable
    {
        [Inject]
        private readonly CameraSettings _cameraSettings;

        [Inject]
        private readonly PlayerSightView _playerSightView;

        [Inject]
        private readonly Transform _cameraTransform;

        public void Tick()
        {
            var cameraForward = _cameraTransform.forward;
            _playerSightView.transform.position = _cameraTransform.position +
                                                  cameraForward * _cameraSettings.PlayerSightOffset;

            var rotation = Quaternion.LookRotation(cameraForward);
            rotation.z = _cameraTransform.rotation.z;

            _playerSightView.transform.rotation = rotation;
        }
    }
}