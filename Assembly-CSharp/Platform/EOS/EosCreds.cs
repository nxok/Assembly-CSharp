using System;

namespace Platform.EOS
{
	public class EosCreds
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public EosCreds(string _productId, string _sandboxId, string _deploymentId, string _clientId, string _clientSecret, bool _serverMode)
		{
			this.ProductId = _productId;
			this.SandboxId = _sandboxId;
			this.DeploymentId = _deploymentId;
			this.ClientId = _clientId;
			this.ClientSecret = _clientSecret;
			this.ServerMode = _serverMode;
		}

		public const string StorageEncKey = "0000000000000000000000000000000000000000000000000000000000000000";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string productId = "85fffb61212b491999cd7fc03eb09bf6";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string sandboxId = "8a44365d5ccb43328b4df2f8ca199e43";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string deploymentId = "c9ccbd00333f4dd6995beb7c75000942";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string deploymentId_Old = "30b9e9e5f58b4f4e82930b3bef76d9e1";

		public static readonly EosCreds ClientCredentials = new EosCreds("85fffb61212b491999cd7fc03eb09bf6", "8a44365d5ccb43328b4df2f8ca199e43", "c9ccbd00333f4dd6995beb7c75000942", "xyza7891WBnGQuvNMiNyg6SYeYOhbA2F", "aopC/pp4xFK643dkeOOktsSiFV1IC5qQiLfJ8EJjPrw", false);

		public static readonly EosCreds ServerCredentials = new EosCreds("85fffb61212b491999cd7fc03eb09bf6", "8a44365d5ccb43328b4df2f8ca199e43", "c9ccbd00333f4dd6995beb7c75000942", "xyza7891nSjSAzYxhnVGWL1xKR4jAL7I", "fkCG6lR19l6KCfXFxxF1dppvCbA76qZT9IO+4eqX5QU", true);

		public static readonly EosCreds ServerDeviceIdCredentials = new EosCreds("85fffb61212b491999cd7fc03eb09bf6", "8a44365d5ccb43328b4df2f8ca199e43", "c9ccbd00333f4dd6995beb7c75000942", "xyza7891UbuRjRa7tG4QlJvZC6l62dwE", "Ah8Ul94L+ybtvVhqJAKPMOKKfuLu+8bPlmoZaPLFbkc", false);

		public readonly string ProductId;

		public readonly string SandboxId;

		public readonly string DeploymentId;

		public readonly string ClientId;

		public readonly string ClientSecret;

		public readonly bool ServerMode;
	}
}
