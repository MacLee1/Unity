using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExcelDB<T> : ScriptableObject
	where T : class
{
	static T _instance;

	public static T Instance
	{
		get
		{
			if(_instance == null)
				_instance = Resources.Load("SheetData/" + typeof(T), typeof(T)) as T;
			return _instance;
		}
	}
}
