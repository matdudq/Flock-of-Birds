using Unity.Entities;

namespace FlockOfBirds
{
	public struct BoidGroup : IComponentData
	{
		public Entity boidPrefab;
		public float spawnRadius;
		public int count;
	}
}