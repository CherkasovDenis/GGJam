using Cysharp.Threading.Tasks;
using DG.Tweening;
using GGJam.Childhood.OutDoor.Scripts;
using GGJam.Childhood.SchoolGame.Scripts;
using GGJam.Childhood.Scripts.Mother;
using GGJam.Dialogs.Scripts;
using GGJam.Scripts;
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
		private readonly OutDoorGame _outDoorGame;
		[Inject]
		private CanvasGroup _fader;
		[Inject]
		private readonly SceneService _sceneService;


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
			_outDoorGame.gameObject.SetActive(true);
			
			await FadeDown();

			await _outDoorGame.RunMiniGame();
			
			_sceneService.LoadNextScene();
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