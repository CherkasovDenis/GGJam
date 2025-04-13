using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GGJam.Menu.Scripts.PlayButton;
using GGJam.Scripts;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace GGJam.Menu.Scripts.Intro
{
    public class IntroController : IInitializable, IDisposable
    {
        [Inject]
        private IntroSettings _introSettings;

        [Inject]
        private PlayButtonView _playButtonView;

        [Inject]
        private SceneService _sceneService;

        private CancellationTokenSource _ctr;

        public void Initialize()
        {
            _playButtonView.Clicked += StartIntro;
        }

        public void Dispose()
        {
            _playButtonView.Clicked -= StartIntro;

            if (_ctr != null)
            {
                _ctr.Cancel();
                _ctr.Dispose();
            }
        }

        private void StartIntro()
        {
            _ctr = new CancellationTokenSource();
            StartIntroAsync(_ctr.Token).Forget();
        }

        private async UniTask StartIntroAsync(CancellationToken cancellationToken)
        {
            // Включается музыка

            _introSettings.TextRect.DOAnchorPosY(_introSettings.TextEndPosition, _introSettings.TextMoveDuration)
                .SetEase(Ease.Linear)
                .ToUniTask(cancellationToken: cancellationToken);

            // Появился и поплыл текст

            await UniTask.WaitForSeconds(_introSettings.BackgroundDuration, cancellationToken: cancellationToken);

            await FadeBackground(_introSettings.Backgrounds[0], cancellationToken);

            await UniTask.WaitForSeconds(_introSettings.BackgroundDuration, cancellationToken: cancellationToken);

            await FadeBackground(_introSettings.Backgrounds[1], cancellationToken);

            await UniTask.WaitForSeconds(_introSettings.BackgroundDuration, cancellationToken: cancellationToken);

            // Скрылся текст

            _sceneService.LoadNextScene();
        }

        private async UniTask FadeBackground(Image background, CancellationToken cancellationToken)
        {
            await background.DOFade(1f, _introSettings.BackgroundFadeDuration)
                .ToUniTask(cancellationToken: cancellationToken);
        }
    }
}