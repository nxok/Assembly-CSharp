using System;

public class ParentControllerState
{
	public ParentControllerState(XUiController parentController)
	{
		if (parentController == null)
		{
			return;
		}
		if (parentController.ViewComponent == null)
		{
			this.m_parentControllers = new XUiController[parentController.Children.Count];
			for (int i = 0; i < parentController.Children.Count; i++)
			{
				this.m_parentControllers[i] = parentController.Children[i];
			}
		}
		else
		{
			this.m_parentControllers = new XUiController[] { parentController };
		}
		this.m_isVisible = new bool[this.m_parentControllers.Length];
		this.m_isEscClosable = new bool[this.m_parentControllers.Length];
		for (int j = 0; j < this.m_parentControllers.Length; j++)
		{
			this.m_isVisible[j] = this.m_parentControllers[j].ViewComponent.IsVisible;
			this.m_isEscClosable[j] = this.m_parentControllers[j].WindowGroup.isEscClosable;
		}
	}

	public void Hide()
	{
		if (this.m_parentControllers == null)
		{
			return;
		}
		for (int i = 0; i < this.m_parentControllers.Length; i++)
		{
			this.m_parentControllers[i].ViewComponent.IsVisible = false;
			this.m_parentControllers[i].WindowGroup.isEscClosable = false;
		}
	}

	public void Restore()
	{
		if (this.m_parentControllers == null)
		{
			return;
		}
		for (int i = 0; i < this.m_parentControllers.Length; i++)
		{
			this.m_parentControllers[i].ViewComponent.IsVisible = this.m_isVisible[i];
			this.m_parentControllers[i].WindowGroup.isEscClosable = this.m_isEscClosable[i];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiController[] m_parentControllers;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool[] m_isVisible;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool[] m_isEscClosable;
}
