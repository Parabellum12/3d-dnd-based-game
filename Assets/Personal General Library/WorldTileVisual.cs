

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WorldTileVisual : MonoBehaviour {

    private GridClass<World_Handler_Script.WorldBuildTile> objectGrid;
    private Mesh mesh;
    private bool updateMesh;
    [SerializeField] Texture texture;

    private void Awake() {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void SetGrid(GridClass<World_Handler_Script.WorldBuildTile> objectGrid) {
        this.objectGrid = objectGrid;
        UpdateHeatMapVisual();
        objectGrid.onGridValueChanged += Grid_OnGridValueChanged;
    }

    private void Grid_OnGridValueChanged(object sender, GridClass<World_Handler_Script.WorldBuildTile>.OnGridValueChangedEventArgs e) {
        updateMesh = true;
    }

    private void LateUpdate() {
        if (updateMesh) {
            updateMesh = false;
            UpdateHeatMapVisual();
        }
    }

    private void UpdateHeatMapVisual() {
        Debug.Log("Updated Mesh");
        MeshUtils.CreateEmptyMeshArrays(objectGrid.getWidth() * objectGrid.getWidth(), out Vector3[] vertices, out Vector2[] uv, out int[] triangles);
        for (int x = 0; x < objectGrid.getWidth(); x++) {
            for (int y = 0; y < objectGrid.getHeight(); y++) {
                int index = x * objectGrid.getHeight() + y;
                Vector3 quadSize = new Vector3(1, 1) * objectGrid.getCellSize();

                World_Handler_Script.WorldBuildTile gridObject = objectGrid.getGridObject(x, y);
                Vector2 pos = gridObject.getMainBuildData().altasPos;
                int tilePixelSize = 128;
                Vector2 uv00 = new Vector2(((tilePixelSize + 1) * pos.x) / texture.width, ((tilePixelSize+1) * (pos.y)) / texture.height);
                Vector2 uv11 = new Vector2(((tilePixelSize * pos.x) + tilePixelSize-1) / texture.width, ((tilePixelSize * pos.y) + tilePixelSize-1) / texture.height);

                //Debug.Log(gridObject.getMainBuildData().BuildingName + ":" + pos.ToString() + ":" + uv00.ToString() + ":" + uv11.ToString());
                MeshUtils.AddToMeshArrays(vertices, uv, triangles, index, objectGrid.getWorldPosition(x, y) + quadSize * .5f, 0f, quadSize, uv00, uv11);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }
}
*/


