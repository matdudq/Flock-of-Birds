using Unity.Entities;

namespace FlockOfBirds
{
	public struct BoidSharedData : ISharedComponentData
	{
		public float radius;
		public float speed;
		public float separation;
		public float alignment;
		public float alignmentRadius;
		public float cohesion;
	}
}