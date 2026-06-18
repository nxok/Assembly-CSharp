using System;
using System.Collections;
using System.Collections.Generic;
using DynamicMusic.Factories;
using MusicUtils.Enums;
using UnityEngine;

namespace DynamicMusic
{
	public static class TrackerTests
	{
		public static void Run(int num)
		{
			switch (num)
			{
			case 0:
				GameManager.Instance.StartCoroutine(TrackerTests.MusicTimeTrackerTest());
				return;
			case 1:
				GameManager.Instance.StartCoroutine(TrackerTests.DayTimeTrackerTest());
				return;
			case 2:
				GameManager.Instance.StartCoroutine(TrackerTests.PlayerTrackerTest());
				return;
			case 3:
				GameManager.Instance.StartCoroutine(TrackerTests.SelectorTest());
				return;
			case 4:
				GameManager.Instance.StartCoroutine(TrackerTests.ConductorTest());
				return;
			case 5:
				GameManager.Instance.StartCoroutine(TrackerTests.RealTimeConductorTest());
				return;
			default:
				return;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IEnumerator MusicTimeTrackerTest()
		{
			TrackerTests.isFinished = false;
			IMultiNotifiableFilter musicTimeTracker = Factory.CreateMusicTimeTracker();
			int num;
			for (int i = 0; i < 2; i = num + 1)
			{
				musicTimeTracker.Notify(MusicActionType.Play);
				yield return new WaitForSeconds(30f);
				musicTimeTracker.Notify(MusicActionType.Pause);
				yield return new WaitForSeconds(30f);
				musicTimeTracker.Notify(MusicActionType.UnPause);
				yield return new WaitForSeconds(30f);
				musicTimeTracker.Notify(MusicActionType.Stop);
				yield return new WaitForSeconds(30f);
				num = i;
			}
			musicTimeTracker.Notify();
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IEnumerator DayTimeTrackerTest()
		{
			TrackerTests.isFinished = false;
			List<SectionType> sectionTypes = new List<SectionType> { SectionType.Exploration };
			DayTimeTracker dtt = Factory.CreateDayTimeTracker();
			do
			{
				yield return new WaitUntil(() => TrackerTests.continueTest);
				TrackerTests.continueTest = false;
				dtt.Filter(sectionTypes);
			}
			while (!TrackerTests.isFinished);
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IEnumerator PlayerTrackerTest()
		{
			TrackerTests.isFinished = false;
			IFilter<SectionType> playerTracker = Factory.CreatePlayerTracker();
			while (!TrackerTests.isFinished)
			{
				List<SectionType> list = new List<SectionType>(TrackerTests.sections);
				playerTracker.Filter(list);
				yield return new WaitUntil(() => TrackerTests.continueTest);
				TrackerTests.continueTest = false;
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IEnumerator SelectorTest()
		{
			TrackerTests.isFinished = false;
			ISectionSelector sectionSelector = Factory.CreateSectionSelector();
			while (!TrackerTests.isFinished)
			{
				SectionType sectionType = sectionSelector.Select();
				Log.Out(string.Format("Selected Section: {0}", sectionType));
				yield return new WaitUntil(() => TrackerTests.continueTest);
				TrackerTests.continueTest = false;
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IEnumerator ConductorTest()
		{
			TrackerTests.isFinished = false;
			Conductor c = Factory.CreateConductor();
			while (!TrackerTests.isFinished)
			{
				yield return new WaitUntil(() => TrackerTests.continueTest);
				c.Update();
				TrackerTests.continueTest = false;
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IEnumerator RealTimeConductorTest()
		{
			TrackerTests.isFinished = false;
			Conductor c = Factory.CreateConductor();
			while (!TrackerTests.isFinished)
			{
				c.Update();
				yield return null;
			}
			yield break;
		}

		public static bool continueTest = false;

		public static bool isFinished = false;

		[PublicizedFrom(EAccessModifier.Private)]
		public static List<SectionType> sections = new List<SectionType>
		{
			SectionType.Exploration,
			SectionType.Suspense,
			SectionType.Combat
		};
	}
}
