using UnityEngine;

namespace GGJam.Scripts.Coma.Shapes
{
    public class ShapesSoundService : MonoBehaviour
    {
        [SerializeField]
        private AudioClip _correctSound;

        [SerializeField]
        private AudioSource _audioSource;

        public void PlayCorrectSound()
        {
            _audioSource.PlayOneShot(_correctSound);
        }
    }
}