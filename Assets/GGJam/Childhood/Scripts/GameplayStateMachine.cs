using Cysharp.Threading.Tasks;
using DG.Tweening;
using GGJam.Childhood.SchoolGame.Scripts;
using GGJam.Childhood.Scripts.Mother;
using GGJam.Dialogs.Scripts;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GGJam.ChildHood.Scripts
{
	public class GameplayStateMachine : IAsyncStartable
	{
		[Inject]
		private readonly DialogService _dialogService;
		[Inject]
		private readonly MotherSong _motherSong;
		[Inject]
		private readonly MathGame _mathGame;
		[Inject]
		private CanvasGroup _fader;
		


		public async UniTask StartAsync(CancellationToken cancellation = new CancellationToken())
		{
			_motherSong.gameObject.SetActive(true);
			await FadeDown();
			await _motherSong.RunMiniGame();
			
			await FadeIn();
			_motherSong.gameObject.SetActive(false);
			_mathGame.gameObject.SetActive(true);
			await FadeDown();
			
			await _mathGame.RunMiniGame();
			await FadeIn();
			_mathGame.gameObject.SetActive(false);
			
			await FadeDown();
			
			
			// await _dialogService.ShowDialog("loh_1");

			// await _dialogService.ShowDialog("pidr_2");

			// _dialogService.HideWindow();
		}

		private async UniTask FadeIn()
		{
			_fader.gameObject.SetActive(true);
			await _fader.DOFade(1, .5f);
		}

		private async UniTask FadeDown()
		{
			await _fader.DOFade(0, .5f);
			_fader.gameObject.SetActive(false);
		}
	}
}