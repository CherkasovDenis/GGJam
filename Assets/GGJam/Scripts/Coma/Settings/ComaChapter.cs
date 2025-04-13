using System;
using System.Collections.Generic;
using UnityEngine;

namespace GGJam.Scripts.Coma.Settings
{
    [CreateAssetMenu(fileName = nameof(ComaChapter), menuName = nameof(ComaChapter))]
    public class ComaChapter : ScriptableObject
    {
        public string ComaChapterName => _comaChapterName;

        public List<ShapeConnect> Connects => _connects;

        [SerializeField]
        private string _comaChapterName;

        [SerializeField]
        private List<ShapeConnect> _connects;
    }

    [Serializable]
    public class ShapeConnect
    {
        public List<string> Connect => _connect;

        [SerializeField, Tooltip("Первые две строки - id, которые должны соединиться, третья - результат соедининения")]
        private List<string> _connect;

        public override string ToString()
        {
            return $"{_connect[0]}-{_connect[1]}-{_connect[2]}";
        }
    }
}