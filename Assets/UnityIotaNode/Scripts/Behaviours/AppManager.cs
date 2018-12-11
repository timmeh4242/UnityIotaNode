using UnityEngine;
using Unity.Entities;
using uIota;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

public class AppManager : MonoBehaviour
{
    public static World World;
    public static EntityManager EntityManager;

    //public static EntityArchetype TransactionArchetype;
    public static EntityArchetype BaseTransactionArchetype;
    public static EntityArchetype ValueTransactionArchetype;
    public static EntityArchetype ZeroValueTransactionArchetype;

    public static GameObject TransactionPrefab;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        World = World.Active;
        EntityManager = World.GetOrCreateManager<EntityManager>();

        //TransactionArchetype = World.Active.GetOrCreateManager<EntityManager>().CreateArchetype
        //(
        //    typeof(Hash),
        //    typeof(SignatureMessageFragment),
        //    typeof(Address),
        //    //typeof(TransactionValue),
        //    typeof(Bundle),
        //    typeof(Trunk),
        //    typeof(Branch),
        //    typeof(Nonce)
        //);

        BaseTransactionArchetype = World.Active.GetOrCreateManager<EntityManager>().CreateArchetype
        (
            //typeof(Transaction),
            typeof(Hash),
            typeof(SignatureMessageFragment),
            typeof(Address),
            //typeof(TransactionValue),
            typeof(Bundle),
            typeof(Trunk),
            typeof(Branch),
            typeof(Nonce)
        );

        ValueTransactionArchetype = World.Active.GetOrCreateManager<EntityManager>().CreateArchetype
        (
            //typeof(Transaction),
            typeof(Hash),
            typeof(SignatureMessageFragment),
            typeof(Address),
            typeof(IotaValue),
            typeof(Bundle),
            typeof(Trunk),
            typeof(Branch),
            typeof(Nonce)
        );

        ZeroValueTransactionArchetype = World.Active.GetOrCreateManager<EntityManager>().CreateArchetype
        (
            //typeof(Transaction),
            typeof(Hash),
            typeof(SignatureMessageFragment),
            typeof(Address),
            typeof(Bundle),
            typeof(Trunk),
            typeof(Branch),
            typeof(Nonce)
        );

        TransactionPrefab = Resources.Load<GameObject>("TransactionPrefab");
    }
}
