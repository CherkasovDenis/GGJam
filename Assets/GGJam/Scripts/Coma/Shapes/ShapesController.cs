using System;
using System.Collections.Generic;
using System.Linq;
using GGJam.Scripts.Coma.PlayerSight;
using GGJam.Scripts.Coma.Settings;
using GGJam.Scripts.Coma.SoundWave;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace GGJam.Scripts.Coma.Shapes
{
    public class ShapesController : IInitializable, IDisposable, IStartable, ITickable
    {
        [Inject]
        private CameraSettings _cameraSettings;

        [Inject]
        private ComaChapterSettings _comaChapterSettings;

        [Inject]
        private Transform _cameraTransform;

        [Inject]
        private readonly ShapesModel _shapesModel;

        [Inject]
        private readonly PlayerSightView _playerSightView;

        [Inject]
        private readonly SoundWaveView _soundWaveViewPrefab;

        [Inject]
        private Camera _camera;

        public void Initialize()
        {
            foreach (var shapeView in _shapesModel.ShapeViews)
            {
                shapeView.Clicked += AddToInventory;
                shapeView.Combined += CheckCombine;
            }

            _shapesModel.OnNextChapter += PrepareNextChapter;
        }

        public void Dispose()
        {
            foreach (var shapeView in _shapesModel.ShapeViews)
            {
                shapeView.Clicked -= AddToInventory;
                shapeView.Combined -= CheckCombine;
            }

            _shapesModel.OnNextChapter -= PrepareNextChapter;
        }

        public void Start()
        {
            foreach (var shapeView in _shapesModel.ShapeViews)
            {
                shapeView.Disable();
            }

            _shapesModel.AllConnections = _comaChapterSettings.Chapters.SelectMany(x => x.Connects).ToList();

            _shapesModel.NextChapter();
        }

        public void Tick()
        {
            foreach (var shapeView in _shapesModel.CurrentShapeViews)
            {
                UpdateShapePosition(shapeView);
            }
        }

        private void PrepareNextChapter(ComaChapter chapter)
        {
            Debug.Log($"Prepare new chapter - {chapter.ComaChapterName}");

            _shapesModel.CurrentShapeViews.Clear();

            var idsInChapter = new List<string>();

            foreach (var shapeConnect in chapter.Connects)
            {
                // Не берем последний id - он результат соединения других
                for (var i = 0; i < shapeConnect.Connect.Count - 1; i++)
                {
                    idsInChapter.Add(shapeConnect.Connect[i]);
                }
            }

            foreach (var shapeView in _shapesModel.ShapeViews)
            {
                if (idsInChapter.Contains(shapeView.Id))
                {
                    EnableShapeView(shapeView);
                }
            }
        }

        private void CheckCombine(string firstId, string secondId)
        {
            var firstShapeView = _shapesModel.CurrentShapeViews.First(x => x.Id == firstId);
            var secondShapeView = _shapesModel.CurrentShapeViews.First(x => x.Id == secondId);

            foreach (var shapeConnect in _shapesModel.AllConnections)
            {
                var one = shapeConnect.Connect[0];
                var two = shapeConnect.Connect[1];
                var three = shapeConnect.Connect[2];

                if ((firstId == one && secondId == two) || (firstId == two && secondId == one))
                {
                    foreach (var shapeView in _shapesModel.ShapeViews)
                    {
                        if (three == shapeView.Id)
                        {
                            // TODO: Проиграть звук правильного соединения
                            EnableShapeView(shapeView);
                            break;
                        }
                    }

                    firstShapeView.Disable();
                    secondShapeView.Disable();
                    _shapesModel.CurrentShapeViews.Remove(firstShapeView);
                    _shapesModel.CurrentShapeViews.Remove(secondShapeView);

                    _shapesModel.AddConnect(shapeConnect.ToString());
                    _shapesModel.TryLaunchNextChapter();

                    return;
                }
            }

            AddToInventory(firstShapeView, true);
            AddToInventory(secondShapeView, true);
            _playerSightView.FreePoint(firstId);
            _playerSightView.FreePoint(secondId);
            // TODO: Проиграть звук неправильного соединения
        }

        private void EnableShapeView(ShapeView shapeView)
        {
            Debug.Log($"Enabling {shapeView.Id}");

            if (!shapeView.Initialized)
            {
                var shapePosition = shapeView.transform.position;
                var cameraPosition = _cameraTransform.position;
                var direction = (shapePosition - cameraPosition).normalized;
                var spawnPosition = cameraPosition + direction * _cameraSettings.SoundWaveParticleDistance;
                var rotation = Quaternion.LookRotation(direction);

                shapeView.Initialize(Object.Instantiate(_soundWaveViewPrefab, spawnPosition, rotation));
            }

            shapeView.Enable();
            _shapesModel.CurrentShapeViews.Add(shapeView);
        }

        private void UpdateShapePosition(ShapeView shapeView)
        {
            var distanceFromCamera = _cameraSettings.SoundWaveParticleDistance;

            // Получаем позиции
            var shapePos = shapeView.transform.position;

            // Конвертируем позицию shape в координаты viewport (x,y в [0;1] если объект виден)
            var viewportPos = _camera.WorldToViewportPoint(shapePos);

            // Проверяем, виден ли объект:
            // (z > 0 означает, что объект перед камерой, а x,y в [0;1] – внутри экрана)
            var shapeVisible = (viewportPos.z > 0 &&
                                viewportPos.x >= 0f && viewportPos.x <= 1f &&
                                viewportPos.y >= 0f && viewportPos.y <= 1f);

            Vector3 spawnWorldPos;

            if (shapeVisible)
            {
                // Если объект виден, вычисляем точку на луче от камеры к shape
                var cameraPosition = _cameraTransform.position;
                var direction = (shapePos - cameraPosition).normalized;
                spawnWorldPos = cameraPosition + direction * distanceFromCamera;
            }
            else
            {
                // Если объект не виден, обработаем случай, когда он может находиться за камерой:
                if (viewportPos.z < 0f)
                {
                    viewportPos.x = 1f - viewportPos.x;
                    viewportPos.y = 1f - viewportPos.y;
                }

                viewportPos.x = Mathf.Clamp(viewportPos.x, 0f, 1f);
                viewportPos.y = Mathf.Clamp(viewportPos.y, 0f, 1f);
                viewportPos.z = distanceFromCamera; // задаём нужную глубину (расстояние от камеры)

                // Переводим обратно в мировые координаты
                spawnWorldPos = _camera.ViewportToWorldPoint(viewportPos);
            }

            // Обновляем позицию объекта партиклов:
            shapeView.SoundWaveView.transform.position = spawnWorldPos;

            var lookDirection = (_cameraTransform.position - spawnWorldPos).normalized;
            shapeView.SoundWaveView.transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        private void AddToInventory(ShapeView shapeView) => AddToInventory(shapeView, false);

        private void AddToInventory(ShapeView shapeView, bool wrongDrop)
        {
            shapeView.transform.SetParent(_playerSightView.ShapesParent);

            shapeView.AddToInventory(_playerSightView.GetEmptyPoint(shapeView.Id), wrongDrop);
        }
    }
}