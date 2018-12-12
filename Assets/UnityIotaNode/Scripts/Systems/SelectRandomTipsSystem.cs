using Unity.Entities;
using System.Security.Cryptography;
using System.Text;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace uIota
{
    [DisableAutoCreation]
    public class SelectRandomTipsSystem : JobComponentSystem
    {
        class Barrier : BarrierSystem { }
        [Inject] Barrier barrier;

        ComponentGroup unprocessedTx;
        ComponentGroup processedTx;

        protected override void OnCreateManager()
        {
            base.OnCreateManager();

            unprocessedTx = GetComponentGroup(typeof(Trunk), typeof(Branch), ComponentType.Subtractive(typeof(HasTips)));
            processedTx = GetComponentGroup(typeof(Trunk), typeof(Branch), typeof(Hash), typeof(HasTips));
        }

        [BurstCompile]
        struct GetRandomTipsJob : IJobParallelFor
        {
            [ReadOnly][DeallocateOnJobCompletion]
            public NativeArray<ArchetypeChunk> chunks;
            public ArchetypeChunkBufferType<Trunk> trunkType;
            public ArchetypeChunkBufferType<Branch> branchType;

            [ReadOnly] public BufferArray<Hash> processedHashes;

            public void Execute(int index)
            {
                var chunk = chunks[index];
                var trunksAccessor = chunk.GetBufferAccessor(trunkType);
                var branchesAccessor = chunk.GetBufferAccessor(branchType);

                //var rnd = new Unity.Mathematics.Random((uint)(index + 1));
                var rnd = new Unity.Mathematics.Random(0x6E624EB7u);

                for (var i = 0; i < chunk.Count; i++)
                {
                    var trunkBuffer = trunksAccessor[i];
                    var branchBuffer = branchesAccessor[i];

                    var nextIndex = rnd.NextInt(0, processedHashes.Length);
                    //UnityEngine.Debug.Log(nextIndex);
                    var hash = processedHashes[nextIndex];
                    trunkBuffer.Clear();
                    for (var j = 0; j < hash.Length; j++)
                    {
                        var trunk = new Trunk() { Value = hash[j].Value };
                        trunkBuffer.Add(trunk);
                    }

                    nextIndex = rnd.NextInt(0, processedHashes.Length);
                    //UnityEngine.Debug.Log(nextIndex);
                    hash = processedHashes[nextIndex];
                    branchBuffer.Clear();
                    for (var j = 0; j < hash.Length; j++)
                    {
                        var branch = new Branch() { Value = hash[j].Value };
                        branchBuffer.Add(branch);
                    }
                }
            }
        }
        
        //[BurstCompile]
        struct InitializeJob : IJob
        {
            [ReadOnly] public EntityArray entities;
            [ReadOnly] public EntityCommandBuffer.Concurrent entityCommandBuffer;

            public void Execute()
            {
                for(var i = 0; i < entities.Length; i++)
                {
                    entityCommandBuffer.AddComponent(i, entities[i], new HasTips());
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if(unprocessedTx.GetEntityArray().Length < 1) { return inputDeps; }

            var chunks = unprocessedTx.CreateArchetypeChunkArray(Allocator.TempJob);
            var processedHashes = processedTx.GetBufferArray<Hash>();
            var job = new GetRandomTipsJob()
            {
                chunks = chunks,
                trunkType = GetArchetypeChunkBufferType<Trunk>(),
                branchType = GetArchetypeChunkBufferType<Branch>(),
                processedHashes = processedHashes
            };
            var getTipsHandle = job.Schedule(chunks.Length, 32);

            var initializeTxHandle = new InitializeJob()
            {
                entities = unprocessedTx.GetEntityArray(),
                entityCommandBuffer = barrier.CreateCommandBuffer().ToConcurrent()
            };
            var initializeHandle = initializeTxHandle.Schedule(getTipsHandle);

            return initializeHandle;
        }
    }
}
