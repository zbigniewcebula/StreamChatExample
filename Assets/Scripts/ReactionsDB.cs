using System.Linq;
using UnityEngine;

[CreateAssetMenu(
	fileName = "ReactionsDB.asset",
	menuName = "ScriptableObjects/ReactionsDB",
	order = 1
)]
public class ReactionsDB : ScriptableObject
{
	//Accessor
	public int Count => reactions.Length;

	//Public/Inspector
	[Header("Entries")]
	public Sprite[] reactions;

	//Method
	public bool HasReaction(string id)
		=> reactions.Any(r => r.name == id);

	public Sprite GetReactionIcon(string id)
		=> reactions.FirstOrDefault(r => r.name == id);
}