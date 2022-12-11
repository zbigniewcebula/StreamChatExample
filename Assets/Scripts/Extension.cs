using StreamChat.Core.StatefulModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;

public static class Extension
{
	public static void Invoke(this Button btn) => btn.onClick.Invoke();

	public static async Task<IEnumerable<IStreamChannelMember>> QueryMembersEx(
		this IStreamChannel channel
	)
	{
		return await channel.QueryMembers(
			new Dictionary<string, object>() {
				{
					"created_at", new Dictionary<string, object>
					{
						{ "$gt", "1970-01-01T00:00:01.00Z" }
					}
				}
			}
		);
	}
}