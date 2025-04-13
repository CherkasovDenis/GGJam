using System;
using System.Collections.Generic;
using UnityEngine;

namespace GGJam.Scripts.Coma.Settings
{
    [CreateAssetMenu(fileName = nameof(ComaChapterSettings), menuName = nameof(ComaChapterSettings))]
    public class ComaChapterSettings : ScriptableObject
    {
        public List<ComaChapter> Chapters => _chapters;

        [SerializeField]
        private List<ComaChapter> _chapters;
    }
}