using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DeepDreamGames
{
	public class EditorCoroutines
	{
		private class Coroutine
		{
			public IEnumerator enumerator;
			public Stack<IEnumerator> stack;
		}

		static private bool initialized;
		static private readonly List<Coroutine> coroutines = new List<Coroutine>();

		#region Private Methods
		// 
		static private void Initialize()
		{
			if (initialized) { return; }

			initialized = true;
			EditorApplication.update += OnUpdate;
		}

		// 
		static private void Deinitialize()
		{
			if (!initialized) { return; }

			initialized = false;
			EditorApplication.update -= OnUpdate;
		}
		#endregion

		#region Private Methods
		// 
		static private void OnUpdate()
		{
			for (int i = 0; i < coroutines.Count; i++)
			{
				Coroutine coroutine = coroutines[i];
				bool done = false;
				try
				{
					done = !coroutine.enumerator.MoveNext();
				}
				catch (Exception ex)
				{
					// An exception has occured - abort entire stack for this instance
					coroutines.RemoveAt(i);
					i--;
					Debug.LogException(ex);
					continue;
				}

				// Current IEnumerator finished
				if (done)
				{
					// End
					if (coroutine.stack == null || coroutine.stack.Count == 0)
					{
						coroutines.RemoveAt(i);
						i--;
					}
					// Pop
					else
					{
						coroutine.enumerator = coroutine.stack.Pop();
					}
				}
				// Push
				else if (coroutine.enumerator.Current is IEnumerator)
				{
					if (coroutine.stack == null) { coroutine.stack = new Stack<IEnumerator>(); }
					coroutine.stack.Push(coroutine.enumerator);
					coroutine.enumerator = (IEnumerator)coroutine.enumerator.Current;
				}
			}

			if (coroutines.Count == 0)
			{
				Deinitialize();
			}
		}
		#endregion

		#region Public Methods
		// 
		static public void StartCoroutine(IEnumerator enumerator)
		{
			if (enumerator == null) { return; }

			Initialize();
			Coroutine coroutine = new Coroutine()
			{
				enumerator = enumerator,
			};
			coroutines.Add(coroutine);
		}

		// 
		static public void StopCoroutine(IEnumerator enumerator)
		{
			if (enumerator == null) { return; }

			for (int i = 0; i < coroutines.Count; i++)
			{
				if (enumerator == coroutines[i].enumerator)
				{
					coroutines.RemoveAt(i);
					return;
				}
			}

			if (coroutines.Count == 0)
			{
				Deinitialize();
			}
		}

		// 
		static public void StopAllCoroutines()
		{
			coroutines.Clear();
			Deinitialize();
		}
		#endregion
	}
}