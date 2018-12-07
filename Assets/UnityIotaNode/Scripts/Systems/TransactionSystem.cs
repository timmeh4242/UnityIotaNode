using Unity.Entities;
using System.Security.Cryptography;
using System.Text;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

namespace uIota
{
    //[DisableAutoCreation]
    public class TransactionSystem : JobComponentSystem
    {
        private SHA256 hasher = SHA256.Create();
        private class Barrier : BarrierSystem { }
        [Inject] private Barrier barrier;

        //private ComponentGroup unprocessedTransactionsGroup;
        //private ComponentGroup processedTransactionsGroup;

        protected override void OnCreateManager()
        {
            base.OnCreateManager();

            CreateTransaction();

            //processedTransactionsGroup = GetComponentGroup(typeof(Hash), typeof(Bundle), typeof(Trunk), typeof(Branch));
            //unprocessedTransactionsGroup = GetComponentGroup(typeof(Hash), typeof(Bundle), typeof(Trunk), typeof(Branch));
        }

        struct ProcessedTransaction : ISystemStateComponentData { }

        //[BurstCompile]
        [RequireComponentTag(typeof(Hash))]
        [RequireSubtractiveComponent(typeof(Initialized))]
        struct Job : IJobProcessComponentDataWithEntity<Transaction>
        {
            [ReadOnly] public BufferFromEntity<Hash> hashEntityBuffer;
            [ReadOnly] public EntityCommandBuffer.Concurrent entityCommandBuffer;

            public void Execute(Entity entity, int index, ref Transaction data)
            {
                var hashBytes = Encoding.UTF8.GetBytes("YT9CVQDZMIFXNAYXAPHIFGEMIEBVGXIZVPXCYFI9YSOJRVWKY9SNYPWNXQVHGLVZTFWMBLSWEIPVA9999");
                var hashArray = new Hash[hashBytes.Length];
                for (var i = 0; i < hashBytes.Length; i++)
                {
                    hashArray[i].Value = hashBytes[i];
                }
                hashEntityBuffer[entity].CopyFrom(hashArray);
                entityCommandBuffer.AddComponent(index, entity, new Initialized());
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new Job()
            {
                hashEntityBuffer = GetBufferFromEntity<Hash>(false),
                entityCommandBuffer = barrier.CreateCommandBuffer().ToConcurrent()
            };
            return job.Schedule(this, inputDeps);
        }

        private void CreateTransaction()
        {
            //var entity = EntityManager.CreateEntity(AppManager.BaseTransactionArchetype);
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponent(entity, typeof(Transaction));
            EntityManager.AddBuffer<Hash>(entity);
            EntityManager.AddBuffer<Bundle>(entity);

            var hashBytes = Encoding.UTF8.GetBytes("YT9CVQDZMIFXNAYXAPHIFGEMIEBVGXIZVPXCYFI9YSOJRVWKY9SNYPWNXQVHGLVZTFWMBLSWEIPVA9999");
            var hashArray = new Hash[hashBytes.Length];
            for (var i = 0; i < hashBytes.Length; i++)
            {
                hashArray[i].Value = hashBytes[i];
            }
            var hashBuffer = EntityManager.GetBuffer<Hash>(entity);
            hashBuffer.CopyFrom(hashArray);
        }

        private byte[] GetHash(string message)
        {
            return hasher.ComputeHash(Encoding.UTF8.GetBytes(message));
        }
    }
}
