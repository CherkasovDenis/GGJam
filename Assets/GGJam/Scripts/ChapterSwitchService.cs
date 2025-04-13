using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;

namespace GGJam.Scripts
{
    public class ChapterSwitchService : MonoBehaviour
    {
        [Inject]
        private HellModel _hellModel;

        [SerializeField]
        private TMP_Text _chapterText;

        [SerializeField]
        private Image _fade;

        [SerializeField]
        private GameObject _blocker;

        [SerializeField]
        private List<string> _chapterNames;

        [SerializeField]
        private float _fadeDuration;

        [SerializeField]
        private float _textFadeDuration;

        [SerializeField]
        private float _delay;

        private int _currentSceneIndex = 0;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void SwitchChapter()
        {
            _currentSceneIndex++;

            if (_currentSceneIndex >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.Log("Quit");
                Application.Quit();
                return;
            }

            SwitchChapterAsync().Forget();
        }

        private async UniTask SwitchChapterAsync()
        {
            gameObject.SetActive(true);
            _chapterText.alpha = 0f;

            if (_currentSceneIndex == 0)
            {
                _fade.color = Color.black;
            }
            else
            {
                var color = Color.black;
                color.a = 0f;

                _fade.color = color;
                _fade.DOFade(1f, _fadeDuration);
            }

            if (_currentSceneIndex == 2)
            {
                _chapterText.text = _hellModel.SendToHell ? _chapterNames[3] : _chapterNames[2];
            }
            else
            {
                _chapterText.text = _chapterNames[_currentSceneIndex - 1];
            }

            await UniTask.WaitForSeconds(_fadeDuration - _textFadeDuration);

            _chapterText.DOFade(1f, _textFadeDuration);

            await UniTask.WaitForSeconds(_textFadeDuration);

            LoadNextScene();

            await UniTask.WaitForSeconds(_delay);

            await _chapterText.DOFade(0f, _textFadeDuration).ToUniTask();

            await _fade.DOFade(0f, _fadeDuration).ToUniTask();

            gameObject.SetActive(false);
        }

        private void LoadNextScene()
        {
            Debug.Log($"Loading scene {_currentSceneIndex}");
            SceneManager.LoadScene(_currentSceneIndex);
        }
    }
}