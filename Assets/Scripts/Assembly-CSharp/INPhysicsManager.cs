using System;
using UnityEngine;

public class INPhysicsManager : MonoBehaviour
{
	private float m_timeStep;

	public float TimeStep
	{
		get
		{
			return m_timeStep;
		}
		set
		{
			m_timeStep = value;
		}
	}

	public static INPhysicsManager Instance { get; private set; }

	public event Action BeforeSimulation;

	public event Action AfterSimulation;

	private void Awake()
	{
		Instance = this;
		UnityEngine.Object.DontDestroyOnLoad(this);
		Physics.autoSimulation = false;
		m_timeStep = Time.fixedDeltaTime;
	}

	private void FixedUpdate()
	{
		BeforeSimulation?.Invoke();
		if (!Physics.autoSimulation)
		{
			Physics.Simulate(m_timeStep);
		}
		AfterSimulation?.Invoke();
	}
}
