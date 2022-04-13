using Unity.Entities;

namespace FlockOfBirds
{
	public struct BoidSharedData : ISharedComponentData
	{
		public float radius;
		public float speed;
		public float maneuverSpeed;
		public float separation;
		public float alignment;
		public float cohesion;
		public float cellRadius;
	}
}