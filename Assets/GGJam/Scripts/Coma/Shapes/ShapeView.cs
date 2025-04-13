using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GGJam.Scripts.Coma.Settings;
using GGJam.Scripts.Coma.SoundWave;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;
using Random = UnityEngine.Random;

namespace GGJam.Scripts.Coma.Shapes
{
    public class ShapeView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public event Action<ShapeView> Clicked;

        public event Action<string, string> Combined;

        public string Id => _id;

        public bool Initialized { get; private set; }

        public bool InInventory { get; private set; }

        public SoundWaveView SoundWaveView { get; private set; }

        [Inject]
        private Camera _camera;

        [Inject]
        private CameraSettings _cameraSettings;

        [SerializeField]
        private string _id;

        [SerializeField]
        private AudioSource _audioSource;

        [SerializeField]
        private MeshRenderer _mesh;

        [SerializeField]
        private SpriteRenderer _sprite;

        private bool _isDragging;
        private bool _addingToInventory;
        private Vector3 _offset;
        private float _fixedZ;
        private Vector3 _inventoryPoint;
        private bool _playingSound;
        private Tween _flightSequence;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_addingToInventory || _isDragging)
            {
                return;
            }

            if (InInventory)
            {
                PlaySoundInInventoryAsync().Forget();
            }
            else
            {
                Clicked?.Invoke(this);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!InInventory)
            {
                return;
            }

            StopFlight();
            _isDragging = true;

            var position = transform.position;
            _fixedZ = _camera.WorldToScreenPoint(position).z;

            var worldMouse =
                _camera.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, _fixedZ));
            _offset = position - worldMouse;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!InInventory)
            {
                return;
            }

            var worldMouse = _camera.ScreenToWorldPoint(
                new Vector3(eventData.position.x, eventData.position.y, _fixedZ));
            var worldTarget = worldMouse + _offset;

            var localTarget = transform.parent.InverseTransformPoint(worldTarget);
            localTarget.z = 0f;
            transform.localPosition = localTarget;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!InInventory)
            {
                return;
            }

            var hitColliders = Physics.OverlapSphere(transform.position, 3f);

            if (hitColliders.Length > 1)
            {
                if (hitColliders[0].transform.parent.TryGetComponent(out ShapeView firstShapeView) &&
                    hitColliders[1].transform.parent.TryGetComponent(out ShapeView secondShapeView))
                {
                    if (firstShapeView.InInventory && secondShapeView.InInventory)
                    {
                        Combined?.Invoke(firstShapeView.Id, secondShapeView.Id);
                        _isDragging = false;
                        StopFlight();
                        return;
                    }
                }
            }

            _isDragging = false;
            transform.DOLocalMove(_inventoryPoint, 1f).OnComplete(StartRandomFlight);
        }

        public void Enable()
        {
            if (InInventory)
            {
                return;
            }

            gameObject.SetActive(true);
            _audioSource.loop = true;
            _audioSource.Play();

            if (SoundWaveView != null)
            {
                SoundWaveView.Play();
            }
        }

        public void Disable()
        {
            _audioSource.Stop();

            if (SoundWaveView != null)
            {
                SoundWaveView.Stop();
            }

            gameObject.SetActive(false);
        }

        public void Initialize(SoundWaveView soundWaveView)
        {
            SoundWaveView = soundWaveView;
            Initialized = true;
        }

        public void AddToInventory(Vector3 point, bool wrongDrop)
        {
            if (InInventory && !wrongDrop)
            {
                return;
            }

            _inventoryPoint = point;

            var duration = 1f;

            _addingToInventory = true;

            var sequence = DOTween.Sequence();

            sequence
                .Append(transform.DOLocalMove(point, duration))
                .Join(transform.DOLocalRotate(Vector3.zero, duration));

            if (!InInventory)
            {
                sequence.Append(_sprite.DOFade(1f, duration));
            }

            if (wrongDrop)
            {
                sequence
                    .Append(transform.DOShakeScale(duration));
            }

            sequence.OnComplete(() =>
            {
                _addingToInventory = false;
                InInventory = true;
                StartRandomFlight();

                if (_sprite != null)
                {
                    _mesh.enabled = false;
                }
            });

            sequence.Play();

            PlaySoundInInventoryAsync().Forget();
        }

        private async UniTask PlaySoundInInventoryAsync()
        {
            if (_playingSound)
            {
                return;
            }

            _playingSound = true;

            _audioSource.Stop();
            _audioSource.spatialBlend = 0f;
            _audioSource.Play();

            SoundWaveView.Play();

            await UniTask.Delay(TimeSpan.FromSeconds(6f),
                cancellationToken: destroyCancellationToken);

            SoundWaveView.Stop();
            _audioSource.Stop();
            _playingSound = false;
        }

        private void StartRandomFlight()
        {
            if (_isDragging)
            {
                return;
            }

            _flightSequence?.Kill();
            _flightSequence = RandomFlightTween().OnComplete(StartRandomFlight);
        }

        private void StopFlight()
        {
            _flightSequence?.Kill();
        }

        private Tween RandomFlightTween()
        {
            var randomPoint = new Vector3(
                Random.Range(_cameraSettings.ShapesClampX.x, _cameraSettings.ShapesClampX.y),
                Random.Range(_cameraSettings.ShapesClampY.x, _cameraSettings.ShapesClampY.y),
                0f);

            return transform.DOLocalMove(randomPoint, 3f)
                .SetSpeedBased()
                .SetEase(Ease.Linear);
        }
    }
}