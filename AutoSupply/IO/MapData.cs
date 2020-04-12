using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSupply.IO
{
    public class MapData
    {
        public class Area
        {
            public int MinX { get; }

            public int MinY { get; }

            public int MaxX { get; }

            public int MaxY { get; }

            public Area(int minX, int minY, int maxX, int maxY)
            {
                MinX = minX;
                MinY = minY;
                MaxX = maxX;
                MaxY = maxY;
            }

            public bool ContainsPoint(Vector2 point)
            {
                return ContainsPoint(point.X, point.Y);
            }

            public bool ContainsPoint(float x, float y)
            {
                return MinX <= x && MinY <= y && x <= MaxX && y <= MaxY;
            }
        }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("blacklist")]
        public List<Area> BlackList { get; private set; } = new List<Area>();

        [JsonProperty("whitelist")]
        public List<Area> WhiteList { get; private set; } = new List<Area>();
    }
}
