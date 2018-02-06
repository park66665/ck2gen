using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibNoise;
using LibNoise.Modfiers;
using LibNoise.Modifiers;

namespace CrusaderKingsStoryGen.MapGen
{
    public class TerrainMap
    {
        Perlin terrainType = new Perlin();
        RidgedMultifractal mountainTerrain = new RidgedMultifractal();
        Billow baseLandTerrain = new Billow();
        Billow baseWaterTerrain = new Billow();
        private NoiseTexture output;
        public NoiseTexture ResultBitmap { get; set; }
        private double seaLevel = 0.07f;
        public float ZoomMultiplier = 1.7f;
        public float MinLandFreq = 0.1f;
        public float MaxLandFreq = 1f;

        public TerrainMap(LockBitmap landBitmap = null, LockBitmap hillBitmap = null, LockBitmap mountainBitmap = null)
        {
            LandBitmap = landBitmap;

            MountainBitmap = mountainBitmap;
        }

        public LockBitmap LandBitmap { get; set; }
        public LockBitmap MountainBitmap { get; set; }

        public void Init(int seed, int width, int height)
        {
            if (LandBitmap != null && width != LandBitmap.Width)
            {
                LandBitmap.ResizeImage(width, height, false);
              
            }
       
            if (MountainBitmap != null && width != MountainBitmap.Width)
            {
                MountainBitmap.ResizeImage(width, height, false);
             }
            float delta = width / 3072.0f;
            delta *= ZoomMultiplier;
            DivNoise = delta;

            //   Rand.SetSeed(seed);
            baseWaterTerrain.Frequency = 2.0;
            baseLandTerrain.Frequency = (2.0);
            ScaleBiasOutput flatTerrain = new ScaleBiasOutput(baseLandTerrain);
            flatTerrain.Scale = 0.005;
            flatTerrain.Bias = seaLevel;//SeaLevel;
            MinLandFreq = 0.2f;
            MaxLandFreq = 1f;
            if (LandBitmap != null)
                MinLandFreq = 0.7f;

            ScaleBiasOutput hillTerrain = new ScaleBiasOutput(baseLandTerrain);
            hillTerrain.Scale = 0.09;
            hillTerrain.Bias = seaLevel + 0.2;//SeaLevel;

            ScaleBiasOutput waterTerrain = new ScaleBiasOutput(baseWaterTerrain);
            waterTerrain.Bias = -0.33f;//SeaLevel;
            waterTerrain.Scale = 0.001;

            Perlin waterLandType = new Perlin();
            float landFreq = Rand.Next((int) (MinLandFreq*10000), (int) (MaxLandFreq*10000))/10000.0f;
            waterLandType.Persistence = 0.45;
            waterLandType.Frequency = landFreq;
            //waterLandType.OctaveCount = 12;
            waterLandType.Seed = Rand.Next(1000000);

            Select waterLandSelector = new Select(waterLandType, waterTerrain, flatTerrain);

            if (LandBitmap != null)
            {
                waterLandSelector = new BitmapSelect(waterLandType, waterTerrain, flatTerrain, DivNoise, LandBitmap);
             }
            waterLandSelector.EdgeFalloff = (0.145);
            waterLandSelector.SetBounds(-0.0, 1000); ;


            Select landHillSelector = new Select(waterLandType, waterLandSelector, hillTerrain);
      
            if (LandBitmap != null)
            {
                landHillSelector = new BitmapSelect(waterLandType, waterLandSelector, hillTerrain, DivNoise, LandBitmap);
             }
            landHillSelector.EdgeFalloff = (0.45);
            landHillSelector.SetBounds(0.25f, 1000); ;

            terrainType.Persistence = 0.3;
            terrainType.Frequency = 0.3;
            terrainType.Seed = Rand.Next(10000000);

            var clamp = new ClampOutput(terrainType);
            clamp.SetBounds(0, 1);
            //            mountainTerrain.Frequency /= 1.5f;
            mountainTerrain.Lacunarity = 35;
            mountainTerrain.Frequency = 3.2;
            mountainTerrain.Seed = Rand.Next(10000000);
            MultiplyPositive mul = new MultiplyPositive(waterLandType, waterLandType);

            ScaleOutput scaled = new ScaleOutput(mul, 0.00001);

            Add add = new Add(new BiasOutput(mountainTerrain, 0.8 + seaLevel), landHillSelector);
       
            MultiplyPositive mul2 = new MultiplyPositive(add, add);
            MultiplyPositive mul3 = new MultiplyPositive(clamp, mul);

            Select terrainSelector = new Select(mul3, landHillSelector, add);
        
            if (MountainBitmap != null)
            {
                terrainSelector = new BitmapSelect(mul3, landHillSelector, add, DivNoise, MountainBitmap);
             }
            terrainSelector.EdgeFalloff = (7.925);
            terrainSelector.SetBounds(0.3, 1000);

            Turbulence finalTerrain = new Turbulence(terrainSelector);
            finalTerrain.Frequency = 4;
            finalTerrain.Power = 0.075;
            Width = width;
            Height = height;
            //   ResultBitmap2 = new NoiseTexture(width, height, clamp);
            //   System.Console.Out.WriteLine("Left: " + ResultBitmap2.minRange + " - " + ResultBitmap2.maxRange);

            //   ResultBitmap2 = new NoiseTexture(width, height, finalTerrain, DivNoise, 1.25f, -0.66f);
            //    System.Console.Out.WriteLine("Left: " + ResultBitmap2.minRange + " - " + ResultBitmap2.maxRange);
            ResultBitmap = new NoiseTexture(width, height, finalTerrain, DivNoise, 1.25f, -0.66f);
            System.Console.Out.WriteLine("Right: " + ResultBitmap.minRange + " - " + ResultBitmap.maxRange);
        }

        public void InitGen(int seed, int width, int height)
        {
            if (LandBitmap != null && width != LandBitmap.Width)
            {
                LandBitmap.ResizeImage(width, height, false);

            }

            if (MountainBitmap != null && width != MountainBitmap.Width)
            {
                MountainBitmap.ResizeImage(width, height, false);
            }
            float delta = width / 3072.0f;
            delta *= ZoomMultiplier;
            DivNoise = delta;

            //   Rand.SetSeed(seed);
            baseWaterTerrain.Frequency = 2.0;
            baseLandTerrain.Frequency = (2.0);
            ScaleBiasOutput flatTerrain = new ScaleBiasOutput(baseLandTerrain);
            flatTerrain.Scale = 0.005;
            flatTerrain.Bias = seaLevel;//SeaLevel;
            MinLandFreq = 0.2f;
            MaxLandFreq = 1f;
            if (LandBitmap != null)
                MinLandFreq = 0.7f;

            ScaleBiasOutput hillTerrain = new ScaleBiasOutput(baseLandTerrain);
            hillTerrain.Scale = 0.09;
            hillTerrain.Bias = seaLevel + 0.2;//SeaLevel;

            ScaleBiasOutput waterTerrain = new ScaleBiasOutput(baseWaterTerrain);
            waterTerrain.Bias = -0.33f;//SeaLevel;
            waterTerrain.Scale = 0.001;

            Perlin waterLandType = new Perlin();
            float landFreq = Rand.Next((int)(MinLandFreq * 10000), (int)(MaxLandFreq * 10000)) / 10000.0f;
            waterLandType.Persistence = 0.45;
            waterLandType.Frequency = landFreq;
            //waterLandType.OctaveCount = 12;
            waterLandType.Seed = Rand.Next(1000000);

            Select waterLandSelector = new Select(waterLandType, waterTerrain, flatTerrain);

            waterLandSelector.EdgeFalloff = (0.145);
            waterLandSelector.SetBounds(-0.0, 1000); ;


            Select landHillSelector = new Select(waterLandType, waterLandSelector, hillTerrain);

        
            landHillSelector.EdgeFalloff = (0.45);
            landHillSelector.SetBounds(0.25f, 1000); ;

            terrainType.Persistence = 0.3;
            terrainType.Frequency = 0.3;
            terrainType.Seed = Rand.Next(10000000);

            var clamp = new ClampOutput(terrainType);
            clamp.SetBounds(0, 1);
            //            mountainTerrain.Frequency /= 1.5f;
            mountainTerrain.Lacunarity = 35;
            mountainTerrain.Frequency = 3.2;
            mountainTerrain.Seed = Rand.Next(10000000);
            MultiplyPositive mul = new MultiplyPositive(waterLandType, waterLandType);

            ScaleOutput scaled = new ScaleOutput(mul, 0.00001);

            Add add = new Add(new BiasOutput(mountainTerrain, 0.8 + seaLevel), landHillSelector);

            MultiplyPositive mul2 = new MultiplyPositive(add, add);
            MultiplyPositive mul3 = new MultiplyPositive(clamp, mul);

            Select terrainSelector = new Select(mul3, landHillSelector, add);

            terrainSelector.EdgeFalloff = (0.325);
            terrainSelector.SetBounds(0.28, 1000);

            Turbulence finalTerrain = new Turbulence(terrainSelector);
            finalTerrain.Frequency = 4;
            finalTerrain.Power = 0.075;
            Width = width;
            Height = height;
            //   ResultBitmap2 = new NoiseTexture(width, height, clamp);
            //   System.Console.Out.WriteLine("Left: " + ResultBitmap2.minRange + " - " + ResultBitmap2.maxRange);

            //   ResultBitmap2 = new NoiseTexture(width, height, finalTerrain, DivNoise, 1.25f, -0.66f);
            //    System.Console.Out.WriteLine("Left: " + ResultBitmap2.minRange + " - " + ResultBitmap2.maxRange);
            ResultBitmap = new NoiseTexture(width, height, finalTerrain, DivNoise, 1.25f, -0.66f);
            System.Console.Out.WriteLine("Right: " + ResultBitmap.minRange + " - " + ResultBitmap.maxRange);
        }
        public void InitFromExisting(int seed, int width, int height, float[,] selectionMap)
        {
            float delta = width / 3072.0f;
            delta *= ZoomMultiplier;
            DivNoise = delta;

            //   Rand.SetSeed(seed);
            baseWaterTerrain.Frequency = 2.0;
            baseLandTerrain.Frequency = (2.0);
            ScaleBiasOutput flatTerrain = new ScaleBiasOutput(baseLandTerrain);
            flatTerrain.Scale = 0.005;
            flatTerrain.Bias = seaLevel;//SeaLevel;

            ScaleBiasOutput hillTerrain = new ScaleBiasOutput(baseLandTerrain);
            hillTerrain.Scale = 0.065;
            hillTerrain.Bias = seaLevel + 0.2;//SeaLevel;

            ScaleBiasOutput waterTerrain = new ScaleBiasOutput(baseWaterTerrain);
            waterTerrain.Bias = -0.73f;//SeaLevel;
            waterTerrain.Scale = 0.05;

            Perlin waterLandType = new Perlin();

            waterLandType.Persistence = 0.45;
            waterLandType.Frequency = 0.5;
            //waterLandType.OctaveCount = 12;
            waterLandType.Seed = Rand.Next(1000000);
            Select waterLandSelector = new Select(waterLandType, waterTerrain, flatTerrain);
            waterLandSelector.EdgeFalloff = (0.045);
            waterLandSelector.SetBounds(-0.0, 1000); ;

            Select landHillSelector = new Select(waterLandType, waterLandSelector, hillTerrain);
            landHillSelector.EdgeFalloff = (0.15);
            landHillSelector.SetBounds(0.4, 1000); ;


            terrainType.Persistence = 0.3;
            terrainType.Frequency = 0.3;
            terrainType.Seed = Rand.Next(10000000);

            var clamp = new ClampOutput(terrainType);
            clamp.SetBounds(0, 1);
            //            mountainTerrain.Frequency /= 1.5f;
            mountainTerrain.Lacunarity = 30;
            mountainTerrain.Frequency = 1.9;
            MultiplyPositive mul = new MultiplyPositive(waterLandType, waterLandType);

            ScaleOutput scaled = new ScaleOutput(mul, 0.00001);

            Add add = new Add(new BiasOutput(mountainTerrain, 1 + seaLevel), landHillSelector);

            MultiplyPositive mul2 = new MultiplyPositive(mul, mul);
            MultiplyPositive mul3 = new MultiplyPositive(clamp, mul);

            Select terrainSelector = new Select(mul3, landHillSelector, add);
            terrainSelector.EdgeFalloff = (0.425);

            terrainSelector.SetBounds(0.2, 1000);

            Turbulence finalTerrain = new Turbulence(terrainSelector);
            finalTerrain.Frequency = 4;
            finalTerrain.Power = 0.075;
            Width = width;
            Height = height;
            //   ResultBitmap2 = new NoiseTexture(width, height, clamp);
            //   System.Console.Out.WriteLine("Left: " + ResultBitmap2.minRange + " - " + ResultBitmap2.maxRange);

            //   ResultBitmap2 = new NoiseTexture(width, height, finalTerrain, DivNoise, 1.25f, -0.66f);
            //    System.Console.Out.WriteLine("Left: " + ResultBitmap2.minRange + " - " + ResultBitmap2.maxRange);
            ResultBitmap = new NoiseTexture(width, height, finalTerrain, DivNoise, 1.25f, -0.66f);
            System.Console.Out.WriteLine("Right: " + ResultBitmap.minRange + " - " + ResultBitmap.maxRange);
        }
        public void InitO(int seed, int width, int height)
        {
            float delta = width / 3072.0f;
            delta *= ZoomMultiplier;
            DivNoise = delta;

            //   Rand.SetSeed(seed);
            baseWaterTerrain.Frequency = 2.0;
            baseLandTerrain.Frequency = (2.0);
            ScaleBiasOutput flatTerrain = new ScaleBiasOutput(baseLandTerrain);
            flatTerrain.Scale = 0.005;
            flatTerrain.Bias = seaLevel;//SeaLevel;

            ScaleBiasOutput hillTerrain = new ScaleBiasOutput(baseLandTerrain);
            hillTerrain.Scale = 0.065;
            hillTerrain.Bias = seaLevel + 0.2;//SeaLevel;

            ScaleBiasOutput waterTerrain = new ScaleBiasOutput(baseWaterTerrain);
            waterTerrain.Bias = -0.73f;//SeaLevel;
            waterTerrain.Scale = 0.05;

            Perlin waterLandType = new Perlin();

            waterLandType.Persistence = 0.45;
            waterLandType.Frequency = 0.5;
            //waterLandType.OctaveCount = 12;
            waterLandType.Seed = Rand.Next(1000000);
            Select waterLandSelector = new Select(waterLandType, waterTerrain, flatTerrain);
            waterLandSelector.EdgeFalloff = (0.045);
            waterLandSelector.SetBounds(-0.0, 1000); ;

            Select landHillSelector = new Select(waterLandType, waterLandSelector, hillTerrain);
            landHillSelector.EdgeFalloff = (0.15);
            landHillSelector.SetBounds(0.4, 1000); ;


            terrainType.Persistence = 0.3;
            terrainType.Frequency = 0.3;
            terrainType.Seed = Rand.Next(10000000);

            var clamp = new ClampOutput(terrainType);
            clamp.SetBounds(0, 1);
            //            mountainTerrain.Frequency /= 1.5f;
            mountainTerrain.Lacunarity = 30;
            mountainTerrain.Frequency = 1.3;
            MultiplyPositive mul = new MultiplyPositive(waterLandType, waterLandType);

            ScaleOutput scaled = new ScaleOutput(mul, 0.00001);

            Add add = new Add(new BiasOutput(mountainTerrain, 1 + seaLevel), landHillSelector);

            MultiplyPositive mul2 = new MultiplyPositive(mul, mul);
            MultiplyPositive mul3 = new MultiplyPositive(clamp, mul);

            Select terrainSelector = new Select(mul3, landHillSelector, add);
            terrainSelector.EdgeFalloff = (0.425);

            terrainSelector.SetBounds(0.3, 1000);

            Turbulence finalTerrain = new Turbulence(terrainSelector);
            finalTerrain.Frequency = 4;
            finalTerrain.Power = 0.075;
            Width = width;
            Height = height;
            //   ResultBitmap2 = new NoiseTexture(width, height, clamp);
            //   System.Console.Out.WriteLine("Left: " + ResultBitmap2.minRange + " - " + ResultBitmap2.maxRange);

            ResultBitmap = new NoiseTexture(width, height, finalTerrain, DivNoise, 1.25f, -0.66f);
            System.Console.Out.WriteLine("Range: " + ResultBitmap.minRange + " - " + ResultBitmap.maxRange);
        }

        public float DivNoise { get; set; }

        public NoiseTexture ResultBitmap2 { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }
    }
}
