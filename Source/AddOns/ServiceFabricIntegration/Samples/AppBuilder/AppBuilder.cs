﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;
using Microsoft.PSharp.ReliableServices;
using Microsoft.PSharp.ReliableServices.Utilities;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System.Runtime.Serialization;

namespace AppBuilder
{
	#region events

	/// <summary>
	/// Issued by user to register himself/herself with AppBuilder.
	/// </summary>
	[DataContract]
	class UserRegisterEvent : Event
	{
		[DataMember]
		public MachineId user;

		public UserRegisterEvent(MachineId user)
		{
			this.user = user;
		}
	}

	/// <summary>
	/// AppBuilder sends back the public key of new user on successful registration.
	/// </summary>
	[DataContract]
	class UserRegisterResponseEvent : Event
	{
		[DataMember]
		public int publicKey;

		public UserRegisterResponseEvent(int publicKey)
		{
			this.publicKey = publicKey;
		}
	}

	#endregion

	class AppBuilder : ReliableStateMachine
	{
		#region fields

		/// <summary>
		/// Handle to the AzureKeyVault mock
		/// </summary>
		ReliableRegister<MachineId> AzureKeyVaultMachine;

		#endregion

		#region states
		[Start]
		[OnEntry(nameof(Initialize))]
		[OnEventDoAction(typeof(UserRegisterEvent), nameof(RequestRegistration))]
		[OnEventDoAction(typeof(RegistrationResponseEvent), nameof(CompleteRegistration))]
		
		class Init : MachineState { }
		#endregion

		#region handlers

		private async Task Initialize()
		{
			MachineId keyVaultMachine = await this.ReliableCreateMachine(typeof(AzureKeyVaultMock), null, new AzureKeyVaultInitEvent(this.Id));
			await AzureKeyVaultMachine.Set(CurrentTransaction, keyVaultMachine);
		}

		/// <summary>
		/// Raise a registration request with AzureKeyVault
		/// </summary>
		private async Task RequestRegistration()
		{
			UserRegisterEvent e = this.ReceivedEvent as UserRegisterEvent;
			await this.ReliableSend(await AzureKeyVaultMachine.Get(CurrentTransaction),
				new RegisterNewUserEvent(e.user));
		}

		private async Task CompleteRegistration()
		{
			RegistrationResponseEvent e = this.ReceivedEvent as RegistrationResponseEvent;

			await this.ReliableSend(e.user, new UserRegisterResponseEvent(e.response));
		}
		#endregion

		#region methods
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="stateManager"></param>
		public AppBuilder(IReliableStateManager stateManager) : base(stateManager) { }

		/// <summary>
		/// Initialize the reliable fields.
		/// </summary>
		/// <returns></returns>
		public override Task OnActivate()
		{
			this.Logger.WriteLine("AppBuilder starting.");

			AzureKeyVaultMachine = new ReliableRegister<MachineId>(QualifyWithMachineName("KeyVault"), this.StateManager, null);

			return Task.CompletedTask;

		}

		private string QualifyWithMachineName(string name)
		{
			return name + "_" + this.Id.Name;
		}

				#endregion


	}
}
