using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HexTecGames
{
	[System.Serializable]
	public class SceneOrder
	{
		public SceneAsset scene;
		[Tooltip("from low to high")]
		public int order;
	}
}