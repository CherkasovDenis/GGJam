using System.Collections.Generic;
using UnityEngine;

namespace GGJam.Scripts.Coma.PlayerSight
{
    public class PlayerSightView : MonoBehaviour
    {
        public Transform ShapesParent => _shapesParent;

        [SerializeField]
        private Transform _shapesParent;

        [SerializeField]
        private List<Transform> _shapePoints;

        private readonly List<string> _pointStatuses = new();

        private void Start()
        {
            foreach (var _ in _shapePoints)
            {
                _pointStatuses.Add(string.Empty);
            }
        }

        public Vector3 GetEmptyPoint(string shapeId)
        {
            for (var i = 0; i < _shapePoints.Count; i++)
            {
                if (_pointStatuses[i] == string.Empty)
                {
                    _pointStatuses[i] = shapeId;
                    return _shapePoints[i].localPosition;
                }
            }

            return Vector3.zero;
        }

        public void FreePoint(string shapeId)
        {
            for (var i = 0; i < _pointStatuses.Count; i++)
            {
                if (_pointStatuses[i] == shapeId)
                {
                    _pointStatuses[i] = string.Empty;
                    return;
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
#endif
    }
}