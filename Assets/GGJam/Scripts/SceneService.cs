using VContainer;

namespace GGJam.Scripts
{
    public class SceneService
    {
        [Inject]
        private ChapterSwitchService _chapterSwitchService;

        public void LoadNextScene()
        {
            _chapterSwitchService.SwitchChapter();
        }
    }
}