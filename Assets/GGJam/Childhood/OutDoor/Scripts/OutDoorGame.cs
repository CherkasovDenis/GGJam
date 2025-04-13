using Cysharp.Threading.Tasks;
using DG.Tweening;
using GGJam.Dialogs.Scripts;
using GGJam.Scripts;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace GGJam.Childhood.OutDoor.Scripts
{
	public class OutDoorGame : MonoBehaviour
	{
		[SerializeField]
		private Button _tower;
		[SerializeField]
		private Button gold;
		[SerializeField]
		private Button kata;
		[SerializeField]
		private Button kon;
		[SerializeField]
		private Button dog;
		[SerializeField]
		private Button dragon;
		[SerializeField]
		private Image dragonBack;
		[SerializeField]
		private TMP_Text hitDrqagon;
		[SerializeField]
		private Image bidDog;
		[SerializeField]
		private Image _showel;
		[SerializeField]
		private Sprite showel2;
		[SerializeField]
		private Image kacheli;
		[SerializeField]
		private Sprite kacheli2;
		[SerializeField]
		private AudioSource dogHit;
		[SerializeField]
		private AudioSource dogHurt;
		[SerializeField]
		private AudioSource _showelHit;
		[SerializeField]
		private AudioSource kidScream;
		[SerializeField]
		private AudioSource kidDead;

		[Inject]
		private readonly DialogService _dialogService;
		[Inject]
		private readonly HellModel _hellModel;

		private UniTaskCompletionSource _mainTask;

		public async UniTask RunMiniGame()
		{
			await _dialogService.ShowDialog(0);
			await _dialogService.ShowDialog(1);
			await _dialogService.ShowDialog(2);
			await _dialogService.ShowDialog(3);
			await _dialogService.ShowDialog(4);
			_dialogService.HideWindow();
			await _tower.OnClickAsync();
			_tower.image.DOFade(1, 1f);

			await _dialogService.ShowDialog(5);
			await _dialogService.ShowDialog(6);
			await _dialogService.ShowDialog(7);
			await _dialogService.ShowDialog(8);
			await _dialogService.ShowDialog(9);
			await _dialogService.ShowDialog(10);
			await _dialogService.ShowDialog(11);

			_dialogService.HideWindow();
			await kata.OnClickAsync();
			kata.image.DOFade(1, 1f);

			await _dialogService.ShowDialog(12);
			await _dialogService.ShowDialog(13);
			await _dialogService.ShowDialog(14);

			_dialogService.HideWindow();
			await kon.OnClickAsync();
			kon.image.DOFade(1, 1f);

			await _dialogService.ShowDialog(15);
			await _dialogService.ShowDialog(16);

			gold.image.DOFade(1, 1f);

			await _dialogService.ShowDialog(17);

			dog.image.DOFade(1, 1f);
			dog.transform.DOPunchScale(Vector3.one * .1f, 1f);
			await _dialogService.ShowDialog(18);

			await _dialogService.ShowDialog(19);
			await _dialogService.ShowDialog(20);

			hitDrqagon.DOFade(1, 1);
			dragon.gameObject.SetActive(true);
			dragon.image.DOFade(1, 1f);
			dragonBack.gameObject.SetActive(true);
			dragonBack.DOFade(1, 1f);
			_dialogService.ShowDialog(21,true).Forget();

			hitDrqagon.transform.DOScale(Vector3.one * 1.1f, .4f).SetLoops(-1, LoopType.Yoyo);
			

			await UniTask.WhenAny(HitTheDragon(), HitTheDragonCall());
			dragon.interactable = false;
			dragon.image.DOFade(0, 1f);
			hitDrqagon.DOFade(0, 1);

			bidDog.DOFade(1, 1f);

			if (_hellModel.SendToHell)
			{
				await KillDog();
			}
			else
			{
				await NotKillDog();
			}

			// await _mainTask.Task;

			async UniTask HitTheDragonCall()
			{
				await UniTask.WaitForSeconds(2);
				_dialogService.ShowDialog(22, true).Forget();
				await UniTask.WaitForSeconds(2);
				_dialogService.ShowDialog(23, true).Forget();
				await UniTask.WaitForSeconds(2);
				_dialogService.ShowDialog(24, true).Forget();
				await UniTask.WaitForSeconds(2);
				_dialogService.ShowDialog(25, true).Forget();

				_hellModel.SendToHell = false;
			}

			async UniTask HitTheDragon()
			{
				await dragon.OnClickAsync();
				_hellModel.SendToHell = true;
				dogHit.Play();
				await UniTask.WaitForSeconds(.65f);
				dogHurt.Play();
			}
		}

		private async UniTask NotKillDog()
		{
			await _dialogService.ShowDialog(32);
			await _dialogService.ShowDialog(33);
			await _dialogService.ShowDialog(34);
			await _dialogService.ShowDialog(35);
			await _dialogService.ShowDialog(36);
			await _dialogService.ShowDialog(37);
			kacheli.DOFade(1, 1);
			await _dialogService.ShowDialog(38);
			await _dialogService.ShowDialog(39);
			kacheli.sprite = kacheli2;
			await _dialogService.ShowDialog(40);
			await _dialogService.ShowDialog(41);
			kidScream.Play();
			await UniTask.WaitForSeconds(.5f);
			kidDead.Play();

			await UniTask.WaitForSeconds(1);
		}

		private async UniTask KillDog()
		{
			await _dialogService.ShowDialog(26);
			await _dialogService.ShowDialog(27);
			await _dialogService.ShowDialog(28);

			_showel.DOFade(1, 1);

			await _dialogService.ShowDialog(29);
			await _dialogService.ShowDialog(30);
			await _dialogService.ShowDialog(31);

			_showel.sprite = showel2;

			await _dialogService.ShowDialog(32);

			_showelHit.Play();

			await UniTask.WaitForSeconds(.5f);
		}
	}
}