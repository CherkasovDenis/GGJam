using System;
using System.Collections.Generic;
using System.Linq;
using GGJam.Scripts.Coma.Settings;
using UnityEngine;
using VContainer;

namespace GGJam.Scripts.Coma.Shapes
{
    public class ShapesModel
    {
        public event Action<ComaChapter> OnNextChapter;

        public event Action OnFinal;

        public List<ShapeView> CurrentShapeViews { get; } = new();

        public List<ShapeConnect> AllConnections { get; set; }

        public List<ShapeView> ShapeViews => _shapeViews;

        [Inject]
        private ComaChapterSettings _comaChapterSettings;

        [Inject]
        private readonly List<ShapeView> _shapeViews;

        private List<string> _connects = new();

        private int _currenChapterIndex = -1;

        public void TryLaunchNextChapter()
        {
            Debug.Log("TryLaunchNextChapter");

            var nextChapterIndex = _currenChapterIndex + 1;

            if (nextChapterIndex >= _comaChapterSettings.Chapters.Count)
            {
                OnFinal?.Invoke();
                return;
            }

            var currentChapter = _comaChapterSettings.Chapters[_currenChapterIndex];

            var connectsInChapter = currentChapter.Connects.Select(x => x.ToString()).ToList();

            foreach (var connect in connectsInChapter)
            {
                // Если хоть один не содержится - то return
                if (!_connects.Contains(connect))
                {
                    return;
                }
            }

            NextChapter();
        }

        public void NextChapter()
        {
            _currenChapterIndex++;

            OnNextChapter?.Invoke(_comaChapterSettings.Chapters[_currenChapterIndex]);
        }

        public void AddConnect(string connect)
        {
            _connects.Add(connect);
        }
    }
}