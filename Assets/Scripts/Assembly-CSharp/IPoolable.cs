/// <summary>
/// 实现此接口的对象在被对象池 Rent/Return 时会自动收到回调，
/// 用于重置组件状态（如粒子系统、Rigidbody 等）。
/// </summary>
public interface IPoolable
{
	/// <summary>从池中取出时调用（SetActive(true) 之后）</summary>
	void OnRent();

	/// <summary>归还到池中时调用（SetActive(false) 之前）</summary>
	void OnReturn();
}
