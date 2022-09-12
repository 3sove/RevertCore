using Microsoft.Xna.Framework.Graphics;
using Revert.Core.Extensions;
using Revert.Core.Graphics.Automata.HSL;
using Revert.Core.Graphics.Automata.Kernels;
using Revert.Core.Mathematics;
using Revert.Core.Mathematics.Curves;
using Revert.Core.Mathematics.Interpolations;
using Revert.Core.Mathematics.Operations;
using Revert.Core.Mathematics.Vectors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics
{
    public static class TextureFactory
    {
        public static PolygonSprite getMountains(float width, float height, int mountainPoints, float roughness, float mountainHue, float lightness, float startingHeight, int interpolationLoopCount = 2)
        {
            var mountainHeights = generateMountainHeights(mountainPoints, roughness, startingHeight);
            var vertices = getMountainVertices(mountainHeights, width, height);

            vertices = vertices.constrainedInterpolate(interpolationLoopCount, false);

            vertices = Curve.reducePoints(vertices, Maths.randomFloat(.5f, .8f));
            vertices = Curve.simplifyPoints(vertices);

            var mountainMatrix = getMountainMatrix(width, height, mountainHeights, mountainHue, lightness);
            var texture = mountainMatrix.getPixmap().GetTexture();
            //texture.setFilter(Texture.TextureFilter.Linear, Texture.TextureFilter.Linear);
            var sprite = TexturedPolygon.getTexturedPolygon(vertices, texture);
            sprite.setOrigin(0f, 0f);
            return sprite;
        }

        public static List<Vector2> getMountainVertices(float width, float height, int mountainPoints, float roughness, float startingHeight)
        {
            var mountainHeights = generateMountainHeights(mountainPoints, roughness, startingHeight);
            var vertices = getMountainVertices(mountainHeights, width, height);
            return vertices;
        }

        private static List<Vector2> getMountainVertices(float[] mountainHeights, float width, float height)
        {
            var vertices = new List<Vector2>();
            vertices.Add(new Vector2());

            for (int i = 0; i < mountainHeights.Length; i++)
            {
                var pointHeight = mountainHeights[i];
                var relativeX = i.relativePosition(mountainHeights.Length);
                vertices.Add(new Vector2(relativeX * width, pointHeight * height));
            }
            vertices.Add(new Vector2(width, 0f));
            return vertices;
        }

        private static float[] generateMountainHeights(int width, float roughness, float startingHeight)
        {
            var current = startingHeight;

            var varyingProbability = 0.2f;

            var heights = new float[width];
            if (Maths.randomBoolean(varyingProbability))
                current = current.vary(roughness, Interpolation.smoother, .15f, .95f, false);
            else
                current = current.vary(roughness * .1f, InterpolationPair.NormalDistribution, .15f, .95f, false);

            return heights;
        }

        private static HSLMatrix getMountainMatrix(float width, float height, float[] mountainHeights, float mountainHue, float lightness)
        {
            var groundSaturation = .2f.interpolate(.4f, lightness);

            var mountainMatrix = new HSLMatrix(new HSLMatrixModel((int)width, (int)height, "Mountain Texture"));
            mountainMatrix.populateCells(mountainHue, groundSaturation, lightness, 0.1f, 1f);

            var probabilities = new float[8].fill(1f / 8f);

            var entropy = Maths.randomFloat(.1f, .2f);
            var impact = Maths.randomFloat(.1f, .2f);

            var steps = (width * height) / mountainHeights.Length;
            drawMountains(mountainMatrix, mountainHeights, (int)width, (int)height, 8, 8, probabilities, entropy, impact, .3f, (int)steps);

            mountainMatrix.lightnessDescent(mountainHeights, lightness, lightness.pow(3f), Interpolation.pow4Out);
            mountainMatrix.step(3, true, false);
            return mountainMatrix;
        }

        public static void drawMountains(HSLMatrix groundMatrix, float[] mountainHeights, int width, int height, int brushWidth, int brushHeight, float[] probabilities, float entropy, float impact, float addProbability, int stepsPerRevolution)
        {
            var brush = new GaussianKernel(brushWidth, brushHeight);

            for (int i = 0; i < mountainHeights.Length; i++)
            {
                var randomOperations = OperationsExtensions.GetRandomAddOrSubtract(addProbability);
                var groundWanderer = new HSLWanderer(groundMatrix.cells, randomOperations, entropy, impact, probabilities, true, false);
                groundWanderer.rememberDirection = true;
                groundWanderer.WanderingCell.HuePermeability = .2f;

                var relativeX = i.relativePosition(mountainHeights.Length);
                var startX = (relativeX * width);
                var startY = (mountainHeights[relativeX.getIndex(mountainHeights)] * height);

                groundWanderer.kernel = brush;
                groundWanderer.meander((int)startX, (int)startY, stepsPerRevolution);
            }
        }


    }
}
