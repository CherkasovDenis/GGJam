using System;
using System.Linq;
using UnityEngine;

namespace GGJam.Dialogs.Scripts
{
	public enum Character
	{
		None = 0,
		Character1 = 1,
		Character2 = 2,
		Character3 = 3,
	}
	[Serializable]
	public class Dialog
	{
		public string Key;
		public Character Character;
		[TextArea]
		public string Text;
	}

	[CreateAssetMenu(fileName = nameof(DialogConfig), menuName = nameof(DialogConfig), order = 0)]
	public class DialogConfig : ScriptableObject
	{
		[field: SerializeField]
		public Dialog[] Dialogs { get; private set; }

		public Dialog GetDialog(string key)
		{
			return Dialogs.FirstOrDefault(x => x.Key == key);
		}
	}
}