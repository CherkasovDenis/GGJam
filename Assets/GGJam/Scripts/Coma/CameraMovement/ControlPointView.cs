using UnityEngine;

namespace GGJam.Scripts.Coma.CameraMovement
{
    public class ControlPointView : MonoBehaviour
    {
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
#endif
    }
}