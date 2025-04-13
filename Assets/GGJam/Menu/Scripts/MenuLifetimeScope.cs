using GGJam.Menu.Scripts.Intro;
using GGJam.Menu.Scripts.PlayButton;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GGJam.Menu.Scripts
{
    public class MenuLifetimeScope : LifetimeScope
    {
        [SerializeField]
        private IntroSettings _introSettings;

        [SerializeField]
        private PlayButtonView _playButtonView;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_introSettings);

            builder.RegisterInstance(_playButtonView);

            builder.RegisterEntryPoint<IntroController>(Lifetime.Scoped);
        }
    }
}