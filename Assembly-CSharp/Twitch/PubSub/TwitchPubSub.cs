using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Twitch.PubSub
{
	public class TwitchPubSub
	{
		public void Connect(string userID)
		{
			if (this.cts != null)
			{
				this.cts.Cancel();
			}
			this.cts = new CancellationTokenSource();
			Task.Run(() => this.StartAsync(new TwitchTopic[]
			{
				TwitchTopic.ChannelPoints(userID),
				TwitchTopic.Bits(userID),
				TwitchTopic.Subscription(userID),
				TwitchTopic.HypeTrain(userID),
				TwitchTopic.CreatorGoal(userID)
			}, this.cts.Token));
		}

		public void Disconnect()
		{
			this.cts.Cancel();
			TwitchPubSub.reconnect = false;
		}

		public async Task StartAsync(TwitchTopic[] newTopics, CancellationToken token)
		{
			TwitchPubSub.<>c__DisplayClass12_0 CS$<>8__locals1 = new TwitchPubSub.<>c__DisplayClass12_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.newTopics = newTopics;
			CS$<>8__locals1.token = token;
			this.topics = CS$<>8__locals1.newTopics;
			TwitchPubSub.reconnect = false;
			this.pingTimer = new Timer(TimeSpan.FromSeconds(30.0).TotalMilliseconds);
			this.pingTimer.Elapsed += this.PingTimer_Elapsed;
			this.pingTimer.Start();
			await this.StartListening(CS$<>8__locals1.newTopics);
			while (!CS$<>8__locals1.token.IsCancellationRequested)
			{
				ArraySegment<byte> messageBuffer = new ArraySegment<byte>(new byte[1024]);
				StringBuilder completeMessage = new StringBuilder();
				WebSocketReceiveResult webSocketReceiveResult = await this.socket.ReceiveAsync(messageBuffer, CS$<>8__locals1.token);
				completeMessage.Append(Encoding.UTF8.GetString(messageBuffer.Array));
				while (!webSocketReceiveResult.EndOfMessage)
				{
					messageBuffer = new ArraySegment<byte>(new byte[1024]);
					webSocketReceiveResult = await this.socket.ReceiveAsync(messageBuffer, CS$<>8__locals1.token);
					completeMessage.Append(Encoding.UTF8.GetString(messageBuffer.Array));
				}
				if (webSocketReceiveResult.MessageType == WebSocketMessageType.Close)
				{
					TwitchPubSub.reconnect = true;
					break;
				}
				try
				{
					string text = completeMessage.ToString();
					this.HandleMessage(text, text.Contains("hype-train-start") ? TwitchPubSub.MessageTypes.HypeStart : TwitchPubSub.MessageTypes.Standard);
				}
				catch (Exception ex)
				{
					Debug.LogWarning(completeMessage.ToString());
					Debug.LogWarning(ex.ToString());
					if (ex.InnerException != null)
					{
						Debug.LogWarning(ex.InnerException.ToString());
					}
					TwitchPubSub.reconnect = true;
				}
				if (TwitchPubSub.reconnect)
				{
					IEnumerable<TimeSpan> reconnectTimeouts = TwitchPubSub._ReconnectTimeouts;
					Func<TimeSpan, bool> func;
					if ((func = CS$<>8__locals1.<>9__1) == null)
					{
						TwitchPubSub.<>c__DisplayClass12_0 CS$<>8__locals2 = CS$<>8__locals1;
						Func<TimeSpan, bool> func2 = (TimeSpan t) => t.TotalMilliseconds == CS$<>8__locals1.<>4__this.reconnectTimer.Interval;
						CS$<>8__locals2.<>9__1 = func2;
						func = func2;
					}
					if (!reconnectTimeouts.Any(func))
					{
						this.reconnectTimer.Interval = TwitchPubSub._ReconnectTimeouts[0].TotalMilliseconds;
					}
					else if (TwitchPubSub._ReconnectTimeouts.Last<TimeSpan>().TotalMilliseconds == this.reconnectTimer.Interval)
					{
						TwitchPubSub.reconnect = false;
					}
					else
					{
						for (int i = 0; i < TwitchPubSub._ReconnectTimeouts.Length; i++)
						{
							if (TwitchPubSub._ReconnectTimeouts[i].TotalMilliseconds == this.reconnectTimer.Interval)
							{
								this.reconnectTimer.Interval = TwitchPubSub._ReconnectTimeouts[i + 1].TotalMilliseconds;
								break;
							}
						}
					}
					await Task.Delay((int)this.reconnectTimer.Interval);
					break;
				}
				messageBuffer = default(ArraySegment<byte>);
				completeMessage = null;
			}
			if (TwitchPubSub.reconnect)
			{
				Task.Run(() => CS$<>8__locals1.<>4__this.StartAsync(CS$<>8__locals1.newTopics, CS$<>8__locals1.token));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void HandleMessage(string receivedMessage, TwitchPubSub.MessageTypes msgType)
		{
			if (msgType != TwitchPubSub.MessageTypes.Standard)
			{
				if (msgType == TwitchPubSub.MessageTypes.HypeStart)
				{
					TwitchManager.Current.StartHypeTrain();
				}
				return;
			}
			JObject jobject = JObject.Parse(receivedMessage);
			string text = Extensions.Value<string>(jobject["type"]);
			if (text == "RESPONSE" && Extensions.Value<string>(jobject["error"]) != "")
			{
				return;
			}
			if (text == "RESPONSE")
			{
				return;
			}
			if (this.HandlePongMessage(receivedMessage))
			{
				return;
			}
			if (this.HandleReconnectMessage(receivedMessage))
			{
				return;
			}
			this.HandleRedemptionsMessages(receivedMessage);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public async Task StartListening(IEnumerable<TwitchTopic> topics)
		{
			this.socket = new ClientWebSocket();
			PubSubListenMessage msg = new PubSubListenMessage();
			PubSubListenMessage msg2 = msg;
			PubSubListenMessage.PubSubListenData pubSubListenData = new PubSubListenMessage.PubSubListenData();
			pubSubListenData.auth_token = TwitchManager.Current.Authentication.oauth.Substring(6);
			pubSubListenData.topics = topics.Select((TwitchTopic t) => t.TopicString).ToArray<string>();
			msg2.data = pubSubListenData;
			await this.socket.ConnectAsync(new Uri("wss://pubsub-edge.twitch.tv"), CancellationToken.None).ContinueWith<Task>((Task t) => this.SendMessageOnSocket(JsonConvert.SerializeObject(msg)));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void PingTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			string text = "{ \"type\": \"PING\" }";
			this.SendMessageOnSocket(text).GetAwaiter().GetResult();
			this.pongTimer = new Timer(TimeSpan.FromSeconds(10.0).TotalMilliseconds);
			this.pongTimer.Elapsed += this.PongTimer_Elapsed;
			this.pongTimer.Start();
			this.pingAcknowledged = false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void PongTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (!this.pingAcknowledged)
			{
				TwitchPubSub.reconnect = true;
				this.pongTimer.Dispose();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Task SendMessageOnSocket(string message)
		{
			if (this.socket.State != WebSocketState.Open)
			{
				return Task.CompletedTask;
			}
			byte[] bytes = Encoding.ASCII.GetBytes(message);
			return this.socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
		}

		public event EventHandler<PubSubBitRedemptionMessage.BitRedemptionData> OnBitsRedeemed;

		public event EventHandler<PubSubSubscriptionRedemptionMessage> OnSubscriptionRedeemed;

		public event EventHandler<PubSubChannelPointMessage.ChannelRedemptionData> OnChannelPointsRedeemed;

		public event EventHandler<PubSubGoalMessage.Goal> OnGoalAchieved;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool HandlePongMessage(string message)
		{
			if (message.Contains("\"PONG\""))
			{
				this.pingAcknowledged = true;
				this.pongTimer.Stop();
				this.pongTimer.Dispose();
				return true;
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool HandleReconnectMessage(string message)
		{
			if (message.Contains("\"RECONNECT\""))
			{
				TwitchPubSub.reconnect = true;
				return true;
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool HandleRedemptionsMessages(string message)
		{
			JObject jobject = JObject.Parse(message);
			if (Extensions.Value<string>(jobject["type"]) == "MESSAGE")
			{
				string text = Extensions.Value<string>(jobject["data"]["topic"]);
				if (text.StartsWith("channel-points-channel-v1"))
				{
					string text2 = Extensions.Value<string>(jobject["data"]["message"]);
					PubSubChannelPointMessage pubSubChannelPointMessage = null;
					try
					{
						pubSubChannelPointMessage = PubSubChannelPointMessage.Deserialize(text2);
					}
					catch (Exception ex)
					{
						Debug.LogError(ex.ToString());
						Debug.LogError(text2);
					}
					if (this.OnChannelPointsRedeemed != null && pubSubChannelPointMessage != null)
					{
						this.OnChannelPointsRedeemed(null, pubSubChannelPointMessage.data);
					}
					return true;
				}
				if (text.StartsWith("channel-bits-events"))
				{
					string text3 = Extensions.Value<string>(jobject["data"]["message"]);
					PubSubBitRedemptionMessage pubSubBitRedemptionMessage = null;
					try
					{
						pubSubBitRedemptionMessage = PubSubBitRedemptionMessage.Deserialize(text3);
					}
					catch (Exception ex2)
					{
						Debug.LogError(ex2.ToString());
						Debug.LogError(text3);
					}
					if (this.OnBitsRedeemed != null && pubSubBitRedemptionMessage != null)
					{
						this.OnBitsRedeemed(null, pubSubBitRedemptionMessage.data);
					}
					return true;
				}
				if (text.StartsWith("channel-subscribe-events"))
				{
					string text4 = Extensions.Value<string>(jobject["data"]["message"]);
					PubSubSubscriptionRedemptionMessage pubSubSubscriptionRedemptionMessage = null;
					try
					{
						pubSubSubscriptionRedemptionMessage = PubSubSubscriptionRedemptionMessage.Deserialize(text4);
					}
					catch (Exception ex3)
					{
						Debug.LogError(ex3.ToString());
						Debug.LogError(text4);
					}
					if (this.OnSubscriptionRedeemed != null && pubSubSubscriptionRedemptionMessage != null)
					{
						this.OnSubscriptionRedeemed(null, pubSubSubscriptionRedemptionMessage);
					}
					return true;
				}
				if (text.StartsWith("creator-goals-events"))
				{
					string text5 = Extensions.Value<string>(jobject["data"]["message"]);
					PubSubGoalMessage pubSubGoalMessage = null;
					try
					{
						pubSubGoalMessage = PubSubGoalMessage.Deserialize(text5);
					}
					catch (Exception ex4)
					{
						Debug.LogError(ex4.ToString());
						Debug.LogError(text5);
					}
					if (pubSubGoalMessage.type == "goal_achieved" && this.OnGoalAchieved != null && pubSubGoalMessage != null)
					{
						this.OnGoalAchieved(null, pubSubGoalMessage.data.goal);
					}
					return true;
				}
				if (text.StartsWith("hype-train-events-v1"))
				{
					try
					{
						string text6 = jobject["data"]["message"].ToString();
						Debug.LogWarning(text6);
						if (text6.Contains("hype-train-start"))
						{
							TwitchManager.Current.StartHypeTrain();
						}
						else if (text6.Contains("hype-train-level-up"))
						{
							TwitchManager.Current.IncrementHypeTrainLevel();
						}
						else if (text6.Contains("hype-train-end"))
						{
							TwitchManager.Current.EndHypeTrain();
						}
					}
					catch (Exception ex5)
					{
						Debug.LogWarning("Hype Train Error: " + message);
						Debug.LogWarning("Hype Train Exception: " + ex5.ToString());
					}
					return true;
				}
			}
			return false;
		}

		public void Cleanup()
		{
			if (this.pingTimer != null)
			{
				this.pingTimer.Dispose();
			}
			if (this.pongTimer != null)
			{
				this.pongTimer.Dispose();
			}
			if (this.socket != null)
			{
				this.socket.Dispose();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ClientWebSocket socket;

		[PublicizedFrom(EAccessModifier.Private)]
		public Timer pingTimer;

		[PublicizedFrom(EAccessModifier.Private)]
		public Timer pongTimer;

		[PublicizedFrom(EAccessModifier.Private)]
		public Timer reconnectTimer = new Timer();

		[PublicizedFrom(EAccessModifier.Private)]
		public bool pingAcknowledged;

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool reconnect = false;

		[PublicizedFrom(EAccessModifier.Private)]
		public TwitchTopic[] topics;

		[PublicizedFrom(EAccessModifier.Private)]
		public CancellationTokenSource cts;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly TimeSpan[] _ReconnectTimeouts = new TimeSpan[]
		{
			TimeSpan.FromSeconds(1.0),
			TimeSpan.FromSeconds(5.0),
			TimeSpan.FromSeconds(10.0),
			TimeSpan.FromSeconds(30.0),
			TimeSpan.FromMinutes(1.0),
			TimeSpan.FromMinutes(5.0)
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public enum MessageTypes
		{
			Standard,
			HypeStart
		}
	}
}
