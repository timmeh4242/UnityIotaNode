using Unity.Entities;

namespace uIota
{
    public class TransactionCreationSystem : ComponentSystem
    {
        ComponentGroup transactions;

        protected override void OnCreateManager()
        {
            base.OnCreateManager();

            var genesis = EntityManager.CreateEntity();

            EntityManager.AddBuffer<Hash>(genesis);
            var hashArray = new Hash[9];
            for (var i = 0; i < 9; i++)
            { hashArray[i].Value = 9; }
            var hashBuffer = EntityManager.GetBuffer<Hash>(genesis);
            hashBuffer.CopyFrom(hashArray);

            var timeStamps = new TimeStamps();
            timeStamps.TimeStamp = (long)-1;
            EntityManager.AddComponent(genesis, typeof(TimeStamps));
            EntityManager.SetComponentData(genesis, timeStamps);

            EntityManager.AddBuffer<Trunk>(genesis);
            EntityManager.AddBuffer<Branch>(genesis);
            EntityManager.AddComponent(genesis, typeof(HasTips));

            transactions = GetComponentGroup(typeof(Hash));
        }

        protected override void OnUpdate()
        {
        }
    }
}
