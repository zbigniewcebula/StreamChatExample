using UnityEngine;
using UnityEngine.UI;

public class MemberEntry : MonoBehaviour
{
	//Static

	//Accessor
	public MemberEntryData Data { get; private set; } = null;

	//Public/Inspector
	[Header("Bindings")]
	[SerializeField] private Text nameLabel = null;
	[SerializeField] private Text rank = null;
	[SerializeField] private Text points = null;
	[SerializeField] private EmblemIcon rankEmblem = null;

#if UNITY_EDITOR
	[Header("DEBUG - ONLY IN INSPECTOR")]
	public bool debugSetMemberData = false;
	public MemberEntryData debugClanData = new MemberEntryData();
#endif

	//Events

	//Private

	//Methods
	public void SetData(MemberEntryData data)
	{
		Data = data;

		nameLabel.text = data.name;
		rank.text = data.rank;
		points.text = data.points.ToString();
		rankEmblem.IconID = (int)data.rankEmblem;
	}

#if UNITY_EDITOR

	private void OnDrawGizmosSelected()
	{
		if(!debugSetMemberData)
			return;
		debugSetMemberData = false;

		SetData(debugClanData);
	}

#endif

	//Classes
#if UNITY_EDITOR

	[System.Serializable]
	public class MemberEntryData
	{
#else
	public class MemberEntryData
	{
#endif

		public enum RankEmblem
		{
			None,
			Bronze,
			Silver,
			Gold
		}

		public enum Rank
		{
			None,
			Member,
			Elder,
			Leader
		}

		//Public
		public string id;
		public string name;
		public string rank;
		public int points;

		public RankEmblem rankEmblem;

		///Instance
		public MemberEntry instance;
	}
}