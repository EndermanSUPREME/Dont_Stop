using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Chunk : MonoBehaviour
{
    /// <summary>
    /// List of all possible chunks that can be spawned in at a given side
    /// </summary>
    [Header("Available Chunk List")]
    [SerializeField] List<GameObject> LeftChunks, RightChunks, TopChunks, BottomChunks;

    [Header("Position References to Spawn Chunks at")]
    [SerializeField] Transform leftPoint;
    [SerializeField] Transform rightPoint;
    [SerializeField] Transform topPoint;
    [SerializeField] Transform bottomPoint;

    [Header("Toggles to specify valid spawn points")]
    [SerializeField] bool spawnLeft = false;
    [SerializeField] bool spawnRight = false;
    [SerializeField] bool spawnTop = false;
    [SerializeField] bool spawnBottom = false;

    // code that prevents backtracking in recursion chain
    int ignoreCode = -1;

    // recursively run down the neighbor chain
    Chunk leftNeighbor, rightNeighbor, topNeighbor, bottomNeighbor;

    void Start()
    {
        if (spawnLeft && leftPoint != null)
        {
            leftNeighbor = SpawnNeighborChunk(ref LeftChunks, leftPoint.position);
            leftNeighbor.SetRightNeighbor(this);
        }
        else Debug.LogWarning("Left Point is not set!");

        if (spawnRight && rightPoint != null)
        {
            rightNeighbor = SpawnNeighborChunk(ref RightChunks, rightPoint.position);
            rightNeighbor.SetLeftNeighbor(this);
        }
        else Debug.LogWarning("Right Point is not set!");

        if (spawnTop && topPoint != null)
        {
            topNeighbor = SpawnNeighborChunk(ref TopChunks, topPoint.position);
            topNeighbor.SetBottomNeighbor(this);
        }
        else Debug.LogWarning("Top Point is not set!");

        if (spawnBottom && bottomPoint != null)
        {
            bottomNeighbor = SpawnNeighborChunk(ref BottomChunks, bottomPoint.position);
            bottomNeighbor.SetTopNeighbor(this);
        }
        else Debug.LogWarning("Bottom Point is not set!");
    }

    // Randomly pick and spawn a chunk using a desired list of chunks and spawn point
    Chunk SpawnNeighborChunk(ref List<GameObject> chunks, Vector3 spawnPosition)
    {
        int size = chunks.Count;
        int rIndex = Random.Range(0,size) % size;

        float distFromPlayer = Vector3.Distance(PlayerManager.Instance.transform.position, spawnPosition);

        GameObject newChunk = Instantiate(chunks[rIndex], spawnPosition, Quaternion.identity);
        if (distFromPlayer >= ChunkManager.Instance.GetMaxRenderDistance())
            newChunk.gameObject.SetActive(false);

        return newChunk.GetComponent<Chunk>();
    }

    void EnableChunk() { gameObject.SetActive(true); }
    void DisableChunk() { gameObject.SetActive(false); }
    /// <summary>
    /// Skip Codes:
    /// -1 - No Skip | 
    /// 0 - Skip Left Neighbor | 
    /// 1 - Skip Right Neighbor | 
    /// 2 - Skip Top Neighbor | 
    /// 3 - Skip Bottom Neighbor
    /// </summary>
    public void TestChunk(int skipCode)
    {
        ignoreCode = skipCode;

        ChunkManager.Instance.SignalVisit();

        float distFromPlayer = Vector3.Distance(PlayerManager.Instance.transform.position, transform.position);
        if (distFromPlayer >= ChunkManager.Instance.GetMaxRenderDistance())
            DisableChunk(); // assume neighbors are too far away
        else
        {
            EnableChunk();

            // check if neighbors of enabled chunk also need enabled
            if (ignoreCode != 0 && leftNeighbor != null) leftNeighbor.TestChunk(1); // skip right on next
            if (ignoreCode != 1 && rightNeighbor != null) rightNeighbor.TestChunk(0); // skip left on next
            if (ignoreCode != 2 && topNeighbor != null) topNeighbor.TestChunk(3); // skip bottom on next
            if (ignoreCode != 3 && bottomNeighbor != null) bottomNeighbor.TestChunk(2); // skip top on next
        }
    }
    void OnTriggerEnter2D(Collider2D collider2d)
    {
        if (collider2d.GetComponent<PlayerManager>() != null)
        {
            ChunkManager.Instance.Signal(this);
        }
    }

    public void SetLeftNeighbor(Chunk chunk) { if (leftNeighbor == null) leftNeighbor = chunk; }
    public void SetRightNeighbor(Chunk chunk) { if (rightNeighbor == null) rightNeighbor = chunk; }
    public void SetTopNeighbor(Chunk chunk) { if (topNeighbor == null) topNeighbor = chunk; }
    public void SetBottomNeighbor(Chunk chunk) { if (bottomNeighbor == null) bottomNeighbor = chunk; }

    public Chunk GetLeftNeighbor() => leftNeighbor;
    public Chunk GetRightNeighbor() => rightNeighbor;
    public Chunk GetTopNeighbor() => topNeighbor;
    public Chunk GetBottomNeighbor() => bottomNeighbor;
}//EndScript