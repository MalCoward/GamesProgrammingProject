﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoliageGeneration : MonoBehaviour
{
	[SerializeField]
	private NoiseMapGeneration noiseMapGeneration;

	[SerializeField]
	private float levelScale;
	[SerializeField]
	private float neighborRadius;

	[SerializeField]
	private GameObject foliagePrefab1;
	[SerializeField]
	private GameObject foliagePrefab2;

	private GameObject foliagePrefab;

	RaycastHit hit;
	float maxHeight = 20f;
	Ray ray;

	public void GenerateFoliage(int levelDepth, int levelWidth, float distanceBetweenVertices, LevelData levelData, Wave[] waves)
	{
		// Generate a foliage noise map using Perlin Noise
		float[,] foliageMap = this.noiseMapGeneration.GeneratePerlinNoiseMap(levelDepth, levelWidth, levelScale, 0, 0, waves);

		float levelSizeX = levelWidth * distanceBetweenVertices;
		float levelSizeZ = levelDepth * distanceBetweenVertices;

		for (int z = 0; z < levelDepth; z++)
		{
			for (int x = 0; x < levelWidth; x++)
			{
				// Convert from Level Coordinate System to Tile Coordinate System and retrieve the corresponding TileData
				TileCoordinate tileCoordinate = levelData.ConvertToTileCoordinate(z, x);
				TileData tileData = levelData.tilesData[tileCoordinate.tileZ, tileCoordinate.tileX];
				int tileWidth = tileData.heightMap.GetLength(1);

				// Get the terrain type of this coordinate
				TerrainType terrainType = tileData.heightTerrainTypes[tileCoordinate.coordinateZ, tileCoordinate.coordinateX];
				float foliageValue = foliageMap[z, x];

				// Compares the current foliage noise value to the neighbor ones
				int neighborZBegin = (int)Mathf.Max(0, z - this.neighborRadius);
				int neighborZEnd = (int)Mathf.Min(levelDepth - 1, z + this.neighborRadius);
				int neighborXBegin = (int)Mathf.Max(0, x - this.neighborRadius);
				int neighborXEnd = (int)Mathf.Min(levelWidth - 1, x + this.neighborRadius);
				float maxValue = 0f;
				for (int neighborZ = neighborZBegin; neighborZ <= neighborZEnd; neighborZ++)
				{
					for (int neighborX = neighborXBegin; neighborX <= neighborXEnd; neighborX++)
					{
						float neighborValue = foliageMap[neighborZ, neighborX];
						if (neighborValue >= maxValue)
						{
							maxValue = neighborValue;
						}
					}
				}

				// If the current foliage noise value is the maximum one, place a foliage in this location
				if (foliageValue == maxValue)
				{

					float xPos = x * distanceBetweenVertices;
					float zPos = z * distanceBetweenVertices;
					float yPos = 0f;

					ray.origin = new Vector3(xPos, maxHeight, zPos);
					ray.direction = Vector3.down;
					hit = new RaycastHit();

					if (Physics.Raycast(ray, out hit) && hit.transform.tag == "Floor")
					{
						yPos = hit.point.y - 0.1f;

						Vector3 foliagePosition = new Vector3(xPos, yPos, zPos);

						// Pick a random foliage Prefab
						int randNum = Random.Range(0, 2);
						switch (randNum)
						{
							case 0:
								foliagePrefab = foliagePrefab1;
								break;
							case 1:
								foliagePrefab = foliagePrefab2;
								break;
							default:
								break;
						}

						// Pick a random foliage scale
						float foliageScale = Random.Range(0.05f, 0.2f);

						float yRotation = Random.Range(-180.0f, 180.0f);

						GameObject foliage = Instantiate(this.foliagePrefab, foliagePosition, Quaternion.identity) as GameObject;
						foliage.transform.localScale = new Vector3(foliageScale, foliageScale, foliageScale);
						foliage.transform.Rotate(0.0f, yRotation, 0.0f);
					}
				}
			}
		}
	}
}
