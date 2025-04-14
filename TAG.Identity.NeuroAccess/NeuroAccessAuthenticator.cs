using Paiwise;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.IoTGateway;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;
using Waher.Runtime.Settings;
using Waher.Script;

namespace TAG.Identity.NeuroAccess
{
	/// <summary>
	/// Service that authenticates Neuro-Access digital identity applications.
	/// </summary>
	public class NeuroAccessAuthenticator : IIdentityAuthenticatorService, IModule
	{
		/// <summary>
		/// Prefix for settings keys
		/// </summary>
		public static readonly string Prefix = typeof(NeuroAccessAuthenticator).Namespace;

		/// <summary>
		/// Key for Onboarding neuron setting parameter.
		/// </summary>
		public static readonly string OnboardingNeuronKey = "Onboarding.DomainName";

		/// <summary>
		/// Onboarding Neuron host.
		/// </summary>
		private static string onboardingNeuron = null;

		/// <summary>
		/// Service that authenticates Neuro-Access digital identity applications.
		/// </summary>
		public NeuroAccessAuthenticator()
		{
		}

		#region IModule

		/// <summary>
		/// Is called when the module is being started.
		/// </summary>
		public Task Start()
		{
			return InvalidateCurrent();
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
		public static async Task InvalidateCurrent()
		{
			onboardingNeuron = await RuntimeSettings.GetAsync(OnboardingNeuronKey, "id.tagroot.io");
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

			bool HasEMail = !string.IsNullOrEmpty(Application.PersonalInformation.EMail);
			bool HasPhoneNr = !string.IsNullOrEmpty(Application.PersonalInformation.Phone);
			bool HasJid = !string.IsNullOrEmpty(Application.PersonalInformation.Jid);
			bool HasCountry = !string.IsNullOrEmpty(Application.PersonalInformation.Country);

			if ((HasEMail || HasPhoneNr) && HasJid && HasCountry)
				return Grade.Perfect;
			else
				return Grade.NotAtAll;
		}

		/// <summary>
		/// Validates an identity application.
		/// </summary>
		/// <param name="Application">Identity application.</param>
		public async Task Validate(IIdentityApplication Application)
		{
			if (string.IsNullOrEmpty(onboardingNeuron))
			{
				Application.ReportError("Service not configured correctly. Please contact operator.",
					"en", "ServiceNotConfigured", ValidationErrorType.Service, this);
				return;
			}

			string Jid = Application.PersonalInformation.Jid;
			int i = Jid.IndexOf('@');
			if (i < 0)
			{
				Application.ReportError("Invalid JID.", "en", "InvalidJid", ValidationErrorType.Client, this);
				return;
			}

			string Domain = Jid[(i + 1)..];
			if (!Gateway.IsDomain(Domain, true))
			{
				Application.ReportError("Invalid JID.", "en", "InvalidJid", ValidationErrorType.Client, this);
				return;
			}

			string AccountName = Jid[..i];
			GenericObject LastLogin = null;

			foreach (GenericObject Obj in await Database.Find<GenericObject>("BrokerAccountLogins", 0, 1, new FilterFieldEqualTo("UserName", AccountName)))
			{
				LastLogin = Obj;
				break;
			}

			if (LastLogin is null)
			{
				Application.ReportError("No login registered on Neuron.", "en", "NoLogin", ValidationErrorType.Client, this);
				return;
			}

			if (!LastLogin.TryGetFieldValue("RemoteEndPoint", out object Obj2) || !(Obj2 is string RemoteEndPoint))
			{
				Application.ReportError("No login registered on Neuron.", "en", "NoLogin", ValidationErrorType.Client, this);
				return;
			}

			if (string.IsNullOrEmpty(Application.PersonalInformation.Country))
			{
				Application.ReportError("Application does not contain country information.", "en", "MissingCountry", ValidationErrorType.Client, this);
				return;
			}

			if (Application.PersonalInformation.Country.Length != 2)
			{
				Application.ClaimInvalid("COUNTRY", "Service not available in your country.", "en", "CountryNotSupported", this);
				return;
			}

			Dictionary<string, object> Request = new Dictionary<string, object>()
			{
				{ "RemoteEndPoint", RemoteEndPoint }
			};

			if (!string.IsNullOrEmpty(Application.PersonalInformation.EMail))
				Request["EMail"] = Application.PersonalInformation.EMail;

			if (!string.IsNullOrEmpty(Application.PersonalInformation.Phone))
				Request["Nr"] = Application.PersonalInformation.Phone;

			if (!string.IsNullOrEmpty(Application.PersonalInformation.Country))
				Request["Country"] = Application.PersonalInformation.Country;

			try
			{
				ContentResponse Content = await InternetContent.PostAsync(new Uri("https://" + onboardingNeuron + "/ID/ValidateOnboarding.ws"), Request,
					Gateway.Certificate, 10000, new KeyValuePair<string, string>("Accept", "application/json"));
				Content.AssertOk();

				if (!(Content.Decoded is bool Result))
				{
					Application.ReportError("Unexpected response received from onboarding server.", 
						"en", "UnexpectedOnboardingServer", ValidationErrorType.Server, this);
					return;
				}

				if (!Result)
				{
					Application.ClaimInvalid("EMAIL", "EMail or Phone Number invalid.", "en", "EMailOrPhoneInvalid", this);
					Application.ClaimInvalid("PHONE", "EMail or Phone Number invalid.", "en", "EMailOrPhoneInvalid", this);
					return;
				}
			}
			catch (Exception ex)
			{
				Application.ReportError(ex.Message, "en", null, ValidationErrorType.Server, this);
				return;
			}

			try
			{
				await Expression.EvalAsync(
					"Account:=Waher.Service.IoTBroker.XmppServerModule.GetAccountAsync(" + Expression.ToString(AccountName) + ");" +
					"Account.EMail:=" + Expression.ToString(Application.PersonalInformation.EMail) + ";" +
					"UpdateObject(Account)", new Variables());
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				Application.ReportError(ex.Message, "en", null, ValidationErrorType.Server, this);
			}
		}

		#endregion
	}
}
