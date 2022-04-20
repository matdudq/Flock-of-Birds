using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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
			
			[ReadOnly]
			public NativeArray<int> hashes;
			[ReadOnly]
			public NativeMultiHashMap<int, int> parallelHashMap;
			
			[WriteOnly]
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
					
					var cellMembers = parallelHashMap.GetValuesForKey(hashes[entityGlobalIndex]);
					float neihboursCount = 0;

					foreach (int cellMember in cellMembers)
					{
						float3 neighbourPosition = neighboursLTW[cellMember].Position;
						float distanceToNeighbour = math.length(position - neighbourPosition);
						if (distanceToNeighbour <= desiredSeparation)
						{
							float3 neighbourPosToPos = math.normalizesafe(position - neighbourPosition);
							separation += neighbourPosToPos;
							neihboursCount++;
						}
						
						if (processingNeighbourCount < neihboursCount)
						{
							break;
						}
					}
					
					separations[entityGlobalIndex] = math.normalizesafe(separation) ;
				}
			}
		}
	}
}