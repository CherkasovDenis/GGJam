using GGJam.Childhood.Scripts.Mother;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GGJam.ChildHood.Scripts
{
	public class ChildhoodScope : LifetimeScope
	{
		[SerializeField]
		private MotherSong _motherSong;
		
		protected override void Configure(IContainerBuilder builder)
		{
			builder.RegisterInstance(_motherSong);
			builder.RegisterEntryPoint<GameplayStateMachine>();
		}
	}
}