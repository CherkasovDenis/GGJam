using GGJam.Scripts.Coma.Settings;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GGJam.Scripts.Coma.CameraMovement
{
    public class ControlPointController : IStartable, ITickable
    {
        [Inject]
        private readonly CameraSettings _cameraSettings;

        [Inject]
        private readonly Transform _cameraTransform;

        [Inject]
        private readonly ControlPointView _controlPointView;

        private Vector3 _desiredPosition;
        private Vector3 _velocity;
        private Vector2 _lastMousePos;

        public void Start()
        {
            _desiredPosition = _cameraTransform.position + _cameraTransform.forward * _cameraSettings.PlayerSightOffset;
            _controlPointView.transform.position = _desiredPosition;

            _lastMousePos = Input.mousePosition;
        }

        public void Tick()
        {
            // Получаем текущую позицию мыши.
            Vector2 mousePos = Input.mousePosition;

            // Рассчитываем скорость мыши в пикселях в секунду.
            var mouseDelta = mousePos - _lastMousePos;
            var mouseSpeed = mouseDelta.magnitude / Time.deltaTime;
            _lastMousePos = mousePos;

            // Вычисляем смещение курсора относительно центра экрана в нормализованных координатах (от -1 до 1).
            var screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            var normOffset = new Vector2(
                (mousePos.x - screenCenter.x) / screenCenter.x,
                (mousePos.y - screenCenter.y) / screenCenter.y
            );

            // Берем максимальный модуль компоненты смещения.
            var maxAbs = Mathf.Max(Mathf.Abs(normOffset.x), Mathf.Abs(normOffset.y));

            var totalMultiplier = 0f;
            // Если курсор внутри центральной зоны – малый эффект (легкое покачивание).
            if (maxAbs <= 1f - _cameraSettings.FrameMargin)
            {
                totalMultiplier = _cameraSettings.MinEffect;
            }
            else
            {
                // Если курсор вне зоны, базовый эффект равен maxEffect плюс дополнительный вклад от скорости.
                var extraMultiplier = _cameraSettings.MaxExtraEffect *
                                      Mathf.Clamp01(mouseSpeed / _cameraSettings.SpeedThreshold);
                totalMultiplier = _cameraSettings.MaxEffect + extraMultiplier;
            }

            // Итоговое нормализованное смещение.
            var finalNormalized = normOffset * totalMultiplier;

            // Вычисляем смещение в мировом пространстве, используя локальные оси камеры.
            var offset = _cameraTransform.right * finalNormalized.x + _cameraTransform.up * finalNormalized.y;
            offset = Vector3.ClampMagnitude(offset, _cameraSettings.MaxOffset);

            // Формируем новое направление: базовое направление (camera.forward) плюс вычисленное смещение.
            // Нормализуя сумму, получаем точку на сфере с радиусом targetDistance.
            var desiredDirection = (_cameraTransform.forward + offset).normalized;
            _desiredPosition = _cameraTransform.position + desiredDirection * _cameraSettings.PlayerSightOffset;

            // 6. Ограничиваем полученную позицию по осям согласно указанным диапазонам.
            var clampedX = Mathf.Clamp(_desiredPosition.x, _cameraSettings.ControlPointClampX.x,
                _cameraSettings.ControlPointClampX.y);
            var clampedY = Mathf.Clamp(_desiredPosition.y, _cameraSettings.ControlPointClampY.x,
                _cameraSettings.ControlPointClampY.y);
            var clampedZ = Mathf.Clamp(_desiredPosition.z, _cameraSettings.ControlPointClampZ.x,
                _cameraSettings.ControlPointClampZ.y);
            _desiredPosition = new Vector3(clampedX, clampedY, clampedZ);

            // Плавно перемещаем контрольную точку к новой позиции с учетом инерции.
            _controlPointView.transform.position = Vector3.SmoothDamp(_controlPointView.transform.position,
                _desiredPosition, ref _velocity, _cameraSettings.SmoothTime);
        }
    }
}