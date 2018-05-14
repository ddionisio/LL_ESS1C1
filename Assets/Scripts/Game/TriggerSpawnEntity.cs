using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSpawnEntity : MonoBehaviour {
    public bool isOnce = true;
    public float delay;

    [Header("Signals")]
    public M8.Signal signalReset;

    [Header("Spawn")]
    public string poolGroup;
    public string poolEntityType;

    public Transform spawnAt;

    private Collider2D mColl;

    public void ResetState() {
        mColl.enabled = true;
    }

    void OnDestroy() {
        if(signalReset) signalReset.callback -= OnSignalReset;
    }

    void Awake() {
        mColl = GetComponent<Collider2D>();

        if(signalReset) signalReset.callback += OnSignalReset;
    }

    void OnTriggerEnter2D(Collider2D collision) {
        Vector2 toPos = spawnAt ? spawnAt.position : transform.position;

        if(delay > 0f)
            StartCoroutine(DoSpawnDelay(toPos));
        else
            M8.PoolController.SpawnFromGroup(poolGroup, poolEntityType, poolEntityType, null, toPos, null);

        if(isOnce)
            mColl.enabled = false;
    }

    void OnSignalReset() {
        StopAllCoroutines();

        ResetState();
    }

    IEnumerator DoSpawnDelay(Vector2 toPos) {
        yield return new WaitForSeconds(delay);

        M8.PoolController.SpawnFromGroup(poolGroup, poolEntityType, poolEntityType, null, toPos, null);
    }
}
