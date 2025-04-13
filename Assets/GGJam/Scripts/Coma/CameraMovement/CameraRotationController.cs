using GGJam.Scripts.Coma.Settings;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GGJam.Scripts.Coma.CameraMovement
{
    public class CameraRotationController : ITickable
    {
        [Inject]
        private readonly CameraSettings _cameraSettings;

        [Inject]
        private readonly Transform _cameraTransform;

        [Inject]
        private readonly ControlPointView _controlPointView;

        public void Tick()
        {
            var desiredRotation = Quaternion.LookRotation(
                _controlPointView.transform.position - _cameraTransform.position);
            _cameraTransform.rotation = Quaternion.Slerp(_cameraTransform.rotation, desiredRotation,
                Time.deltaTime * _cameraSettings.RotationSmoothSpeed);
        }
    }
}