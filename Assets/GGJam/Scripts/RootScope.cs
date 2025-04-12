using GGJam.Dialogs.Scripts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GGJam.Scripts
{
	public class RootScope : LifetimeScope
	{
		[SerializeField]
		private DialogService _dialogService;

		protected override void Configure(IContainerBuilder builder)
		{
			base.Configure(builder);
			builder.RegisterInstance(_dialogService).As<DialogService>();
		}
	}
}