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
		private struct AlignmentJobChunk : IJobChunk
		{
			[ReadOnly]
			public NativeArray<LocalToWorld> neighboursLTW;
			[ReadOnly]
			public ComponentTypeHandle<LocalToWorld> localToWorldType;

			[ReadOnly]
			public NativeArray<int> hashes;
			[ReadOnly]
			public NativeMultiHashMap<int, int> parallelHashMap;
			
			[WriteOnly]
			public NativeArray<float3> alignments;
			
			public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
			{
				NativeArray<LocalToWorld> chunkLTW = chunk.GetNativeArray(localToWorldType);
				
				for (int i = 0; i < chunk.Count; i++)
				{
					int entityGlobalIndex = firstEntityIndex + i;

					float3 alignment = new float3(0,0,0);
					float neihboursCount = 0;

					var cellMembers = parallelHashMap.GetValuesForKey(hashes[entityGlobalIndex]);

					foreach (int cellMember in cellMembers)
					{
						alignment += neighboursLTW[cellMember].Forward;
						neihboursCount++;

						if (processingNeighbourCount < neihboursCount)
						{
							break;
						}
					}
					
					alignments[entityGlobalIndex] = (alignment - chunkLTW[i].Forward) / neihboursCount;
				}
			}
		}
	}
}