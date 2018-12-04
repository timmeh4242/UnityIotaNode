using UnityEngine;
using Unity.Entities;
using uIota;
using System.Collections;

public class AppManager : MonoBehaviour
{
    private World world;
    private EntityManager entityManager;
    //private IotaSystem iotaSystem;

    public static EntityArchetype TransactionArchetype;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        TransactionArchetype = World.Active.GetOrCreateManager<EntityManager>().CreateArchetype( typeof(Transaction));
    }

    private void Awake()
    {
        world = World.Active;
        entityManager = world.GetOrCreateManager<EntityManager>();
        //iotaSystem = world.GetExistingManager<IotaSystem>();
    }

    private void OnEnable()
    {
        StartCoroutine(CreateTransactions());
    }

    private void OnDisable()
    {
        StopCoroutine(CreateTransactions());
    }

    private IEnumerator CreateTransactions()
    {
        while(true)
        {
            var entity = entityManager.CreateEntity(TransactionArchetype);
            var transaction = new Transaction();

            //var hash = new Hash();
            //hash.Value = System.Guid.NewGuid().ToString();
            //hash.Value = Random.Range(0, 1000000);

            //transaction.Hash = hash;
            //transaction.Hash = System.Guid.NewGuid().ToString();
            transaction.Hash = Random.Range(0, 1000000);
            entityManager.SetComponentData(entity, transaction);
            yield return new WaitForSeconds(3f);
        }
    }
}
