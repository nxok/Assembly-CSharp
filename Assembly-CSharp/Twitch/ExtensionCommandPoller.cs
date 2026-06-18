using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UniLinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Twitch
{
	public class ExtensionCommandPoller
	{
		public void Init()
		{
			this.commandQueue = new Queue<ExtensionAction>();
			this.transactionHistory = new HashSet<string>();
			this.lastPollTime = Time.time;
			this.login = TwitchManager.Current.Authentication.userName;
			GameManager.Instance.StartCoroutine(this.CreateQueue());
		}

		public void Cleanup()
		{
			this.commandQueue.Clear();
			this.commandQueue = null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isInCooldown()
		{
			switch (TwitchManager.Current.CooldownType)
			{
			case TwitchManager.CooldownTypes.Startup:
			case TwitchManager.CooldownTypes.Time:
			case TwitchManager.CooldownTypes.BloodMoonDisabled:
			case TwitchManager.CooldownTypes.QuestDisabled:
				return true;
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool cooldownUpdateable()
		{
			return (TwitchManager.Current.CooldownType == TwitchManager.CooldownTypes.Startup || TwitchManager.Current.CooldownType == TwitchManager.CooldownTypes.Time) && Time.realtimeSinceStartup - this.lastPollTime > 30f;
		}

		public void Update()
		{
			if (this.queueExists && TwitchManager.Current.AllowActions && Time.realtimeSinceStartup - this.lastPollTime > 3f && !this.isInCooldown() && TwitchManager.Current.Authentication.oauth != "")
			{
				GameManager.Instance.StartCoroutine(this.PollQueue());
				this.lastPollTime = Time.realtimeSinceStartup;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator PollQueue()
		{
			using (UnityWebRequest req = UnityWebRequest.Get("https://2v3d0ewjcg.execute-api.us-east-1.amazonaws.com/prod/broadcaster/actions"))
			{
				req.SetRequestHeader("Authorization", TwitchManager.Current.Authentication.userID + " " + TwitchManager.Current.Authentication.oauth.Substring(6));
				req.SetRequestHeader("Content-Type", "application/json");
				yield return req.SendWebRequest();
				if (req.result != 1)
				{
					Log.Warning(string.Format("Could not retrieve commands with status code {0}: {1}", req.responseCode, req.downloadHandler.text));
				}
				else
				{
					try
					{
						ExtensionActionResponse extensionActionResponse = JsonConvert.DeserializeObject<ExtensionActionResponse>(req.downloadHandler.text);
						if (this.commandQueue != null)
						{
							if (extensionActionResponse.bitActions.Count > 0)
							{
								long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
								extensionActionResponse.bitActions.Sort((ExtensionBitAction a, ExtensionBitAction b) => a.time_created.CompareTo(b.time_created));
								extensionActionResponse.bitActions.ForEach(delegate(ExtensionBitAction bitAction)
								{
									if (this.transactionHistory.Contains(bitAction.txn_id))
									{
										Log.Warning("duplicate transaction received with id " + bitAction.txn_id);
										return;
									}
									this.transactionHistory.Add(bitAction.txn_id);
									if (currentTime - bitAction.time_created <= 30000L)
									{
										Log.Out(string.Concat(new string[] { "bit action ", bitAction.command, " received from ", bitAction.username, " with txn_id ", bitAction.txn_id }));
										this.commandQueue.Enqueue(bitAction);
										return;
									}
									int num;
									string text;
									if (int.TryParse(bitAction.username, out num) && TwitchManager.Current.ViewerData.IdToUsername.TryGetValue(num, out text))
									{
										TwitchManager twitchManager = TwitchManager.Current;
										twitchManager.AddToBitPot((int)((float)bitAction.cost * twitchManager.BitPotPercentage));
										twitchManager.ViewerData.AddCredit(text, bitAction.cost, false);
										ViewerEntry viewerEntry = TwitchManager.Current.ViewerData.GetViewerEntry(text);
										twitchManager.extensionManager.PushUserBalance(new ValueTuple<string, int>(bitAction.username, viewerEntry.BitCredits));
										return;
									}
									Log.Warning("could not give credit to user id " + bitAction.username);
								});
								this.DeleteTransactionFromTable(extensionActionResponse.bitActions.Select((ExtensionBitAction a) => a.txn_id).ToList<string>());
							}
							extensionActionResponse.standardActions.ForEach(delegate(ExtensionAction cmd)
							{
								if (!cmd.command.Equals("#refreshcredit"))
								{
									this.commandQueue.Enqueue(cmd);
									return;
								}
								int num2;
								string text2;
								if (int.TryParse(cmd.username, out num2) && TwitchManager.Current.ViewerData.IdToUsername.TryGetValue(num2, out text2))
								{
									ViewerEntry viewerEntry2 = TwitchManager.Current.ViewerData.GetViewerEntry(text2);
									TwitchManager.Current.extensionManager.PushUserBalance(new ValueTuple<string, int>(cmd.username, viewerEntry2.BitCredits));
									TwitchManager.Current.extensionManager.PushViewerChatState(cmd.username, true);
									Log.Out("added " + cmd.username + " to new chatters");
									return;
								}
								TwitchManager.Current.extensionManager.PushViewerChatState(cmd.username, false);
							});
						}
					}
					catch (Exception ex)
					{
						Log.Warning("command poller encountered issue receving this data: " + req.downloadHandler.text + "\n excption thrown: " + ex.Message);
					}
				}
			}
			UnityWebRequest req = null;
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator CreateQueue()
		{
			using (UnityWebRequest req = UnityWebRequest.Put("https://2v3d0ewjcg.execute-api.us-east-1.amazonaws.com/prod/command-queue", "{}"))
			{
				req.SetRequestHeader("Authorization", this.login + " " + TwitchManager.Current.Authentication.oauth.Substring(6));
				req.SetRequestHeader("Content-Type", "application/json");
				yield return req.SendWebRequest();
				if (req.result != 1)
				{
					Log.Warning("Could not create queue");
				}
				else
				{
					JObject jobject = JObject.Parse(req.downloadHandler.text);
					JToken jtoken;
					this.queueExists = jobject != null && jobject.TryGetValue("message", ref jtoken) && jtoken.ToString() == "success";
					if (!this.queueExists)
					{
						Log.Warning("Could not create queue");
					}
				}
			}
			UnityWebRequest req = null;
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void DeleteTransactionFromTable(List<string> transactions)
		{
			GameManager.Instance.StartCoroutine(this.DeleteTransactionFromTableCoroutine(transactions));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator DeleteTransactionFromTableCoroutine(List<string> transactions)
		{
			string text = JsonConvert.SerializeObject(new ExtensionDeleteBitActionsRequestData
			{
				transactions = transactions
			});
			using (UnityWebRequest req = UnityWebRequest.Put("https://2v3d0ewjcg.execute-api.us-east-1.amazonaws.com/prod/broadcaster/actions", text))
			{
				req.method = "DELETE";
				req.SetRequestHeader("Authorization", TwitchManager.Current.Authentication.userID + " " + TwitchManager.Current.Authentication.oauth.Substring(6));
				req.SetRequestHeader("Content-Type", "application/json");
				yield return req.SendWebRequest();
				if (req.result != 1)
				{
					Log.Warning("Failed to delete the transactions");
				}
			}
			UnityWebRequest req = null;
			yield break;
			yield break;
		}

		public bool HasCommand()
		{
			return this.commandQueue.Count > 0;
		}

		public ExtensionAction GetCommand()
		{
			return this.commandQueue.Dequeue();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const long BIT_ACTION_TIMEOUT_MS = 30000L;

		[PublicizedFrom(EAccessModifier.Private)]
		public Queue<ExtensionAction> commandQueue;

		[PublicizedFrom(EAccessModifier.Private)]
		public HashSet<string> transactionHistory;

		[PublicizedFrom(EAccessModifier.Private)]
		public string login;

		[PublicizedFrom(EAccessModifier.Private)]
		public float lastPollTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool queueExists;
	}
}
