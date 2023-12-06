using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServiceLocator : MonoBehaviour
{
	public static ServiceLocator Instance;

	private Kernel _container;

	private bool _isInit;

	// Can be init either in awake or through an outside script
	public void Init()
    {
		Instance = this;
		_isInit = true;
		_container = new Kernel();

		DontDestroyOnLoad(this.gameObject);
	}

	private void Awake()
	{
		// Making sure we do not destroy the object due to an already init from ourside
        if (_isInit)
        {
			return;
        }

		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Init();
	}

	public T GetInstanceOfType<T>()
		where T : class
	{
		return _container.GetInstanceOfType<T>();
	}

	public bool TryGetInstanceOfType<T>(out T val)
		where T : class
	{
		val = default(T);
		bool hasValidInstance = _container.HasInstanceOfType<T>();
		if (hasValidInstance) { val = _container.GetInstanceOfType<T>(); }
		return hasValidInstance;
	}

	public void AddServices(Dictionary<Type, object> services, List<Type> overrides = null)
	{
		foreach (var item in services)
		{
            if (overrides != null && overrides.Contains(item.Key))
            {
				_container.AddOrUpdate(item.Key, item.Value);
			}
			else
            {
				_container.Add(item.Key, item.Value);
			}
		}
	}

	public void InitializeServices(Dictionary<Type, object> services)
	{
		var initilizables = services.Values
			.Where(x => (x as IInitializable) != null)
			.Select(x => x as IInitializable)
			.ToList();

		for (int i = 0; i < initilizables.Count; i++)
		{
			initilizables[i].Initialize(Instance);
		}
	}

	public bool ContainsService(Type t)
	{
		return _container.Contains(t);
	}

	public void RemoveServices(List<Type> services)
	{
		for (int i = 0; i < services.Count; i++)
		{
			_container.Remove(services[i]);
		}
	}

	public void Clear()
	{
		_container.Clear();
	}

	public void OnDestroy()
	{
		var enableables = _container.GetAllOfType<IIsDisposable>();

		for (int i = 0; i < enableables.Count; i++)
		{
			enableables[i].Dispose();
		}

	    Clear();
	}
}

public interface IInitializable
{
	void Initialize(ServiceLocator serviceLocator);
}

public interface IEnableable
{
	void OnEnable();
}

public interface IIsDisposable
{
	void Dispose();
}