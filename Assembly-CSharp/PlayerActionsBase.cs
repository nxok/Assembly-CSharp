using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using InControl;

public abstract class PlayerActionsBase : PlayerActionSet
{
	public string Name
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public PlayerActionsBase()
	{
		FieldInfo field = typeof(PlayerActionSet).GetField("actionsByName", BindingFlags.Instance | BindingFlags.NonPublic);
		if (field != null)
		{
			field.SetValue(this, new CaseInsensitiveStringDictionary<PlayerAction>());
		}
		base.ListenOptions = new BindingListenOptions
		{
			UnsetDuplicateBindingsOnSet = false,
			MaxAllowedBindings = 0U,
			MaxAllowedBindingsPerType = 1U,
			AllowDuplicateBindingsPerSet = true,
			IncludeKeys = false,
			IncludeMouseButtons = false,
			IncludeControllers = false,
			IncludeModifiersAsFirstClassKeys = true
		};
		base.ListenOptions.OnBindingFound = delegate(PlayerAction _action, BindingSource _binding)
		{
			if (!_action.HasBinding(_binding))
			{
				return true;
			}
			Log.Out("Binding already bound.");
			_action.StopListeningForBinding();
			return false;
		};
		BindingListenOptions listenOptions = base.ListenOptions;
		listenOptions.OnBindingAdded = (Action<PlayerAction, BindingSource>)Delegate.Combine(listenOptions.OnBindingAdded, new Action<PlayerAction, BindingSource>(delegate(PlayerAction _action, BindingSource _binding)
		{
			Log.Out("Binding added for action {0} on device {1}: {2}", new object[] { _action.Name, _binding.DeviceName, _binding.Name });
		}));
		BindingListenOptions listenOptions2 = base.ListenOptions;
		listenOptions2.OnBindingRejected = (Action<PlayerAction, BindingSource, BindingSourceRejectionType>)Delegate.Combine(listenOptions2.OnBindingRejected, new Action<PlayerAction, BindingSource, BindingSourceRejectionType>(delegate(PlayerAction _action, BindingSource _binding, BindingSourceRejectionType _reason)
		{
			Log.Out("Binding rejected for action {0}: {1}", new object[]
			{
				_action.Name,
				_reason.ToStringCached<BindingSourceRejectionType>()
			});
		}));
		this.InitActionSet();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitActionSet()
	{
		this.CreateActions();
		this.CreateDefaultKeyboardBindings();
		this.CreateDefaultJoystickBindings();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void CreateActions();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void CreateDefaultKeyboardBindings();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void CreateDefaultJoystickBindings();

	public void ResetControllerBindings()
	{
		this.AsyncResetControllerBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public async void AsyncResetControllerBindings()
	{
		foreach (PlayerAction playerAction in base.Actions)
		{
			PlayerActionData.ActionUserData actionUserData = playerAction.UserData as PlayerActionData.ActionUserData;
			if (actionUserData != null && actionUserData.defaultOnStartup)
			{
				foreach (BindingSource bindingSource in playerAction.Bindings)
				{
					if (bindingSource.BindingSourceType == 1)
					{
						playerAction.RemoveBinding(bindingSource);
					}
				}
			}
		}
		await Task.Yield();
		this.CreateDefaultJoystickBindings();
	}

	public List<PlayerAction> ControllerRebindableActions = new List<PlayerAction>();
}
