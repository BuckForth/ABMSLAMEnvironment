using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ABMMetricChart : MonoBehaviour {
	public Vector2Int textureSize;

	public int boarderSpace = 5;

	private Texture2D graphTexture;
	private Color[] blankPixels;

	public int lineThickness;
	private Color[] dot;

	void Start()
	{
		graphTexture = new Texture2D (textureSize.x, textureSize.y);
		graphTexture.filterMode = FilterMode.Point;

		dot = new Color[(lineThickness) * (lineThickness*2)];
		for(int ii = 0; ii < dot.Length; ii++) {
			dot[ii] = Color.black;
		}

		blankPixels = graphTexture.GetPixels ();
	}

	public void updateTexture(List<int> values, int maxCount)
	{
		//First, reset the background to white
		GetComponent<Renderer>().material.mainTexture = graphTexture;
		graphTexture.SetPixels (blankPixels);
		int largestValue = int.MinValue;
		int smallestValue = int.MaxValue;
		foreach (int value in values) {
			if (value > largestValue) {
				largestValue = value;
			}
			if (value < smallestValue) {
				smallestValue = value;
			}
		}
		int xSpace = textureSize.x - (2 * boarderSpace);
		int ySpace = textureSize.y - (2 * boarderSpace);
		int limit = Mathf.Min (values.Count-1, maxCount);

		for (int px = boarderSpace; px < textureSize.x - boarderSpace; px++) {
			int index = (int)(((float)(px - boarderSpace) / (float)xSpace) * limit);
			int py = (int)(/*(ySpace + boarderSpace) - */( ((float)(values[index]-smallestValue) / ((float)largestValue-smallestValue)) * ySpace + boarderSpace) );

			graphTexture.SetPixel (px, py, Color.black);
			graphTexture.SetPixel (px+1, py+1, Color.black);
			graphTexture.SetPixel (px+1, py-1, Color.black);
			graphTexture.SetPixel (px-1, py+1, Color.black);
			graphTexture.SetPixel (px-1, py-1, Color.black);
		}

		graphTexture.Apply();
	}

	public void updateTexturef(List<float> values, int maxCount)
	{
		//First, reset the background to white
		GetComponent<Renderer>().material.mainTexture = graphTexture;
		graphTexture.SetPixels (blankPixels);
		float largestValue = float.MinValue;
		float smallestValue = float.MaxValue;
		foreach (float value in values) {
			if (value > largestValue) {
				largestValue = value;
			}
			if (value < smallestValue) {
				smallestValue = value;
			}
		}
		//Debug.Log (largestValue);
		int xSpace = textureSize.x - (2 * boarderSpace);
		int ySpace = textureSize.y - (2 * boarderSpace);
		int limit = Mathf.Min (values.Count-1, maxCount);

		for (int px = boarderSpace; px < textureSize.x - boarderSpace; px++) {
			int index = (int)(((float)(px - boarderSpace) / (float)xSpace) * limit);
			int py = (int)(/*(ySpace + boarderSpace) - */( ((float)(values[index]-smallestValue) / ((float)largestValue-smallestValue)) * ySpace + boarderSpace) );
			graphTexture.SetPixel (px, py, Color.black);
			graphTexture.SetPixel (px+1, py+1, Color.black);
			graphTexture.SetPixel (px+1, py-1, Color.black);
			graphTexture.SetPixel (px-1, py+1, Color.black);
			graphTexture.SetPixel (px-1, py-1, Color.black);
		}
		graphTexture.Apply();
	}


	public void updateTexture2(List<int> values, int maxCount)
	{
		//First, reset the background to white
		graphTexture = new Texture2D(textureSize.x, textureSize.y);
		graphTexture.filterMode = FilterMode.Point;
		GetComponent<Renderer>().material.mainTexture = graphTexture;
		int largestValue = int.MinValue;
		foreach (int value in values) {
			if (value > largestValue) {
				largestValue = value;
			}
		}

		int xSpace = textureSize.x - (2 * boarderSpace);
		int ySpace = textureSize.y - (2 * boarderSpace);
		int limit = Mathf.Min (values.Count, maxCount);

		for (int ix = 0; ix < limit; ix++) {
			int pixelX = (int)(((float)ix / (float)maxCount) * xSpace + boarderSpace);
			int pixelY = (int)(((float)values[ix]/(float)largestValue) * ySpace + boarderSpace);
			graphTexture.SetPixel (pixelX, pixelY, Color.black);
		}

		graphTexture.Apply();
	}
}
