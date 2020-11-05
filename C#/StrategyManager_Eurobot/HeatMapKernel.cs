

using Amplifier.OpenCL;
using System;
using System.Threading.Tasks;

namespace StrategyManager
{
    class HeatMapKernel : OpenCLFunctions
    {
        [OpenCLKernel]
        void ParallelCalculateHeatMap([Global, Output] double[] heatMap, [Global] int[] width, [Global] int[] height,
            [Global] float[] widthTerrain, [Global] float[] heightTerrain, [Global] float[] destinationX, [Global] float[] destinationY)
        {

            int i = get_global_id(0);
            float destXInHeatmap = (float)((float)destinationX[i] / widthTerrain[i] + 0.5) * (width[i] - 1);  //-1 car on a augmenté la taille de 1 pour avoir une figure symétrique
            float destYInHeatmap = (float)((float)destinationY[i] / heightTerrain[i] + 0.5) * (height[i] - 1);  //-1 car on a augmenté la taille de 1 pour avoir une figure symétrique

            float normalizer = height[i];

            for (int y = 0; y < height[i]; y++)
            {
                for (int x = 0; x < width[i]; x++)
                {
                    //Calcul de la fonction de cout de stratégie
                    //heatMap[y, x] = 
                    float sqrtCalc = ((destXInHeatmap - x) * (destXInHeatmap - x) + (destYInHeatmap - y) * (destYInHeatmap - y)) *
                        ((destXInHeatmap - x) * (destXInHeatmap - x) + (destYInHeatmap - y) * (destYInHeatmap - y));
                    sqrtCalc /= normalizer;
                    sqrtCalc = 1 - sqrtCalc;
                    if(0 > sqrtCalc)
                    {
                        //O = 0
                    }
                    else
                    {
                        //O = sqrtCalc
                    }
                }
            }
        }
    }
}
