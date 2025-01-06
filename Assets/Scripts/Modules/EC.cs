//Event Center

using System;
using System.Collections.Generic;
using UnityEngine.Events;
public delegate void ECListener(object args);

public static class EC
{


	private static Object agent = new Object();

	private static Dictionary<string, Dictionary<object, List<ECListener>>> msgMap = new Dictionary<string, Dictionary<object, List<ECListener>>>();
	private static Dictionary<string, Dictionary<object, List<Action>>> actMap = new Dictionary<string, Dictionary<object, List<Action>>>();

	public static void On(this object obj, string key, ECListener evt)
	{
		if (msgMap.ContainsKey(key))
		{
			if (!msgMap[key].ContainsKey(obj))
			{
				msgMap[key].Add(obj, new List<ECListener> { evt });
				return;
			}
			if (!msgMap[key][obj].Contains(evt))
			{
				msgMap[key][obj].Add(evt);
				return;
			}
		}
		else
		{
			msgMap.Add(key, new Dictionary<object, List<ECListener>>
			{
				{obj,new List<ECListener>{evt}}
			});
		}
	}
	public static void On(string key, Action act)
	{
		agent.On(key, act);
	}
	public static void On(string key, ECListener evt)
	{
		agent.On(key, evt);
	}
	public static void On(this object obj, string key, Action act)
	{
		if (actMap.ContainsKey(key))
		{
			if (!actMap[key].ContainsKey(obj))
			{
				actMap[key].Add(obj, new List<Action> { act });
				return;
			}
			if (!actMap[key][obj].Contains(act))
			{
				actMap[key][obj].Add(act);
				return;
			}
		}
		else
		{
			actMap.Add(key, new Dictionary<object, List<Action>>
			{
				{obj,new List<Action>{act}}
			});
		}
	}

	public static void Off(this object obj, string key, ECListener evt)
	{
		if (msgMap.ContainsKey(key) && msgMap[key].ContainsKey(obj))
		{
			msgMap[key][obj].Remove(evt);
		}
	}
	public static void Off(this object obj, string key, Action act)
	{
		if (actMap.ContainsKey(key) && actMap[key].ContainsKey(obj))
		{
			actMap[key][obj].Remove(act);
		}
	}
	public static void Off(string key, ECListener evt)
	{
		agent.Off(key, evt);
	}
	public static void Off(string key, Action act)
	{
		agent.Off(key, act);
	}

	public static void Off(this object obj, string key)
	{
		if (msgMap.ContainsKey(key))
		{
			msgMap[key].Remove(obj);
		}
		if (actMap.ContainsKey(key))
		{
			actMap[key].Remove(obj);
		}
	}

	public static void Send(string key, object args = null)
	{
		if (args == null)
		{
			if (actMap.ContainsKey(key))
			{
				foreach (List<Action> list in actMap[key].Values)
				{
					List<Action> list2 = new List<Action>(list);
					list2.ForEach(e => e.Invoke());
				}
			}
		}
		else
		{
			if (msgMap.ContainsKey(key))
			{
				foreach (List<ECListener> list in msgMap[key].Values)
				{
					List<ECListener> list2 = new List<ECListener>(list);
					list2.ForEach(e => e.Invoke(args));
				}
			}
		}
	}

	public static void Send(this object obj, string key, object args = null)
	{
		if (msgMap.ContainsKey(key) && msgMap[key].ContainsKey(obj))
		{
			List<ECListener> list = new List<ECListener>(msgMap[key][obj]);
			list.ForEach(e => e.Invoke(args));
		}
		if (actMap.ContainsKey(key) && actMap[key].ContainsKey(obj) && args == null)
		{
			List<Action> list = new List<Action>(actMap[key][obj]);
			list.ForEach(e => e.Invoke());
		}
	}
}