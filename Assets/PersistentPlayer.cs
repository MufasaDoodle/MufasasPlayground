using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PersistentPlayer : NetworkBehaviour
{
    [SerializeField]
    PersistentPlayerRuntimeCollection m_PersistentPlayerRuntimeCollection;

    [SerializeField]
    NetworkNameState m_NetworkNameState;

    public NetworkNameState NetworkNameState => m_NetworkNameState;


    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public override void OnNetworkSpawn()
    {
        gameObject.name = "PersistentPlayer" + OwnerClientId;

        // Note that this is done here on OnNetworkSpawn in case this NetworkBehaviour's properties are accessed
        // when this element is added to the runtime collection. If this was done in OnEnable() there is a chance
        // that OwnerClientID could be its default value (0).
        m_PersistentPlayerRuntimeCollection.Add(this);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        RemovePersistentPlayer();
    }

    public override void OnNetworkDespawn()
    {
        RemovePersistentPlayer();
    }

    void RemovePersistentPlayer()
    {
        m_PersistentPlayerRuntimeCollection.Remove(this);
    }
}
