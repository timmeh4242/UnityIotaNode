using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using E7.ECS.LineRenderer;

namespace uIota
{
    public class VisualizationSystem : ComponentSystem
    {
        class Barrier : BarrierSystem { }
        [Inject] Barrier barrier;

        ComponentGroup addedTx;
        //ComponentGroup removedTx;

        //ArchetypeChunkEntityType entityChunkType;
        //ArchetypeChunkBufferType<Hash> hashChunkType;
        //ArchetypeChunkBufferType<Trunk> trunkChunkType;
        //ArchetypeChunkBufferType<Branch> branchChunkType;
        //ArchetypeChunkComponentType<TimeStamps> timeStampChunkType;

        NativeHashMap<Entity, float3> txToPositions;

        protected override void OnCreateManager()
        {
            base.OnCreateManager();

            addedTx = GetComponentGroup(typeof(TimeStamps), typeof(Hash), typeof(Trunk), typeof(Branch),typeof(HasTips), ComponentType.Subtractive(typeof(Initialized)));
            //removedTx = GetComponentGroup(typeof(TimeStamps), typeof(Initialized));

            txToPositions = new NativeHashMap<Entity, float3>(1024, Allocator.Persistent);
        }

        protected override void OnDestroyManager()
        {
            base.OnDestroyManager();

            txToPositions.Dispose();
        }

        struct Initialized : ISystemStateComponentData { }

        protected override void OnUpdate()
        {
            var chunks = addedTx.CreateArchetypeChunkArray(Allocator.TempJob);
            if(chunks.Length < 1)
            {
                chunks.Dispose();
                return;
            }

            var entityChunkType = GetArchetypeChunkEntityType();
            var hashChunkType = GetArchetypeChunkBufferType<Hash>();
            var trunkChunkType = GetArchetypeChunkComponentType<Trunk>();
            var branchChunkType = GetArchetypeChunkComponentType<Branch>();
            var timeStampChunkType = GetArchetypeChunkComponentType<TimeStamps>();

            var commandBuffer = barrier.CreateCommandBuffer();

            for (var i = 0; i < chunks.Length; i++)
            {
                var chunk = chunks[i];
                var entities = chunk.GetNativeArray(entityChunkType);
                var hashes = chunk.GetBufferAccessor(hashChunkType);
                var trunks = chunk.GetNativeArray(trunkChunkType);
                var branches = chunk.GetNativeArray(branchChunkType);
                var timeStamps = chunk.GetNativeArray(timeStampChunkType);

                for (var j = 0; j < chunk.Count; j++)
                {
                    commandBuffer.CreateEntity(AppManager.CubeArchetype);

                    var yPosition = UnityEngine.Mathf.Lerp((float)-chunk.Count / 2f, (float)chunk.Count / 2f, (float)j / (float)chunk.Count) * 2f;
                    var position = new float3(timeStamps[j].TimeStamp, yPosition, 0);
                    commandBuffer.SetComponent(new Position { Value = position });
                    commandBuffer.SetSharedComponent(AppManager.TransactionRenderer);

                    txToPositions.TryAdd(entities[j], position);

                    float3 trunkPos;
                    if(txToPositions.TryGetValue(trunks[j].Value, out trunkPos))
                    {
                        commandBuffer.CreateEntity();
                        var lineSegment = new LineSegment { from = position, to = trunkPos, lineWidth = 0.15f };
                        commandBuffer.AddComponent(lineSegment);
                        var lineStyle = new LineStyle { lineMaterial = AppManager.TransactionRenderer.material };
                        commandBuffer.AddSharedComponent(lineStyle);
                    }

                    float3 branchPos;
                    if (txToPositions.TryGetValue(branches[j].Value, out branchPos))
                    {
                        commandBuffer.CreateEntity();
                        var lineSegment = new LineSegment { from = position, to = branchPos, lineWidth = 0.15f };
                        commandBuffer.AddComponent(lineSegment);
                        var lineStyle = new LineStyle { lineMaterial = AppManager.TransactionRenderer.material };
                        commandBuffer.AddSharedComponent(lineStyle);
                    }

                    commandBuffer.AddComponent(entities[j], new Initialized());
                }
            }
            chunks.Dispose();

            //for (var i = 0; i < removedTx.GetEntityArray().Length; i++)
            //{

            //}
        }
    }
}