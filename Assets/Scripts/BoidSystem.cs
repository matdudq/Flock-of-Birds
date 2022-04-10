using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace FlockOfBirds
{
	[UpdateInGroup(typeof(SimulationSystemGroup))]
	[UpdateBefore(typeof(TransformSystemGroup))]
	internal partial class BoidSystem : SystemBase
	{
		private EntityQuery boidQuery;

		private readonly List<BoidSharedData> boidSharedDatas = new List<BoidSharedData>();
		
		protected override void OnCreate()
		{
			base.OnCreate();
			
			boidQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new[] { 
					ComponentType.ReadOnly<BoidSharedData>(),
					ComponentType.ReadWrite<LocalToWorld>() 
				},
			});
		}

		protected override void OnUpdate()
		{
			float deltaTime = math.min(0.05f,Time.DeltaTime);
			EntityManager.GetAllUniqueSharedComponentData(boidSharedDatas);
			
			for (int i = 0; i < boidSharedDatas.Count; i++)
			{
				BoidSharedData sharedData = boidSharedDatas[i];
				boidQuery.ResetFilter();
				boidQuery.AddSharedComponentFilter(sharedData);
				
				var boidCount = boidQuery.CalculateEntityCount();

				if (boidCount == 0)
				{
					continue;
				}
				
				NativeArray<LocalToWorld> neighboursLTW = boidQuery.ToComponentDataArrayAsync<LocalToWorld>(Allocator.TempJob, out JobHandle setQueryHandle);
				Dependency = JobHandle.CombineDependencies(Dependency, setQueryHandle);

				ComponentTypeHandle<LocalToWorld> ltwComponentHandle = GetComponentTypeHandle<LocalToWorld>();
				
				NativeArray<float3> alignmentVectors = CollectionHelper.CreateNativeArray<float3, RewindableAllocator>(boidCount, ref World.UpdateAllocator);
				
				var calculateAlignmentJobChunk = new AlignmentJobChunk()
				{
					boidSharedData = sharedData,
					localToWorldType = ltwComponentHandle,
					alignments = alignmentVectors,
					neighboursLTW = neighboursLTW
				};

				Dependency = calculateAlignmentJobChunk.ScheduleParallel(boidQuery, Dependency);
				
				NativeArray<float3> separationVectors = CollectionHelper.CreateNativeArray<float3, RewindableAllocator>(boidCount, ref World.UpdateAllocator);

				var calculateSeparationJobChunk = new SeparationJobChunk()
				{
					boidSharedData = sharedData,
					localToWorldType = ltwComponentHandle,
					separations = separationVectors,
					neighboursLTW = neighboursLTW
				};

				Dependency = calculateSeparationJobChunk.ScheduleParallel(boidQuery, Dependency);
				
				var moveBoidJobChunk = new MoveBoidJobChunk()
				{
					alignmentVectors = alignmentVectors,
					separationVectors = separationVectors,
					boidSharedData = sharedData,
					deltaTime = deltaTime,
					localToWorldType = ltwComponentHandle
				};
				
				Dependency = moveBoidJobChunk.ScheduleParallel(boidQuery, Dependency);
				
				var applyBordersJobChunk = new ApplyBordersBoidChunk()
				{
					borderRadius = BoidSystemSettings.Instance.BorderRadius,
					borderPosition = BoidSystemSettings.Instance.BorderOrigin,
					localToWorldType = ltwComponentHandle,
				};
				
				Dependency = applyBordersJobChunk.ScheduleParallel(boidQuery, Dependency);

				neighboursLTW.Dispose(Dependency);
			}
			
			boidSharedDatas.Clear();
		}
	}
}