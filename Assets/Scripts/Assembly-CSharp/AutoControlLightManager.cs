using System.Collections.Generic;
using UnityEngine;

public class AutoControlLightManager : PartManager
{
	private List<AutoControlLight> m_lights;

	private HashSet<Collider> m_activeColliders;

	private Dictionary<Renderer, float> m_activeRenderers;

	// 复用缓冲区，避免 FixedUpdate 每帧分配
	private HashSet<Collider> m_newColliders = new HashSet<Collider>();
	private HashSet<Renderer> m_newRenderers = new HashSet<Renderer>();
	private Dictionary<Renderer, float> m_newRendererAlphas = new Dictionary<Renderer, float>();

	public override StatusCode Status => StatusCode.Running;

	protected override void Initialize()
	{
		base.Initialize();
		m_lights = new List<AutoControlLight>();
		m_activeColliders = new HashSet<Collider>();
		m_activeRenderers = new Dictionary<Renderer, float>();
	}

	public override void FixedUpdate()
	{
		// 快速收集所有 AutoControlLight 部件
		m_lights.Clear();
		foreach (BasePart part in Contraption.Instance.Parts)
		{
			if (part.IsAutoControlLight())
			{
				m_lights.Add((AutoControlLight)part);
			}
		}

		// 无 AutoControlLight 时直接跳过
		if (m_lights.Count == 0)
		{
			return;
		}

		// 检查是否有启用的 AutoControlLight
		bool anyEnabled = false;
		for (int i = 0; i < m_lights.Count; i++)
		{
			if (m_lights[i].IsEnabled() && m_lights[i].enclosedInto != null)
			{
				anyEnabled = true;
				break;
			}
		}

		if (!anyEnabled)
		{
			return;
		}

		Rigidbody[] components = INContraption.Instance.GetComponents<Rigidbody>();

		// 复用缓冲区
		m_newColliders.Clear();
		m_newRenderers.Clear();
		m_newRendererAlphas.Clear();

		foreach (AutoControlLight light in m_lights)
		{
			if (!light.IsEnabled() || light.enclosedInto == null)
			{
				light.SetDetected(detected: false, force: true);
				continue;
			}

			bool detected = light.CurrentType == 1;
			bool collided = false;
			Vector3 position = light.transform.position;
			Vector3 direction = light.transform.TransformDirection(Vector3.down);

			foreach (Rigidbody rb in components)
			{
				Vector3 rbPos = rb.position;
				float along = direction.x * (rbPos.x - position.x) + direction.y * (rbPos.y - position.y);
				float across = direction.y * (rbPos.x - position.x) - direction.x * (rbPos.y - position.y);
				bool inDetect = light.IsInDetectArea(along, across);
				bool inCollide = light.IsInCollideArea(along, across);

				if (inDetect || inCollide)
				{
					BasePart part = rb.GetComponent<BasePart>();
					int lightCC = light.ConnectedComponent;
					int partCC = (part != null) ? part.ConnectedComponent : -1;

					if (inDetect && (part == null || partCC != lightCC))
						detected |= true;
					if (inCollide && part != null && partCC != lightCC)
						collided |= true;

					if (detected && collided)
						break;
				}
			}

			light.SetDetected(detected, force: false);
			light.SetCollided(collided);
		}

		// 处理 Frame 部件的碰撞/渲染
		foreach (BasePart part in Contraption.Instance.Parts)
		{
			if (part.m_partType != BasePart.PartType.WoodenFrame && part.m_partType != BasePart.PartType.MetalFrame)
				continue;

			bool inLight = false;
			Vector3 partPos = part.rigidbody.position;

			foreach (AutoControlLight light in m_lights)
			{
				if (!light.IsEnabled() || !light.IsDetected())
					continue;

				Vector3 lightPos = light.transform.position;
				Vector3 dir = light.transform.TransformDirection(Vector3.down);
				float along = dir.x * (partPos.x - lightPos.x) + dir.y * (partPos.y - lightPos.y);
				float across = dir.y * (partPos.x - lightPos.x) - dir.x * (partPos.y - lightPos.y);

				if (light.IsInCollideArea(along, across))
				{
					inLight = part != null && part.ConnectedComponent == light.ConnectedComponent;
					if (inLight) break;
				}
			}

			if (inLight)
			{
				if (part.collider != null) m_newColliders.Add(part.collider);
				if (part.renderer != null) m_newRenderers.Add(part.renderer);
			}
		}

		// 恢复上一帧修改的 Collider 和 Renderer
		foreach (Collider col in m_activeColliders)
		{
			if (col != null) col.enabled = true;
		}
		foreach (KeyValuePair<Renderer, float> kv in m_activeRenderers)
		{
			if (kv.Key != null)
			{
				Color c = kv.Key.material.color;
				c.a = kv.Value;
				kv.Key.material.color = c;
			}
		}

		// 应用新修改
		foreach (Collider col in m_newColliders)
		{
			col.enabled = false;
		}
		foreach (Renderer r in m_newRenderers)
		{
			Color c = r.material.color;
			m_newRendererAlphas[r] = c.a;
			c.a = 0.25f;
			r.material.color = c;
		}

		// 交换缓冲区（复用）
		HashSet<Collider> tempCol = m_activeColliders;
		m_activeColliders = m_newColliders;
		m_newColliders = tempCol;

		Dictionary<Renderer, float> tempRen = m_activeRenderers;
		m_activeRenderers = m_newRendererAlphas;
		m_newRendererAlphas = tempRen;
	}
}
