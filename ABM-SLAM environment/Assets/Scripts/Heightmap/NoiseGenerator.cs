using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class NoiseGenerator
{
    public int seedX = 9122023;  //random seed value for noise
    public int seedY = 3202219;  //random seed value for noise

    public Octive[] octives;

    public float getHeight(float xx, float yy)
    {
        float rval = 0f;
        foreach (Octive oct in octives)
        {
            rval += (oct.getHeight(xx, yy, seedX, seedY));
        }
        return rval;
    }

    [System.Serializable]
    public class Octive
    {
        public float magnitude = 1;
        public float scale = 10f;
        public float offset = 0f;
        public float power = 1f;
        public Octive subOctive = null;

        public float getHeight(float px, float py, float seedOffsetX, float seedOffsetY)
        {
            float rval = 0f;
            float xPos = seedOffsetX + ((float)px / (float)scale);
            float yPos = seedOffsetY + ((float)py / (float)scale);
            rval = Mathf.PerlinNoise(xPos, yPos) * (float)magnitude + (float)offset;
            float absVal = Mathf.Abs(rval);
            float sign = rval / absVal;
            rval = ((Mathf.Pow(absVal, power)) * sign);
            if (subOctive != null)
            {
                rval += subOctive.getHeight(px, py, seedOffsetX, seedOffsetY);
            }
            return rval;
        }
    }
}



