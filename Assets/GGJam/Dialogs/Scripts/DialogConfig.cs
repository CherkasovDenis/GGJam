using System;
using System.Linq;
using UnityEngine;

namespace GGJam.Dialogs.Scripts
{
	public enum Character
	{
		None = 0,
		KOLYA = 1,
		PETYA = 2,
		ZHENYA = 3,
		DOG = 4,
	}
	[Serializable]
	public class Dialog
	{
		public Character Character;
		[TextArea]
		public string Text;
	}

	[CreateAssetMenu(fileName = nameof(DialogConfig), menuName = nameof(DialogConfig), order = 0)]
	public class DialogConfig : ScriptableObject
	{
		[field: SerializeField]
		public Dialog[] Dialogs { get; private set; }

		public Dialog GetDialog(int key)
		{
			return Dialogs[key];
		}
	}
}