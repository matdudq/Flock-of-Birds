using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace FlockOfBirds
{
	internal partial class BoidSystem 
	{
		[BurstCompile]
		private struct SeparationJobChunk : IJobChunk
		{
			[ReadOnly]
			public BoidSharedData boidSharedData;
			[ReadOnly]
			public NativeArray<LocalToWorld> neighboursLTW;
			[ReadOnly]
			public ComponentTypeHandle<LocalToWorld> localToWorldType;
			
			public NativeArray<float3> separations;

			public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
			{
				NativeArray<LocalToWorld> chunkLTW = chunk.GetNativeArray(localToWorldType);
				float desiredSeparation = math.pow(boidSharedData.radius, 2);
				for (int i = 0; i < chunk.Count; i++)
				{
					int entityGlobalIndex = firstEntityIndex + i;
					float3 position = chunkLTW[i].Position;

					float3 separation = new float3(0,0,0);
					float neihboursCount = 0;
					
					for (int j = 0; j < neighboursLTW.Length; j++)
					{
						if (entityGlobalIndex == j)
						{
							continue;
						}
					
						float3 neighbourPosition = neighboursLTW[j].Position;
						float distanceToNeighbour = math.length(position - neighbourPosition);
						if (distanceToNeighbour <= desiredSeparation)
						{
							float3 neighbourPosToPos = math.normalize(position - neighbourPosition);
							neighbourPosToPos /= distanceToNeighbour;
							separation += neighbourPosToPos;
							neihboursCount++;
						}
					}

					separations[entityGlobalIndex] = neihboursCount > 0 ? math.normalize(separation) : separation;
				}
			}
		}
	}
}