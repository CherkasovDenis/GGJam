using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace GGJam.Scripts.Coma
{
    public class Final : MonoBehaviour, IPointerClickHandler
    {
        [Inject]
        private HellModel _hellModel;

        [Inject]
        private SceneService _sceneService;

        [SerializeField]
        private Image _heart;

        [SerializeField]
        private Sprite _heart1;

        [SerializeField]
        private Image _door;

        [SerializeField]
        private Image _heaven;

        [SerializeField]
        private AudioSource _heavenSound;

        [SerializeField]
        private Image _hell;

        [SerializeField]
        private AudioSource _hellSound;

        private int _step;

        private bool _changing;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_changing)
            {
                return;
            }

            _changing = true;

            _step++;

            if (_step == 1)
            {
                ChangeHeart();
            }

            if (_step == 2)
            {
                ShowDoor();
            }

            if (_step == 3)
            {
                ShowHeavenOrHell();
            }

            if (_step == 4)
            {
                _sceneService.LoadNextScene();
            }

            DisableChangingAfterDelay().Forget();
        }

        public void ChangeHeart()
        {
            _heart.sprite = _heart1;
        }

        public void ShowDoor()
        {
            _door.DOFade(1f, 3f);
        }

        public void ShowHeavenOrHell()
        {
            if (_hellModel.SendToHell)
            {
                _hell.DOFade(1f, 3f);
                _hellSound.Play();
            }
            else
            {
                _heaven.DOFade(1f, 3f);
                _heavenSound.Play();
            }
        }

        public async UniTask DisableChangingAfterDelay()
        {
            await UniTask.WaitForSeconds(5f);

            _changing = false;
        }
    }
}