﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGeneration : MonoBehaviour
{

	[SerializeField]
	NoiseMapGeneration noiseMapGeneration;

	[SerializeField]
	private MeshRenderer tileRenderer;

	[SerializeField]
	private MeshFilter meshFilter;

	[SerializeField]
	private MeshCollider meshCollider;

	[SerializeField]
	private float mapScale;

	[SerializeField]
	private TerrainType[] terrainTypes;

	[SerializeField]
	private float heightMultiplier;

	[SerializeField]
	private AnimationCurve heightCurve;

	//[SerializeField]
	//private Wave[] wavestwo;

	//void Start()
	//{
	//	GenerateTile();
	//}

	Vector2[] uvs;
	public Texture lowTexture, mediumTexture, highTexture;
	Renderer renderer;

	//private Wave[] GenerateWaves()
	//{
	//	return wavestwo;
	//}

	public TileData GenerateTile(float centerVertexZ, float maxDistanceZ, Wave[] waves)
	{
		renderer = this.gameObject.GetComponent<Renderer>();

		// calculate tile depth and width based on the mesh vertices
		Vector3[] meshVertices = this.meshFilter.mesh.vertices;
		int tileDepth = (int)Mathf.Sqrt(meshVertices.Length);
		int tileWidth = tileDepth;

		// calculate the offsets based on the tile position
		float offsetX = -this.gameObject.transform.position.x;
		float offsetZ = -this.gameObject.transform.position.z;

		// calculate the offsets based on the tile position
		float[,] heightMap = this.noiseMapGeneration.GeneratePerlinNoiseMap(tileDepth, tileWidth, this.mapScale, offsetX, offsetZ, waves);

		TerrainType[,] heightTerrainTypes = new TerrainType[tileDepth, tileWidth];

		// generate a heightMap using noise
		//Texture2D tileTexture = BuildTexture(heightMap, this.terrainTypes, heightTerrainTypes);
		//this.tileRenderer.material.mainTexture = tileTexture;

		// Terrain Texture
		for (int z = 0; z < tileDepth; z++)
		{
			for (int x = 0; x < tileWidth; x++)
			{

				float height = heightMap[z, x];
				TerrainType terrainType = ChooseTerrainType(height, this.terrainTypes);
				heightTerrainTypes[z, x] = terrainType;

				switch (terrainType.name)
				{
					case "low":
						renderer.material.SetTexture("_MainTex", lowTexture);
						break;
					case "medium":
						renderer.material.SetTexture("_MainTex", mediumTexture);
						break;
					case "high":
						renderer.material.SetTexture("_MainTex", highTexture);
						break;
					default:
						break;
				}
			}
		}

		uvs = new Vector2[meshVertices.Length];
		for (int i = 0, z = 0; z < tileDepth; z++)
		{
			for (int x = 0; x < tileWidth; x++)
			{
				//uvs[i] = new Vector2((float)x / tileWidth, (float)z / tileDepth);
				uvs[i] = new Vector2((float)x / tileWidth * 2, (float)z / tileDepth * 2);
				i++;
			}
		}

		// update the tile mesh vertices according to the height map
		UpdateMeshVertices(heightMap);

		TileData tileData = new TileData(heightMap, heightTerrainTypes, this.meshFilter.mesh, this.gameObject);
		return tileData;
	}

	//private Texture2D BuildTexture(float[,] heightMap, TerrainType[] terrainTypes, TerrainType[,] chosenTerrainTypes)
	//{
	//	int tileDepth = heightMap.GetLength(0);
	//	int tileWidth = heightMap.GetLength(1);

	//	Color[] colourMap = new Color[tileDepth * tileWidth];
	//	for (int z = 0; z < tileDepth; z++)
	//	{
	//		for (int x = 0; x < tileWidth; x++)
	//		{
	//			// transform the 2D map index is an Array index
	//			int colourIndex = z * tileWidth + x;
	//			float height = heightMap[z, x];
	//			// assign as color a shade of grey proportional to the height value

	//			// choose a terrain type according to the height value
	//			TerrainType terrainType = ChooseTerrainType(height, terrainTypes);
	//			// assign the color according to the terrain type
	//			//colourMap[colourIndex] = terrainType.colour;

	//			switch(terrainType.name)
	//			{
	//				case "low":
	//					colourMap[colourIndex] = terrainType.colour;
	//					break;
	//				case "medium":
	//					colourMap[colourIndex] = terrainType.colour;
	//					break;
	//				case "high":
	//					colourMap[colourIndex] = terrainType.colour;
	//					break;
	//				default:
	//					break;
	//			}

	//			//colourMap[colourIndex] = Color.Lerp(Color.black, Color.white, height);
	//			// save the chosen terrain type
	//			chosenTerrainTypes[z, x] = terrainType;
	//		}
	//	}

	//	// create a new texture and set its pixel colors
	//	Texture2D tileTexture = new Texture2D(tileWidth, tileDepth);
	//	tileTexture.wrapMode = TextureWrapMode.Clamp;
	//	tileTexture.SetPixels(colourMap);
	//	tileTexture.Apply();

	//	return tileTexture;
	//}
	
	TerrainType ChooseTerrainType(float height, TerrainType[] terrainTypes)
	{
		// for each terrain type, check if the height is lower than the one for the terrain type
		foreach (TerrainType terrainType in terrainTypes)
		{
			// return the first terrain type whose height is higher than the generated one
			if (height < terrainType.height)
			{
				return terrainType;
			}
		}
		return terrainTypes[terrainTypes.Length - 1];
	}

	private void UpdateMeshVertices(float[,] heightMap)
	{
		int tileDepth = heightMap.GetLength(0);
		int tileWidth = heightMap.GetLength(1);

		Vector3[] meshVertices = this.meshFilter.mesh.vertices;

		// iterate through all the heightMap coordinates, updating the vertex index
		int vertexIndex = 0;
		for (int zIndex = 0; zIndex < tileDepth; zIndex++)
		{
			for (int xIndex = 0; xIndex < tileWidth; xIndex++)
			{
				float height = heightMap[zIndex, xIndex];

				Vector3 vertex = meshVertices[vertexIndex];
				// change the vertex Y coordinate, proportional to the height value. The height value is evaluated by the heightCurve function, in order to correct it.
				meshVertices[vertexIndex] = new Vector3(vertex.x, this.heightCurve.Evaluate(height) * this.heightMultiplier, vertex.z);

				vertexIndex++;
			}
		}

		// update the vertices in the mesh and update its properties
		this.meshFilter.mesh.vertices = meshVertices;
		this.meshFilter.mesh.uv = uvs;
		this.meshFilter.mesh.RecalculateBounds();
		this.meshFilter.mesh.RecalculateNormals();
		// update the mesh collider
		this.meshCollider.sharedMesh = this.meshFilter.mesh;
	}

}

[System.Serializable]
public class TerrainType
{
	public string name;
	public float height;
	public Color colour;
	// public int index;
}

// Class to store all the Data of a Tile
public class TileData
{
	public float[,] heightMap;
	public TerrainType[,] heightTerrainTypes;
	public Mesh mesh;
	public GameObject plane;

	public TileData(float[,] heightMap, TerrainType[,] heightTerrainTypes, Mesh mesh, GameObject plane)
	{
		this.heightMap = heightMap;
		this.heightTerrainTypes = heightTerrainTypes;
		this.mesh = mesh;
		this.plane = plane;
	}
}
