using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChunkManager : Singleton<ChunkManager>
{
    [SerializeField] int MaxRenderDistance = 35, recursiveMax = 100;
    Chunk chunkWithPlayer = null;
    int chunksVisited = 0;

    void FixedUpdate()
    {
        chunksVisited = 0;
        if (chunkWithPlayer != null)
        {
            chunkWithPlayer.TestChunk(-1);
        }
    }

    public int GetMaxRenderDistance() => MaxRenderDistance;

    // Update Manager with Chunk that contains the Player
    public void Signal(Chunk chunk)
    {
        chunkWithPlayer = chunk;
    }
    public void SignalVisit()
    {
        ++chunksVisited;
        if (chunksVisited > recursiveMax)
        {
            // Exit Play Mode in Editor
            #if UNITY_EDITOR
                Debug.LogError("Potential Stack Overflow from Recursion Occured!");
                EditorApplication.isPlaying = false;
            #endif
        }
    }

    void OnDrawGizmos()
    {
        if (chunkWithPlayer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(chunkWithPlayer.transform.position, 5f);
        }
    }
}//EndScript