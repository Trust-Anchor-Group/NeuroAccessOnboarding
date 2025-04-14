using Paiwise;
using System.Text;
using Waher.Events;
using Waher.Events.Console;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;
using Waher.Runtime.Settings;
using Waher.Script;

namespace TAG.Identity.NeuroAccess.Test
{
	[TestClass]
	public sealed class NeuroAccessOnboardingTests
	{
		private static FilesProvider? filesProvider = null;
		private static ConsoleEventSink? consoleEventSink = null;

		[AssemblyInitialize]
		public static async Task AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(NeuroAccessOnboardingTests).Assembly,
				typeof(IIdentityApplication).Assembly,
				typeof(NeuroAccessAuthenticator).Assembly,
				typeof(Database).Assembly,
				typeof(FilesProvider).Assembly,
				typeof(ObjectSerializer).Assembly,
				typeof(RuntimeSettings).Assembly,
				typeof(Expression).Assembly);

			Log.Register(consoleEventSink = new ConsoleEventSink(false));

			if (!Database.HasProvider)
			{
				filesProvider = await FilesProvider.CreateAsync("Data", "Default", 8192, 1000, 8192, Encoding.UTF8, 10000, true);
				Database.Register(filesProvider);
			}

			Assert.IsTrue(await Types.StartAllModules(60000));
		}

		[AssemblyCleanup]
		public static async Task AssemblyCleanup()
		{
			await Types.StopAllModules();

			if (filesProvider is not null)
			{
				await filesProvider.DisposeAsync();
				filesProvider = null;
			}

			if (consoleEventSink is not null)
			{
				Log.Unregister(consoleEventSink);
				consoleEventSink = null;
			}
		}

		[TestMethod]
		public void Test_01_FindService()
		{
			KeyValuePair<string, object>[] Claims =
			[
				new("COUNTRY", "BR"),
				new("PHONE", "+155512345678"),
				new("EMAIL", "test@example.com"),
				new("JID", "test@example.com"),
			];
			PersonalInformation PI = new PersonalInformation()
			{
				Country = "BR",
				Phone = "+155512345678",
				EMail = "test@example.com",
				Jid = "test@example.com"
			};
			IIdentityApplication Application = new IdentityApplication(string.Empty, string.Empty, PI, Claims, []);
			IIdentityAuthenticatorService Authenticator = Types.FindBest<IIdentityAuthenticatorService, IIdentityApplication>(
				Application);
			
			Assert.IsNotNull(Authenticator);
			Assert.IsTrue(Authenticator is NeuroAccessAuthenticator);
		}

		[TestMethod]
		public async Task Test_02_ValidateApplication()
		{
			KeyValuePair<string, object>[] Claims =
			[
				new("COUNTRY", "BR"),
				new("PHONE", "+155512345678"),
				new("EMAIL", "test@example.com"),
				new("JID", "test@example.com"),
			];
			PersonalInformation PI = new PersonalInformation()
			{
				Country = "BR",
				Phone = "+155512345678",
				EMail = "test@example.com",
				Jid = "test@example.com"
			};
			IIdentityApplication Application = new IdentityApplication(string.Empty, string.Empty, PI, Claims, []);
			IIdentityAuthenticatorService Authenticator = Types.FindBest<IIdentityAuthenticatorService, IIdentityApplication>(
				Application);

			Assert.IsNotNull(Authenticator);
			Assert.IsTrue(Authenticator is NeuroAccessAuthenticator);

			await Authenticator.Validate(Application);

			Assert.IsTrue(Application.IsValid.HasValue);
			Assert.IsTrue(Application.IsValid.Value);
		}
	}
}
