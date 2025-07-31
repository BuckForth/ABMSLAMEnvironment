using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ABMMapTile{

	public float regrowthRate = 0.1f;

	public float  maxFood = 1f;
	public float currFood = 0.5f;

	public float elevation = 0.5f;

	public ABMMapTile(float regrowthRate, float elevation)
	{
		this.regrowthRate = regrowthRate;
		this.elevation = elevation;
	}

	public float getMinMapTileColor()
	{
		return currFood/maxFood;
	}

	public void tileUpdate()
	{
		currFood += (regrowthRate);
		if (currFood > maxFood) {
			currFood = maxFood;
		}
	}

	public float isEaten()
	{
		return isEaten(0.333f);
	}

	public float isEaten(float greedIndex)
	{
		float availableFood = currFood * greedIndex;
		currFood -= availableFood;
		return availableFood;
	}
}
