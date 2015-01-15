using UnityEngine;
using System.Collections;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class BlockGame : BaseGame<BlockGameData> {
    private BlockSpawner blockSpawner;


    protected override void Awake() {
        base.Awake();
        blockSpawner = FindObjectOfType<BlockSpawner>();
    }

    protected override void Deserialize(BlockGameData data) {
        blockSpawner.Reset();
        foreach (BlockData blockData in data.Blocks) {
            blockSpawner.Spawn(blockData);
        }
    }

    protected override BlockGameData Serialize() {
        BlockData[] blockData = FindObjectsOfType<Block>().Select(block => block.Data).ToArray();
        Data.Blocks = blockData;
        return Data;
    }
}
