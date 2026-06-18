using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

public static class EventsFromXml
{
	public static DateTime Now
	{
		get
		{
			if (!(EventsFromXml.ForceTestDateTime != DateTime.MinValue))
			{
				return DateTime.Now;
			}
			return EventsFromXml.ForceTestDateTime;
		}
	}

	public static IEnumerator Load(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (root == null)
		{
			yield break;
		}
		using (IEnumerator<XElement> enumerator = root.Elements("event").GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				XElement xelement = enumerator.Current;
				EventsFromXml.parseEvent(xelement);
			}
			yield break;
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseEvent(XElement _element)
	{
		if (!_element.HasAttribute("name"))
		{
			throw new XmlLoadException("events.xml", _element, "Attribute 'name' missing");
		}
		string attribute = _element.GetAttribute("name");
		DateTime minValue = DateTime.MinValue;
		int num = int.MinValue;
		int minValue2 = int.MinValue;
		DateTime dateTime = DateTime.MinValue;
		int minValue3 = int.MinValue;
		DateTime dateTime2 = DateTime.MinValue;
		string text;
		if (_element.TryGetAttribute("base_date", out text) && !EventsFromXml.TryParseDate(text, out minValue))
		{
			throw new XmlLoadException("events.xml", _element, "Attribute 'base_date' has invalid format");
		}
		string text2;
		if (_element.TryGetAttribute("start_offset", out text2) && !int.TryParse(text2, out num))
		{
			throw new XmlLoadException("events.xml", _element, "Attribute 'start_offset' is not a valid integer number");
		}
		string text3;
		if (_element.TryGetAttribute("end_offset", out text3) && !int.TryParse(text3, out minValue2))
		{
			throw new XmlLoadException("events.xml", _element, "Attribute 'end_offset' is not a valid integer number");
		}
		string text4;
		if (_element.TryGetAttribute("start_date", out text4) && !EventsFromXml.TryParseDate(text4, out dateTime))
		{
			throw new XmlLoadException("events.xml", _element, "Attribute 'start_date' has invalid format");
		}
		string text5;
		if (_element.TryGetAttribute("end_date", out text5) && !EventsFromXml.TryParseDate(text5, out dateTime2))
		{
			throw new XmlLoadException("events.xml", _element, "Attribute 'end_date' has invalid format");
		}
		string text6;
		if (_element.TryGetAttribute("duration", out text6) && (!int.TryParse(text6, out minValue3) || minValue3 < 1))
		{
			throw new XmlLoadException("events.xml", _element, "Attribute 'duration' is not a valid integer number or not greater/equal 1");
		}
		if (num > -2147483648 && minValue == DateTime.MinValue)
		{
			throw new XmlLoadException("events.xml", _element, "Event has 'start_offset' but no 'base_date'");
		}
		if (minValue2 > -2147483648 && minValue == DateTime.MinValue)
		{
			throw new XmlLoadException("events.xml", _element, "Event has 'end_offset' but no 'base_date'");
		}
		if (num > -2147483648 && dateTime != DateTime.MinValue)
		{
			throw new XmlLoadException("events.xml", _element, "Event has both 'start_offset' and 'start_date'");
		}
		if (minValue2 > -2147483648 && dateTime2 != DateTime.MinValue)
		{
			throw new XmlLoadException("events.xml", _element, "Event has both 'end_offset' and 'end_date'");
		}
		if (minValue2 > -2147483648 && minValue3 > -2147483648)
		{
			throw new XmlLoadException("events.xml", _element, "Event has both 'end_offset' and 'duration'");
		}
		if (minValue3 > -2147483648 && dateTime2 != DateTime.MinValue)
		{
			throw new XmlLoadException("events.xml", _element, "Event has both 'duration' and 'end_date'");
		}
		if (dateTime == DateTime.MinValue)
		{
			if (minValue == DateTime.MinValue)
			{
				throw new XmlLoadException("events.xml", _element, "Event has neither 'base_date' nor 'start_date'");
			}
			if (num == -2147483648)
			{
				num = 0;
			}
			dateTime = minValue.AddDays((double)num);
		}
		if (dateTime2 == DateTime.MinValue)
		{
			if (minValue3 == -2147483648 && minValue2 == -2147483648)
			{
				throw new XmlLoadException("events.xml", _element, "Event has neither 'end_offset' nor 'duration' nor 'end_date'");
			}
			if (minValue2 > -2147483648)
			{
				dateTime2 = minValue.AddDays((double)minValue2);
			}
			if (minValue3 > -2147483648)
			{
				dateTime2 = dateTime.AddDays((double)minValue3);
			}
		}
		DateTime now = EventsFromXml.Now;
		if (dateTime2 < dateTime)
		{
			dateTime2 = dateTime2.AddYears(1);
		}
		if (dateTime2.Year > now.Year)
		{
			dateTime = dateTime.AddYears(-1);
			dateTime2 = dateTime2.AddYears(-1);
		}
		if (dateTime < now && dateTime2 < now)
		{
			dateTime = dateTime.AddYears(1);
			dateTime2 = dateTime2.AddYears(1);
		}
		EventsFromXml.EventDefinition eventDefinition = new EventsFromXml.EventDefinition(attribute, dateTime, dateTime2);
		if (!EventsFromXml.Events.TryAdd(attribute, eventDefinition))
		{
			Log.Error("Event with the same name '" + attribute + "' already defined");
		}
	}

	public static bool TryParseDate(string _dateString, out DateTime _date)
	{
		_dateString = _dateString.Trim();
		EventsFromXml.SpecialDateDelegate specialDateDelegate;
		if (EventsFromXml.specialDateHandlers.TryGetValue(_dateString, out specialDateDelegate))
		{
			_date = specialDateDelegate(_dateString);
			return true;
		}
		if (!DateTime.TryParseExact(_dateString, "MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _date))
		{
			return false;
		}
		_date = _date.ChangeYear(DateTime.Now.Year);
		return true;
	}

	public static DateTime ChangeYear(this DateTime _dt, int _newYear)
	{
		return _dt.AddYears(_newYear - _dt.Year);
	}

	public static void Cleanup()
	{
		EventsFromXml.Events.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static EventsFromXml()
	{
		EventsFromXml.specialDateHandlers["easter"] = new EventsFromXml.SpecialDateDelegate(EventsFromXml.EasterDate);
		EventsFromXml.specialDateHandlers["advent"] = new EventsFromXml.SpecialDateDelegate(EventsFromXml.FirstSundayOfAdvent);
		EventsFromXml.specialDateHandlers["thanksgiving"] = new EventsFromXml.SpecialDateDelegate(EventsFromXml.ThanksgivingDate);
	}

	public static DateTime EasterDate(string _origString)
	{
		return EventsFromXml.EasterSunday(DateTime.Now.Year);
	}

	public static DateTime EasterSunday(int _year)
	{
		int num;
		int num2;
		EventsFromXml.EasterSunday(_year, out num, out num2);
		return new DateTime(_year, num, num2);
	}

	public static void EasterSunday(int _year, out int _month, out int _day)
	{
		int num = _year % 19;
		int num2 = _year / 100;
		int num3 = (num2 - num2 / 4 - (8 * num2 + 13) / 25 + 19 * num + 15) % 30;
		int num4 = num3 - num3 / 28 * (1 - num3 / 28 * (29 / (num3 + 1)) * ((21 - num) / 11));
		_day = num4 - (_year + _year / 4 + num4 + 2 - num2 + num2 / 4) % 7 + 28;
		_month = 3;
		if (_day > 31)
		{
			_month++;
			_day -= 31;
		}
	}

	public static DateTime FirstSundayOfAdvent(string _origString)
	{
		return EventsFromXml.FirstSundayOfAdvent(DateTime.Now.Year);
	}

	public static DateTime FirstSundayOfAdvent(int _year)
	{
		int num = 4;
		int num2 = 0;
		DateTime dateTime = new DateTime(_year, 12, 25);
		if (dateTime.DayOfWeek != DayOfWeek.Sunday)
		{
			num--;
			num2 = (int)dateTime.DayOfWeek;
		}
		return dateTime.AddDays((double)(-1 * (num * 7 + num2)));
	}

	public static DateTime ThanksgivingDate(string _origString)
	{
		DateTime dateTime = default(DateTime);
		for (int i = 22; i <= 28; i++)
		{
			dateTime = new DateTime(DateTime.Now.Year, 11, i);
			if (dateTime.DayOfWeek == DayOfWeek.Thursday)
			{
				break;
			}
		}
		return dateTime;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string XMLName = "events.xml";

	public static DateTime ForceTestDateTime = DateTime.MinValue;

	public static readonly Dictionary<string, EventsFromXml.EventDefinition> Events = new CaseInsensitiveStringDictionary<EventsFromXml.EventDefinition>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, EventsFromXml.SpecialDateDelegate> specialDateHandlers = new CaseInsensitiveStringDictionary<EventsFromXml.SpecialDateDelegate>();

	public readonly struct EventDefinition
	{
		public bool Active
		{
			get
			{
				DateTime now = EventsFromXml.Now;
				return now >= this.Start && now < this.End;
			}
		}

		public EventDefinition(string _name, DateTime _start, DateTime _end)
		{
			this.Name = _name;
			this.Start = _start;
			this.End = _end;
		}

		public readonly string Name;

		public readonly DateTime Start;

		public readonly DateTime End;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public delegate DateTime SpecialDateDelegate(string _origString);
}
