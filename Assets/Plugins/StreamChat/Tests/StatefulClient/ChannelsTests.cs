﻿#if STREAM_TESTS_ENABLED
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using StreamChat.Core.LowLevelClient.Requests;
using StreamChat.Core;
using StreamChat.Core.StatefulModels;
using UnityEngine.TestTools;

namespace StreamChat.Tests.StatefulClient
{
    /// <summary>
    /// Tests operations executed on <see cref="StreamChannel"/>
    /// </summary>
    internal class ChannelsTests : BaseStateIntegrationTests
    {
        [UnityTest]
        public IEnumerator When_creating_channel_with_id_expect_no_errors()
            => ConnectAndExecute(When_creating_channel_with_id_expect_no_errors_Async);

        private async Task When_creating_channel_with_id_expect_no_errors_Async()
        {
            var channel = await CreateUniqueTempChannelAsync();
        }

        [UnityTest]
        public IEnumerator When_creating_channel_for_users_expect_no_errors()
            => ConnectAndExecute(When_creating_channel_for_users_expect_no_errors_Async);

        private async Task When_creating_channel_for_users_expect_no_errors_Async()
        {
            var userSteveId = "integration-test-user-1-" + Guid.NewGuid();
            var userDaveId = "integration-test-user-2-" + Guid.NewGuid();

            var requestBody = new UpdateUsersRequest
            {
                Users = new Dictionary<string, UserObjectRequest>
                {
                    {
                        userSteveId, new UserObjectRequest
                        {
                            Id = userSteveId,
                            AdditionalProperties = new Dictionary<string, object>
                            {
                                {"name", "Steve"}
                            }
                        }
                    },
                    {
                        userDaveId, new UserObjectRequest
                        {
                            Id = userDaveId,
                            AdditionalProperties = new Dictionary<string, object>
                            {
                                {"name", "Dave"}
                            }
                        }
                    }
                }
            };

            var response = await Client.InternalLowLevelClient.UserApi.UpsertManyUsersAsync(requestBody);
            var users = response.Users.Select(_ => _.Value).ToList();

            var filters = new Dictionary<string, object>()
            {
                {
                    "id", new Dictionary<string, object>
                    {
                        {
                            "$in", users.Select(_ => _.Id).ToList()
                        }
                    }
                }
            };

            var streamUsers = await Client.QueryUsersAsync(filters);
            Assert.NotNull(streamUsers);

            var steveUser = streamUsers.FirstOrDefault(_ => _.Id == userSteveId);
            var daveUser = streamUsers.FirstOrDefault(_ => _.Id == userDaveId);

            Assert.NotNull(steveUser);
            Assert.NotNull(daveUser);

            var channelForMembers =
                await Client.GetOrCreateChannelWithMembersAsync(ChannelType.Messaging, streamUsers);
            Assert.NotNull(channelForMembers);
            Assert.NotNull(channelForMembers.Members);
            Assert.AreEqual(2, channelForMembers.Members.Count);

            var steveMember = channelForMembers.Members.FirstOrDefault(_ => _.User.Id == userSteveId);
            var daveMember = channelForMembers.Members.FirstOrDefault(_ => _.User.Id == userDaveId);

            Assert.NotNull(steveMember);
            Assert.NotNull(daveMember);

            //StreamTodo: would be best to remove the users but this is restricted from FE SDK, we would need a backend service with CRON
        }

        [UnityTest]
        public IEnumerator When_mute_channel_expect_muted() => ConnectAndExecute(When_mute_channel_expect_muted_Async);

        private async Task When_mute_channel_expect_muted_Async()
        {
            var channel = await CreateUniqueTempChannelAsync();

            await channel.MuteChannelAsync();

            Assert.IsNotEmpty(Client.LocalUserData.ChannelMutes);

            var channelMute = Client.LocalUserData.ChannelMutes.FirstOrDefault(m => m.Channel == channel);
            Assert.IsNotNull(channelMute);

            Assert.AreEqual(channelMute.User, Client.LocalUserData.User);

            Assert.AreEqual(true, channel.Muted);
        }

        [UnityTest]
        public IEnumerator When_unmute_muted_channel_expect_unmuted()
            => ConnectAndExecute(When_unmute_muted_channel_expect_unmuted_Async);

        private async Task When_unmute_muted_channel_expect_unmuted_Async()
        {
            var channel = await CreateUniqueTempChannelAsync();

            await channel.MuteChannelAsync();

            Assert.IsNotEmpty(Client.LocalUserData.ChannelMutes);

            var channelMute = Client.LocalUserData.ChannelMutes.FirstOrDefault(m => m.Channel == channel);
            Assert.IsNotNull(channelMute);
            Assert.AreEqual(true, channel.Muted);

            Assert.AreEqual(channelMute.User, Client.LocalUserData.User);

            await channel.UnmuteChannelAsync();

            await WaitWhileConditionTrue(() =>
            {
                channelMute = Client.LocalUserData.ChannelMutes.FirstOrDefault(m => m.Channel == channel);
                return channelMute != null;
            });

            Assert.IsNull(channelMute);
            Assert.AreEqual(false, channel.Muted);
        }

        [UnityTest]
        public IEnumerator When_delete_multiple_channels_expect_no_channels_in_state_client()
            => ConnectAndExecute(When_delete_multiple_channels_expect_no_channels_in_state_client_Async);

        private async Task When_delete_multiple_channels_expect_no_channels_in_state_client_Async()
        {
            var channel = await CreateUniqueTempChannelAsync();
            var channel2 = await CreateUniqueTempChannelAsync();

            Assert.IsTrue(Client.WatchedChannels.Contains(channel));
            Assert.IsTrue(Client.WatchedChannels.Contains(channel2));

            var response
                = await Client.DeleteMultipleChannelsAsync(new IStreamChannel[] {channel, channel2},
                    isHardDelete: true);

            Assert.That(response.Result, Contains.Key(channel.Cid));
            Assert.That(response.Result, Contains.Key(channel2.Cid));

            SkipThisTempChannelDeletionInTearDown(channel);
            SkipThisTempChannelDeletionInTearDown(channel2);

            await WaitWhileConditionTrue(() => Client.WatchedChannels.Any());

            Assert.IsEmpty(Client.WatchedChannels);
        }

        [UnityTest]
        public IEnumerator When_truncate_channel_with_past_created_at_expect_no_messages_cleared()
            => ConnectAndExecute(When_truncate_channel_expect_messages_cleared_Async);

        private async Task When_truncate_channel_expect_messages_cleared_Async()
        {
            var channel = await CreateUniqueTempChannelAsync();

            await channel.SendNewMessageAsync("Hello");
            await channel.SendNewMessageAsync("Hello 2");
            await channel.SendNewMessageAsync("Hello 3");

            Assert.AreEqual(3, channel.Messages.Count);

            var beforeDate = DateTimeOffset.UtcNow.AddHours(-1);

            await channel.TruncateAsync(beforeDate, "Hi sorry for deleting all", isHardDelete: true);

            //expect no messages removed + system message added
            Assert.AreEqual(4, channel.Messages.Count);
        }

        [UnityTest]
        public IEnumerator When_truncate_channel_with_system_message_expect_only_system_message()
            => ConnectAndExecute(When_truncate_channel_with_system_message_expect_only_system_message_Async);

        private async Task When_truncate_channel_with_system_message_expect_only_system_message_Async()
        {
            var channel = await CreateUniqueTempChannelAsync();

            await channel.SendNewMessageAsync("Hello");
            await channel.SendNewMessageAsync("Hello 2");
            await channel.SendNewMessageAsync("Hello 3");

            Assert.AreEqual(3, channel.Messages.Count);

            const string systemMessage = "Hi sorry for deleting all";
            await channel.TruncateAsync(systemMessage: systemMessage);

            Assert.AreEqual(1, channel.Messages.Count);
            Assert.AreEqual(systemMessage, channel.Messages[0].Text);
        }

        private class ClanData
        {
            public int MaxMembers;
            public string Name;
            public List<string> Tags;
        }

        [UnityTest]
        public IEnumerator When_set_channel_custom_data_expect_data_on_channel_object()
            => ConnectAndExecute(When_set_channel_custom_data_expect_data_set_on_channel_Async);

        private async Task When_set_channel_custom_data_expect_data_set_on_channel_Async()
        {
            var channel = await CreateUniqueTempChannelAsync();

            var setClanInfo = new ClanData
            {
                MaxMembers = 50,
                Name = "Wild Boards",
                Tags = new List<string>
                {
                    "Competitive",
                    "Legends",
                }
            };

            await channel.UpdatePartialAsync(setFields: new Dictionary<string, object>()
            {
                {"owned_dogs", 5},
                {
                    "breakfast", new string[]
                    {
                        "donuts"
                    }
                },
                {
                    "clan_info", setClanInfo
                }
            });

            var ownedDogs = channel.CustomData.Get<int>("owned_dogs");
            var breakfast = channel.CustomData.Get<List<string>>("breakfast");
            var clanInfo = channel.CustomData.Get<ClanData>("clan_info");
            Assert.AreEqual(5, ownedDogs);

            Assert.Contains("donuts", breakfast);

            Assert.AreEqual(50, clanInfo.MaxMembers);
            Assert.AreEqual("Wild Boards", clanInfo.Name);
            Assert.Contains("Competitive", clanInfo.Tags);
        }

        [UnityTest]
        public IEnumerator When_unset_channel_custom_data_expect_no_data_on_channel_object()
            => ConnectAndExecute(When_unset_channel_custom_data_expect_no_data_on_channel_object_Async);

        private async Task When_unset_channel_custom_data_expect_no_data_on_channel_object_Async()
        {
            var channel = await CreateUniqueTempChannelAsync();
            await channel.UpdatePartialAsync(setFields: new Dictionary<string, object>()
            {
                {"owned_dogs", 5},
                {
                    "breakfast", new string[]
                    {
                        "donuts"
                    }
                }
            });

            var ownedDogs = channel.CustomData.Get<int>("owned_dogs");
            var breakfast = channel.CustomData.Get<List<string>>("breakfast");
            
            Assert.AreEqual(5, ownedDogs);
            Assert.Contains("donuts", breakfast);
            
            await channel.UpdatePartialAsync(unsetFields: new string[] {"owned_dogs", "breakfast"});

            //StreamTodo: this can potentially be non deterministic because we rely on WS event being received before call ends

            Assert.IsFalse(channel.CustomData.ContainsKey("owned_dogs"));
            Assert.IsFalse(channel.CustomData.ContainsKey("breakfast"));
        }

        [UnityTest]
        public IEnumerator When_add_user_to_channel_expect_user_included_in_members()
            => ConnectAndExecute(When_add_user_to_channel_expect_user_included_in_members_Async);

        private async Task When_add_user_to_channel_expect_user_included_in_members_Async()
        {
            var channel = await CreateUniqueTempChannelAsync();

            var otherUserId = OtherAdminUsersCredentials.First().UserId;

            var users = await Client.QueryUsersAsync(new Dictionary<string, object>()
            {
                {
                    "id", new Dictionary<string, object>
                    {
                        {"$eq", otherUserId}
                    }
                }
            });
            var otherUser = users.First();

            await channel.AddMembersAsync(otherUser);

            await WaitWhileConditionTrue(() => channel.Members.All(m => m.User != otherUser));
            Assert.NotNull(channel.Members.FirstOrDefault(member => member.User == otherUser));
        }

        [UnityTest]
        public IEnumerator When_query_members_expect_proper_members_returned()
            => ConnectAndExecute(When_query_members_expect_proper_members_returned_Async);

        private async Task When_query_members_expect_proper_members_returned_Async()
        {
            var channel = await CreateUniqueTempChannelAsync();

            var otherUsers = OtherAdminUsersCredentials.Take(3).ToArray();
            var firstCredentials = otherUsers.First();
            var lastCredentials = otherUsers.Last();

            var users = await Client.QueryUsersAsync(new Dictionary<string, object>()
            {
                {
                    "id", new Dictionary<string, object>
                    {
                        {"$in", otherUsers.Select(u => u.UserId).ToArray()}
                    }
                }
            });

            var firstUser = users.FirstOrDefault(u => u.Id == firstCredentials.UserId);
            var lastUser = users.FirstOrDefault(u => u.Id == lastCredentials.UserId);
            
            Assert.NotNull(firstUser);
            Assert.NotNull(lastUser);
            
            await channel.AddMembersAsync(users);

            var result = await channel.QueryMembers(new Dictionary<string, object>()
            {
                {
                    "id", new Dictionary<string, object>
                    {
                        {"$in", new[] {firstCredentials.UserId, lastCredentials.UserId}}
                    }
                }
            });

            var firstMember = result.FirstOrDefault(m => m.User == firstUser);
            var lastMember = result.FirstOrDefault(m => m.User == lastUser);
            
            Assert.NotNull(firstMember);
            Assert.NotNull(lastMember);
        }
    }
}

#endif