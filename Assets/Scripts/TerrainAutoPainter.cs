using UnityEngine;
using UnityEditor;

public class TerrainAutoPainter : EditorWindow
{
    float rockHeight = 50f;     // Height where rock starts to appear
    float snowHeight = 150f;    // Height where snow starts to appear
    float blendRange = 20f;     // How smooth the transitions are

    [MenuItem("Tools/Terrain Auto-Painter")]
    public static void ShowWindow() => GetWindow<TerrainAutoPainter>("Auto-Painter");

    void OnGUI()
    {
        rockHeight = EditorGUILayout.FloatField("Rock Height Start", rockHeight);
        snowHeight = EditorGUILayout.FloatField("Snow Height Start", snowHeight);
        blendRange = EditorGUILayout.FloatField("Blend Smoothness", blendRange);

        if (GUILayout.Button("Paint All Terrains"))
        {
            PaintTerrains();
        }
    }

    void PaintTerrains()
    {
        Terrain[] terrains = Terrain.activeTerrains;

        foreach (Terrain t in terrains)
        {
            TerrainData data = t.terrainData;
            
            int mapWidth = data.alphamapWidth;
            int mapHeight = data.alphamapHeight;
            float[,,] alphamaps = data.GetAlphamaps(0, 0, mapWidth, mapHeight);

            // Check if we actually have 3 layers assigned to the terrain
            if (data.terrainLayers.Length < 3)
            {
                Debug.LogError($"{t.name} is missing a 3rd layer (Snow). Please add a White layer!");
                continue;
            }

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float normX = (float)x / (mapWidth - 1);
                    float normY = (float)y / (mapHeight - 1);

                    float height = data.GetInterpolatedHeight(normX, normY);

                    // Calculate Weights
                    // We use Clamp01 to ensure the values stay between 0 and 1
                    float rockWeight = Mathf.Clamp01((height - rockHeight) / blendRange);
                    float snowWeight = Mathf.Clamp01((height - snowHeight) / blendRange);

                    // Logic: 
                    // 1. Snow takes priority over Rock.
                    // 2. Rock takes priority over Grass.
                    
                    float finalSnow = snowWeight;
                    float finalRock = rockWeight * (1 - snowWeight); // Rock only shows where there is NO snow
                    float finalGrass = 1 - (finalRock + finalSnow);  // Grass fills whatever is left

                    alphamaps[y, x, 0] = finalGrass; 
                    alphamaps[y, x, 1] = finalRock;
                    alphamaps[y, x, 2] = finalSnow;
                }
            }
            data.SetAlphamaps(0, 0, alphamaps);
        }
    }
}