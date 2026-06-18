using System;
using System.Collections;
using System.Globalization;
using System.Xml.Linq;

public class DialogFromXml
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		Dialog.DialogList.Clear();
		DialogFromXml.CreateDialogs(_xmlFile);
		yield break;
	}

	public static void Reload()
	{
	}

	public static bool CreateDialogs(XmlFile xmlFile)
	{
		Dialog.DialogList.Clear();
		XElement root = xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <dialogs> found!");
		}
		DialogFromXml.ParseNode(root);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseNode(XElement root)
	{
		foreach (XElement xelement in root.Elements("dialog"))
		{
			Dialog dialog = DialogFromXml.ParseDialog(xelement);
			Dialog.DialogList.Add(dialog.ID, dialog);
		}
	}

	public static Dialog ParseDialog(XElement e)
	{
		if (!e.HasAttribute("id"))
		{
			throw new Exception("quest must have an id attribute");
		}
		string attribute = e.GetAttribute("id");
		if (Dialog.DialogList.ContainsKey(attribute))
		{
			throw new Exception("Duplicate dialog entry with id " + attribute);
		}
		Dialog dialog = new Dialog(attribute);
		if (e.HasAttribute("startstatementid"))
		{
			dialog.StartStatementID = e.GetAttribute("startstatementid");
		}
		if (e.HasAttribute("startresponseid"))
		{
			dialog.StartResponseID = e.GetAttribute("startresponseid");
		}
		foreach (XElement xelement in e.Elements())
		{
			if (xelement.Name == "phase")
			{
				DialogFromXml.ParsePhase(dialog, xelement);
			}
			else if (xelement.Name == "statement")
			{
				DialogFromXml.ParseStatement(dialog, xelement);
			}
			else
			{
				if (!(xelement.Name == "response"))
				{
					string text = "Unrecognized xml element ";
					XName name = xelement.Name;
					throw new Exception(text + ((name != null) ? name.ToString() : null));
				}
				DialogFromXml.ParseResponse(dialog, xelement);
			}
		}
		return dialog;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParsePhase(Dialog dialog, XElement e)
	{
		DialogPhase dialogPhase = new DialogPhase(e.GetAttribute("id"));
		dialog.Phases.Add(dialogPhase);
		dialogPhase.OwnerDialog = dialog;
		if (e.HasAttribute("startstatementid"))
		{
			dialogPhase.StartStatementID = e.GetAttribute("startstatementid");
		}
		if (e.HasAttribute("startresponseid"))
		{
			dialogPhase.StartResponseID = e.GetAttribute("startresponseid");
		}
		foreach (XElement xelement in e.Elements("requirement"))
		{
			BaseDialogRequirement baseDialogRequirement = DialogFromXml.ParseRequirement(xelement);
			if (baseDialogRequirement != null)
			{
				dialogPhase.AddRequirement(baseDialogRequirement);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseStatement(Dialog dialog, XElement e)
	{
		DialogStatement dialogStatement = null;
		if (e.HasAttribute("id"))
		{
			dialogStatement = new DialogStatement(e.GetAttribute("id"));
			dialog.Statements.Add(dialogStatement);
			dialogStatement.OwnerDialog = dialog;
		}
		if (e.HasAttribute("nextstatementid"))
		{
			dialogStatement.NextStatementID = e.GetAttribute("nextstatementid");
		}
		if (e.HasAttribute("text"))
		{
			dialogStatement.Text = Localization.Get(e.GetAttribute("text"), false, null);
		}
		foreach (XElement xelement in e.Elements())
		{
			if (xelement.Name == "response_entry")
			{
				DialogResponseEntry dialogResponseEntry = null;
				if (xelement.HasAttribute("id"))
				{
					dialogResponseEntry = new DialogResponseEntry(xelement.GetAttribute("id"));
					dialogStatement.ResponseEntries.Add(dialogResponseEntry);
				}
				if (dialogResponseEntry != null && xelement.HasAttribute("uniqueid"))
				{
					dialogResponseEntry.UniqueID = xelement.GetAttribute("uniqueid");
				}
			}
			else if (xelement.Name == "quest_entry")
			{
				string text = "";
				string text2 = "";
				string text3 = "";
				int num = -1;
				int num2 = -1;
				if (xelement.HasAttribute("id"))
				{
					text = xelement.GetAttribute("id");
				}
				if (xelement.HasAttribute("type"))
				{
					text3 = xelement.GetAttribute("type");
				}
				if (xelement.HasAttribute("nextstatementid"))
				{
					text2 = xelement.GetAttribute("nextstatementid");
				}
				if (xelement.HasAttribute("returnstatementid"))
				{
					text2 = xelement.GetAttribute("returnstatementid");
				}
				if (xelement.HasAttribute("tier"))
				{
					num2 = StringParsers.ParseSInt32(xelement.GetAttribute("tier"), 0, -1, NumberStyles.Integer);
				}
				if (xelement.HasAttribute("listindex"))
				{
					num = Convert.ToInt32(xelement.GetAttribute("listindex"));
				}
				dialogStatement.ResponseEntries.Add(new DialogQuestResponseEntry(text, text3, text2, num, num2));
			}
			else if (xelement.Name == "action")
			{
				BaseDialogAction baseDialogAction = DialogFromXml.ParseAction(xelement);
				if (baseDialogAction != null)
				{
					dialogStatement.AddAction(baseDialogAction);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseResponse(Dialog dialog, XElement e)
	{
		DialogResponse dialogResponse = null;
		if (e.HasAttribute("id"))
		{
			dialogResponse = new DialogResponse(e.GetAttribute("id"));
			dialog.Responses.Add(dialogResponse);
			dialogResponse.OwnerDialog = dialog;
		}
		if (e.HasAttribute("nextstatementid"))
		{
			dialogResponse.NextStatementID = e.GetAttribute("nextstatementid");
		}
		if (e.HasAttribute("text"))
		{
			dialogResponse.Text = Localization.Get(e.GetAttribute("text"), false, null);
		}
		if (e.HasAttribute("returnstatementid"))
		{
			dialogResponse.ReturnStatementID = e.GetAttribute("returnstatementid");
		}
		foreach (XElement xelement in e.Elements())
		{
			if (xelement.Name == "requirement")
			{
				BaseDialogRequirement baseDialogRequirement = DialogFromXml.ParseRequirement(xelement);
				if (baseDialogRequirement != null)
				{
					dialogResponse.AddRequirement(baseDialogRequirement);
				}
			}
			if (xelement.Name == "action")
			{
				BaseDialogAction baseDialogAction = DialogFromXml.ParseAction(xelement);
				if (baseDialogAction != null)
				{
					dialogResponse.AddAction(baseDialogAction);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BaseDialogAction ParseAction(XElement e)
	{
		if (!e.HasAttribute("type"))
		{
			throw new Exception("Dialog Action must have a type!");
		}
		BaseDialogAction baseDialogAction = null;
		string attribute = e.GetAttribute("type");
		try
		{
			baseDialogAction = (BaseDialogAction)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("DialogAction", attribute));
		}
		catch (Exception)
		{
			throw new Exception("No action class '" + attribute + " found!");
		}
		if (e.HasAttribute("id"))
		{
			baseDialogAction.ID = e.GetAttribute("id");
		}
		if (e.HasAttribute("value"))
		{
			baseDialogAction.Value = e.GetAttribute("value");
		}
		return baseDialogAction;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BaseDialogRequirement ParseRequirement(XElement e)
	{
		if (!e.HasAttribute("type"))
		{
			throw new Exception("Dialog Action must have a type!");
		}
		BaseDialogRequirement baseDialogRequirement = null;
		string attribute = e.GetAttribute("type");
		try
		{
			baseDialogRequirement = (BaseDialogRequirement)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("DialogRequirement", attribute));
		}
		catch (Exception)
		{
			throw new Exception("No action class '" + attribute + " found!");
		}
		if (e.HasAttribute("id"))
		{
			baseDialogRequirement.ID = e.GetAttribute("id");
		}
		if (e.HasAttribute("value"))
		{
			baseDialogRequirement.Value = e.GetAttribute("value");
		}
		if (e.HasAttribute("tag"))
		{
			baseDialogRequirement.Tag = e.GetAttribute("tag");
		}
		else
		{
			baseDialogRequirement.Tag = "";
		}
		if (e.HasAttribute("requirementtype"))
		{
			baseDialogRequirement.RequirementVisibilityType = EnumUtils.Parse<BaseDialogRequirement.RequirementVisibilityTypes>(e.GetAttribute("requirementtype"), false);
		}
		return baseDialogRequirement;
	}
}
