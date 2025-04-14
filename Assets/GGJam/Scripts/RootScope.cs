using GGJam.Dialogs.Scripts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GGJam.Scripts
{
	public class RootScope : LifetimeScope
	{
		[SerializeField]
		private Texture2D _cursor;

		[SerializeField]
		private DialogService _dialogService;

		[SerializeField]
		private ChapterSwitchService _chapterSwitchService;

		protected override void Configure(IContainerBuilder builder)
		{
			base.Configure(builder);
			builder.RegisterInstance(_dialogService).As<DialogService>();
			builder.RegisterInstance(_chapterSwitchService).As<ChapterSwitchService>();

			builder.Register<SceneService>(Lifetime.Singleton);

			builder.Register<HellModel>(Lifetime.Singleton);

#if !UNITY_WEBGL
			Cursor.SetCursor(_cursor, Vector2.zero, CursorMode.Auto);
#endif
		}
	}
}