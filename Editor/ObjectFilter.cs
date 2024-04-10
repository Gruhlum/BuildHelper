using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexTecGames.Editor.BuildHelper
{
	[System.Serializable]
	public class ObjectFilter
	{
		public enum Mode { Include, Exclude }
		public Mode mode;
		public GameObject item;
	}
}