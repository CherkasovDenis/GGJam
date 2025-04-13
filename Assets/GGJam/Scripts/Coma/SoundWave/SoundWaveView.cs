using UnityEngine;

namespace GGJam.Scripts.Coma.SoundWave
{
    public class SoundWaveView : MonoBehaviour
    {
        [SerializeField]
        private ParticleSystem _particleSystem;

        public void Play()
        {
            var particleSystemMain = _particleSystem.main;
            particleSystemMain.loop = true;
            _particleSystem.Play();
        }

        public void Stop()
        {
            _particleSystem.Stop();
        }
    }
}