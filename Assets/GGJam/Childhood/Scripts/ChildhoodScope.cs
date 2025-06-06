﻿using GGJam.Childhood.OutDoor.Scripts;
using GGJam.Childhood.SchoolGame.Scripts;
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
		[SerializeField]
		private MathGame _mathGame;
		[SerializeField]
		private OutDoorGame _outDoorGame;
		[SerializeField]
		private CanvasGroup _fader;
		
		
		protected override void Configure(IContainerBuilder builder)
		{
			builder.RegisterInstance(_motherSong);
			builder.RegisterInstance(_mathGame);
			builder.RegisterInstance(_outDoorGame);
			builder.RegisterEntryPoint<GameplayStateMachine>().WithParameter(_fader);
		}
	}
}