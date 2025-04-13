using System;

namespace GGJam.Scripts.Coma
{
    public class ComaModel
    {
        public event Action ComaFinal;

        public void OnComaFinal()
        {
            ComaFinal?.Invoke();
        }
    }
}