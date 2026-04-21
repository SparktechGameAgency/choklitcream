// Scripts/Systems/InfiniteBackground.cs
using UnityEngine;

public class InfiniteBackground : MonoBehaviour
{
    private Transform player;
    private Vector2 tileSize;
    private GameObject[,] tiles = new GameObject[3, 3];

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        tileSize = GetComponent<SpriteRenderer>().bounds.size;

        // Grab the original sprite + material to clone
        SpriteRenderer original = GetComponent<SpriteRenderer>();

        // Build a 3x3 grid of tiles centered on the player
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                // Reuse the original GameObject for the center tile [1,1]
                if (x == 1 && y == 1)
                {
                    tiles[x, y] = gameObject;
                    continue;
                }

                // Clone for the other 8 surrounding tiles
                GameObject tile = new GameObject("BG_" + x + "_" + y);
                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = original.sprite;
                sr.drawMode = original.drawMode;
                sr.size = original.size;
                sr.sortingOrder = original.sortingOrder;
                tile.transform.localScale = transform.localScale;

                tiles[x, y] = tile;
            }
        }

        SnapAllTiles();
    }

    void LateUpdate()
    {
        // Check if player has moved far enough to need a grid shift
        Vector2 centerPos = tiles[1, 1].transform.position;
        float offsetX = player.position.x - centerPos.x;
        float offsetY = player.position.y - centerPos.y;

        if (Mathf.Abs(offsetX) >= tileSize.x || Mathf.Abs(offsetY) >= tileSize.y)
            SnapAllTiles();
    }

    void SnapAllTiles()
    {
        // Round player position to nearest tile so grid snaps cleanly
        Vector2 origin = new Vector2(
            Mathf.Round(player.position.x / tileSize.x) * tileSize.x,
            Mathf.Round(player.position.y / tileSize.y) * tileSize.y
        );

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                tiles[x, y].transform.position = new Vector3(
                    origin.x + (x - 1) * tileSize.x,
                    origin.y + (y - 1) * tileSize.y,
                    transform.position.z
                );
            }
        }
    }
}