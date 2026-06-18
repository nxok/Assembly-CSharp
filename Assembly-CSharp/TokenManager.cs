using System;
using System.Collections.Generic;
using UnityEngine;

public class TokenManager
{
	public AITokenClaim TryClaimToken(EntityAlive claimer, AITokenType tokenType, EntityAlive target, float claimTimeout, AIClaimTokenFlags claimTokenFlags = AIClaimTokenFlags.None)
	{
		if (claimer == null || target == null)
		{
			return AITokenClaim.NoneAvailable;
		}
		TokenManager.EntityTokenState entityTokenState;
		if (!this.EntityTokenStates.TryGetValue(target, out entityTokenState))
		{
			entityTokenState = new TokenManager.EntityTokenState(target);
			this.EntityTokenStates[target] = entityTokenState;
		}
		double num = GameManager.Instance.World.worldTime / 20.0;
		double num2 = 0.0;
		int num3 = 0;
		int num4 = 0;
		ref TokenManager.TokenState ptr = ref entityTokenState.TokenStates[(int)tokenType];
		bool flag = false;
		TokenManager.TokenRequester tokenRequester = null;
		int count = ptr.Requesters.Count;
		for (int i = 0; i < count; i++)
		{
			TokenManager.TokenRequester tokenRequester2 = ptr.Requesters.Dequeue();
			if (!(tokenRequester2.Entity == null))
			{
				if (tokenRequester2.WorldTimeExpireRequest > num)
				{
					num3++;
					num2 += num - tokenRequester2.WorldTimeLastToken;
				}
				bool flag2 = ptr.Claims.Contains(tokenRequester2);
				if (flag2)
				{
					num4++;
				}
				if (tokenRequester == null && tokenRequester2.Entity == claimer)
				{
					flag = flag2;
					if ((claimTokenFlags & AIClaimTokenFlags.UpdateTimeout) != AIClaimTokenFlags.None)
					{
						tokenRequester2.WorldTimeExpireRequest = num + 0.5;
					}
					tokenRequester = tokenRequester2;
					ptr.Requesters.Enqueue(tokenRequester2);
				}
				else
				{
					ptr.Requesters.Enqueue(tokenRequester2);
				}
			}
		}
		if (tokenRequester == null)
		{
			tokenRequester = new TokenManager.TokenRequester
			{
				Entity = claimer,
				WorldTimeExpireRequest = num + 0.5
			};
			ptr.Requesters.Enqueue(tokenRequester);
		}
		if (flag)
		{
			return AITokenClaim.AlreadyClaimed;
		}
		if (num4 < ptr.MaxClaims)
		{
			double num5 = (num - tokenRequester.WorldTimeLastToken) / num2;
			if ((double)Random.value <= num5)
			{
				tokenRequester.Entity = claimer;
				tokenRequester.WorldTimeExpireClaim = num + (double)claimTimeout;
				ptr.Claims.Add(tokenRequester);
				return AITokenClaim.Claimed;
			}
		}
		return AITokenClaim.NoneAvailable;
	}

	public void ReleaseClaimToken(AITokenType tokenType, EntityAlive claimer, EntityAlive target)
	{
		TokenManager.EntityTokenState entityTokenState;
		if (this.EntityTokenStates.TryGetValue(target, out entityTokenState))
		{
			ref TokenManager.TokenState ptr = ref entityTokenState.TokenStates[(int)tokenType];
			foreach (TokenManager.TokenRequester tokenRequester in ptr.Requesters)
			{
				if (tokenRequester.Entity == claimer)
				{
					tokenRequester.WorldTimeExpireClaim = 0.0;
					tokenRequester.WorldTimeLastToken = GameManager.Instance.World.worldTime / 20.0;
					ptr.Claims.Remove(tokenRequester);
				}
			}
		}
	}

	public bool HasClaimToken(AITokenType tokenType, EntityAlive claimer, EntityAlive target)
	{
		TokenManager.EntityTokenState entityTokenState;
		if (this.EntityTokenStates.TryGetValue(target, out entityTokenState))
		{
			ref TokenManager.TokenState ptr = ref entityTokenState.TokenStates[(int)tokenType];
			foreach (TokenManager.TokenRequester tokenRequester in ptr.Requesters)
			{
				if (tokenRequester.Entity == claimer)
				{
					return ptr.Claims.Contains(tokenRequester);
				}
			}
			return false;
		}
		return false;
	}

	public static TokenManager Instance
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static void Init()
	{
		TokenManager.Instance = new TokenManager();
		TokenManager.Instance.Load();
	}

	public static void Cleanup()
	{
		TokenManager instance = TokenManager.Instance;
		if (instance != null)
		{
			instance.EntityTokenStates.Clear();
		}
		TokenManager.Instance = null;
	}

	public void Clear()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Load()
	{
	}

	public void Update()
	{
		using (new ProfilerScope("TokenManager"))
		{
			double num = GameManager.Instance.World.worldTime / 20.0;
			TokenManager.deadEntities.Clear();
			foreach (KeyValuePair<EntityAlive, TokenManager.EntityTokenState> keyValuePair in this.EntityTokenStates)
			{
				if (keyValuePair.Key == null)
				{
					TokenManager.deadEntities.Push(keyValuePair.Key);
				}
				else
				{
					for (int i = 0; i < keyValuePair.Value.TokenStates.Length; i++)
					{
						ref TokenManager.TokenState ptr = ref keyValuePair.Value.TokenStates[i];
						TokenManager.deadRequesters.Clear();
						foreach (TokenManager.TokenRequester tokenRequester in ptr.Claims)
						{
							if (tokenRequester.WorldTimeExpireClaim < num)
							{
								TokenManager.deadRequesters.Add(tokenRequester);
							}
							else
							{
								tokenRequester.WorldTimeLastToken = num;
							}
						}
						if (TokenManager.deadRequesters.Count > 0)
						{
							ptr.Claims.RemoveWhere((TokenManager.TokenRequester x) => TokenManager.deadRequesters.Contains(x));
						}
					}
				}
			}
			EntityAlive entityAlive;
			while (TokenManager.deadEntities.TryPop(out entityAlive))
			{
				this.EntityTokenStates.Remove(entityAlive);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Stack<EntityAlive> deadEntities = new Stack<EntityAlive>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<TokenManager.TokenRequester> deadRequesters = new List<TokenManager.TokenRequester>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<EntityAlive, TokenManager.EntityTokenState> EntityTokenStates = new Dictionary<EntityAlive, TokenManager.EntityTokenState>();

	public class TokenRequester
	{
		public EntityAlive Entity;

		public double WorldTimeExpireRequest;

		public double WorldTimeExpireClaim;

		public double WorldTimeLastToken;
	}

	public struct TokenState
	{
		public TokenState(int maxClaims)
		{
			this.MaxClaims = maxClaims;
			this.Claims = new HashSet<TokenManager.TokenRequester>();
			this.Requesters = new Queue<TokenManager.TokenRequester>();
		}

		public readonly int MaxClaims;

		public readonly HashSet<TokenManager.TokenRequester> Claims;

		public readonly Queue<TokenManager.TokenRequester> Requesters;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class EntityTokenState
	{
		public EntityTokenState(EntityAlive entity)
		{
			this.Entity = entity;
			Dictionary<AITokenType, AITokenConfig> tokenManagerConfig = this.Entity.EntityClass.TokenManagerConfig;
			for (int i = 0; i < this.TokenStates.Length; i++)
			{
				AITokenConfig aitokenConfig;
				if (tokenManagerConfig != null && tokenManagerConfig.TryGetValue((AITokenType)i, out aitokenConfig))
				{
					this.TokenStates[i] = new TokenManager.TokenState(aitokenConfig.MaxClaims);
				}
				else
				{
					this.TokenStates[i] = new TokenManager.TokenState(0);
				}
			}
		}

		public readonly EntityAlive Entity;

		public readonly TokenManager.TokenState[] TokenStates = new TokenManager.TokenState[Enum.GetValues(typeof(AITokenType)).Length];
	}
}
