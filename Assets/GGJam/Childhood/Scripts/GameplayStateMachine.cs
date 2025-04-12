using Cysharp.Threading.Tasks;
using GGJam.Childhood.Scripts.Mother;
using GGJam.Dialogs.Scripts;
using System.Threading;
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
		


		public async UniTask StartAsync(CancellationToken cancellation = new CancellationToken())
		{
			await _motherSong.RunMiniGame();
			
			await UniTask.WaitForSeconds(2, cancellationToken: cancellation);

			await _dialogService.ShowDialog("loh_1");

			await _dialogService.ShowDialog("pidr_2");

			_dialogService.HideWindow();
		}
	}
}