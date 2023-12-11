using Paiwise;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.IoTGateway;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;
using Waher.Runtime.Settings;

namespace TAG.Identity.NeuroAccess
{
	/// <summary>
	/// Service that authenticates Neuro-Access digital identity applications.
	/// </summary>
	public class NeuroAccessAuthenticator : IIdentityAuthenticatorService
	{
		/// <summary>
		/// Service that authenticates Neuro-Access digital identity applications.
		/// </summary>
		public NeuroAccessAuthenticator()
		{
		}

		#region IModule

		/// <summary>
		/// Prefix for settings keys
		/// </summary>
		public static readonly string Prefix = typeof(NeuroAccessAuthenticator).Namespace;

		/// <summary>
		/// Key for Onboarding neuron setting parameter.
		/// </summary>
		public static readonly string OnboardingNeuronKey = Prefix + ".OnboardingNeuron";

		/// <summary>
		/// Onboarding Neuron host.
		/// </summary>
		private static string onboardingNeuron = null;

		/// <summary>
		/// Is called when the module is being started.
		/// </summary>
		public Task Start()
		{
			return LoadConfiguration();
		}

		/// <summary>
		/// Is called when the module is being stopped.
		/// </summary>
		public Task Stop()
		{
			onboardingNeuron = null;
			return Task.CompletedTask;
		}

		#endregion

		#region IIdentityAuthenticatorService

		/// <summary>
		/// Loads service configuration.
		/// </summary>
		public static async Task LoadConfiguration()
		{
			onboardingNeuron = await RuntimeSettings.GetAsync(OnboardingNeuronKey, string.Empty);
		}

		/// <summary>
		/// Checks if the service supports the authentication of an identity application.
		/// </summary>
		/// <param name="Application">Identity application</param>
		/// <returns>How well the application is handled by the service.</returns>
		public Grade Supports(IIdentityApplication Application)
		{
			if (string.IsNullOrWhiteSpace(onboardingNeuron) || Application.NrPhotos != 0)
				return Grade.NotAtAll;

			bool HasEMail = false;
			bool HasPhoneNr = false;
			bool HasJid = false;

			foreach (KeyValuePair<string, object> P in Application.Claims)
			{
				switch (P.Key)
				{
					case "EMAIL":
						HasEMail = true;
						break;

					case "PHONE":
						HasPhoneNr = true;
						break;

					case "JID":
						HasJid = true;
						break;

					default:
						return Grade.NotAtAll;
				}
			}

			return (HasEMail || HasPhoneNr) && HasJid ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// Authenticates an identity application.
		/// </summary>
		/// <param name="Identity">Meta-information in the application.</param>
		/// <param name="Photos">Photos in the application.</param>
		/// <returns>Authentication result.</returns>
		public async Task<IAuthenticationResult> IsValid(KeyValuePair<string, object>[] Identity, IEnumerable<IPhoto> Photos)
		{
			if (string.IsNullOrEmpty(onboardingNeuron))
				return new AuthenticationResult(ErrorType.Service, "Service not configured.");

			foreach (IPhoto _ in Photos)
				return new AuthenticationResult(ErrorType.Service, "Applications with photos cannot be authenticated using this service.");

			string EMail = null;
			string PhoneNr = null;
			string Jid = null;

			foreach (KeyValuePair<string, object> P in Identity)
			{
				if (!(P.Value is string s))
					return new AuthenticationResult(false);

				switch (P.Key)
				{
					case "EMAIL":
						EMail = s;
						break;

					case "PHONE":
						PhoneNr = s;
						break;

					case "JID":
						Jid = s;
						break;

					default:
						return new AuthenticationResult(false);
				}
			}

			if (string.IsNullOrEmpty(Jid) || (string.IsNullOrEmpty(EMail) && string.IsNullOrEmpty(PhoneNr)))
				return new AuthenticationResult(false);

			int i = Jid.IndexOf('@');
			if (i < 0)
				return new AuthenticationResult(false);

			string Domain = Jid.Substring(i + 1);
			if (!Gateway.IsDomain(Domain, true))
				return new AuthenticationResult(false);

			string Account = Jid.Substring(0, i);
			GenericObject LastLogin = null;

			foreach (GenericObject Obj in await Database.Find<GenericObject>("BrokerAccountLogins", 0, 1, new FilterFieldEqualTo("UserName", Account)))
			{
				LastLogin = Obj;
				break;
			}

			if (LastLogin is null)
				return new AuthenticationResult(false);

			if (!LastLogin.TryGetFieldValue("RemoteEndpoint", out object Obj2) || !(Obj2 is string RemoteEndPoint))
				return new AuthenticationResult(false);

			Dictionary<string, object> Request = new Dictionary<string, object>()
			{
				{ "RemoteEndPoint", RemoteEndPoint }
			};

			if (!string.IsNullOrEmpty(EMail))
				Request["EMail"] = EMail;

			if (!string.IsNullOrEmpty(PhoneNr))
				Request["Nr"] = PhoneNr;

			try
			{
				object Obj = await InternetContent.PostAsync(new Uri("https://" + onboardingNeuron + "/ID/ValidateOnboarding.ws"), Request,
					Gateway.Certificate, 10000, new KeyValuePair<string, string>("Accept", "application/json"));

				if (!(Obj is bool Result))
					return new AuthenticationResult(ErrorType.Server, "Unexpected response received from onboarding server.");

				return new AuthenticationResult(Result);
			}
			catch (Exception ex)
			{
				return new AuthenticationResult(ErrorType.Server, ex.Message);
			}
		}

		#endregion
	}
}
