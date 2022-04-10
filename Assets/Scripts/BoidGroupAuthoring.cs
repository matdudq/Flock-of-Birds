using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace FlockOfBirds
{
	public class BoidGroupAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
	{
		public GameObject boidPrefab;
		public float spawnRadius;
		public int count;

		public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
		{
			referencedPrefabs.Add(boidPrefab);
		}

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			dstManager.AddComponentData(entity, new BoidGroup()
			{
				boidPrefab = conversionSystem.GetPrimaryEntity(boidPrefab),
				count = count,
				spawnRadius = spawnRadius
			});
		}
	}
}