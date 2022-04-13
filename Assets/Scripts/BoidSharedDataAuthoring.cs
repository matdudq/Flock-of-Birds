using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace FlockOfBirds
{ 
	#if UNITY_EDITOR
	public class BoidSharedDataAuthoring : MonoBehaviour, IConvertGameObjectToEntity
	{
		[SerializeField]
		private float boidRadius = 0;
		
		[SerializeField]
		private float boidSpeed = 0;

		[SerializeField]
		private float boidManeuverSpeed = 0;
		
		[SerializeField, Range(0f,5f)]
		private float separation = 0.5f;
		
		[SerializeField, Range(0f,5f)]
		private float alignment = 0.5f;
		
		[SerializeField, Range(0f,5f)]
		private float cohesion = 0.5f;
		
		[SerializeField]
		private float cellRadius = 1.0f;
		
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			dstManager.AddSharedComponentData(entity, new BoidSharedData()
			{
				radius = boidRadius,
				speed = boidSpeed,
				maneuverSpeed = boidManeuverSpeed,
				separation = separation,
				alignment = alignment,
				cohesion = cohesion,
				cellRadius = cellRadius
			});
			
			dstManager.RemoveComponent<Translation>(entity);
			dstManager.RemoveComponent<Rotation>(entity);
		}
	}
	#endif
}