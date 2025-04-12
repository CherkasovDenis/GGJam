using VContainer;
using VContainer.Unity;

namespace GGJam.ChildHood.Scripts
{
	public class ChildhoodScope : LifetimeScope
	{
		protected override void Configure(IContainerBuilder builder)
		{
			builder.RegisterEntryPoint<GameplayStateMachine>();
		}
	}
}