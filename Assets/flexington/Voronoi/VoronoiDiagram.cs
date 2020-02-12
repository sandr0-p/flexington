using System;
using System.Linq;
using System.Collections.Generic;
using flexington.Tools;
using UnityEngine;

namespace flexington.Voronoi
{
    /// <summary>
    /// Custom implementation of a Voronoi Diagram.
    /// This implementation is creating squares rather than polygons and 
    /// it will leave empty spaces in case a region can't expand anymore.
    /// </summary>
    public class VoronoiDiagram
    {
        /// <summary>
        /// The Size of the Voronoi Diagram in Pixels
        /// </summary>
        public Vector2Int Size
        {
            get { return _size; }
        }
        private Vector2Int _size;

        /// <summary>
        /// The seed used to instantiate the random number generator
        /// </summary>
        public string Seed
        {
            get { return _seed; }
        }
        private string _seed;

        /// <summary>
        /// Number of different regions
        /// </summary>
        private int _numberOfRegions;

        /// <summary>
        /// List of all regions
        /// </summary>
        private List<VoronoiRegion> _regions;

        /// <summary>
        /// Random Number Generator
        /// </summary>
        private System.Random _rng;

        /// <summary>
        /// The texture generated by this class
        /// </summary>
        Texture2D _texture;

        /// <summary>
        /// Create a new Voronoi Diagram
        /// </summary>
        public VoronoiDiagram(int numberOfRegions, Vector2Int size = default, string seed = default)
        {
            _numberOfRegions = numberOfRegions;

            if (size == default) size = new Vector2Int(_numberOfRegions * 15, _numberOfRegions * 15);
            _size = size;

            if (seed == null || seed == string.Empty) seed = DateTime.Now.ToString();
            _seed = seed;
            _rng = new System.Random(_seed.GetHashCode());

            _regions = new List<VoronoiRegion>();

            GenerateRegions();
        }

        /// <summary>
        /// Generates the different regions and assignes a random
        /// location and color to it. 
        /// </summary>
        private void GenerateRegions()
        {
            for (int i = 0; i < _numberOfRegions; i++)
            {
                _regions.Add(new VoronoiRegion
                {
                    Rect = new Rect(_rng.Next(0, _size.x - 1), _rng.Next(0, _size.y - 1), 1, 1),
                    Color = ColorUtil.GetRandomColor()
                });
            }
        }

        /// <summary>
        /// Simulates the expansion of the regions. After every expansion
        /// the regions checks if it overlaps with another region or the
        /// outer boundries.
        /// </summary>
        public void Simulate()
        {
            while (_regions.Count(r => r.CanGrow) > 0)
            {
                for (int i = 0; i < _regions.Count; i++)
                {
                    VoronoiRegion region = _regions[i];
                    region.GrowUniform(1f);
                    for (int j = 0; j < _regions.Count; j++)
                    {
                        if (i == j) continue;
                        region.Overlaps(_regions[j]);
                    }
                    region.Overlaps(_size);
                }
            }
        }

        /// <summary>
        /// Generates the textures based on the size and the color of all
        /// regions. For the very rare case that two regions occupy the
        /// same pixel, the last region wins.
        /// </summary>
        /// <returns></returns>
        public Texture2D GenerateTexture()
        {
            Color[] pixelColors = new Color[_size.x * _size.y];
            _texture = new Texture2D(_size.x, _size.y);
            _texture.filterMode = FilterMode.Point;

            for (int i = 0; i < pixelColors.Length; i++)
            {
                pixelColors[i] = Color.white;
            }

            for (int y = 0; y < _size.y; y++)
            {
                for (int x = 0; x < _size.x; x++)
                {
                    int index = x * _size.x + y;
                    for (int i = 0; i < _regions.Count; i++)
                    {
                        if (_regions[i].Rect.Contains(new Vector2(x, y)))
                        {
                            pixelColors[index] = _regions[i].Color;
                        }
                    }
                }
            }
            _texture.SetPixels(pixelColors);
            _texture.Apply();
            return _texture;
        }
    }
}