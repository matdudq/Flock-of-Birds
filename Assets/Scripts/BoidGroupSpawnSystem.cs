using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace FlockOfBirds
{
	public partial class BoidGroupSpawnSystem: SystemBase
	{
		private World DefaultWorld
		{
			get
			{
				return World.DefaultGameObjectInjectionWorld;
			}
		}
		
		protected override void OnUpdate()
		{
			var beginInitializationECB = DefaultWorld.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>().CreateCommandBuffer();

			Entities.ForEach((Entity entity, in BoidGroup boidGroup, in Translation boidSchoolPosition) => 
			{ 
				for (int i = 0; i < boidGroup.count; i++)
				{
					Entity spawnedEntity = beginInitializationECB.Instantiate(boidGroup.boidPrefab);

					float3 randomInsideUnitSphere = GetRandomInsideUnitSphere(entity.Index + i);
					float3 position = boidSchoolPosition.Value + randomInsideUnitSphere * boidGroup.spawnRadius;
					quaternion rotation = quaternion.LookRotationSafe(GetRandomInsideUnitSphere((entity.Index + i) * 4034), math.up());
					float3 scale = new float3(1.0f, 1.0f, 1.0f);
					
					LocalToWorld localToWorld = new LocalToWorld
					{
						Value = float4x4.TRS(position, rotation, scale)
					};
					
					beginInitializationECB.RemoveComponent<Translation>(spawnedEntity);
					beginInitializationECB.RemoveComponent<Rotation>(spawnedEntity);
					beginInitializationECB.SetComponent(spawnedEntity, localToWorld);
				}
				
				beginInitializationECB.DestroyEntity(entity);
				
			}).Run();
			
			float3 GetRandomInsideUnitSphere(float seed)
			{
				var random = new Random((uint)seed * 0x9F6ABC1);
				var dir = math.normalizesafe(random.NextFloat3() - new float3(0.5f, 0.5f, 0.5f));
				return dir * random.NextFloat();
			}
		}
	}
}