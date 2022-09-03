using System;
using System.Drawing;
using System.IO;
using System.Text.Json.Serialization;

namespace VpServiceAPI.Entities
{
    public class ArtworkMeta
    {
        public string Name { get; init; }
        public string Color { get; init; }
        [JsonIgnore] public Color FontColor { get; init; }
        [JsonIgnore] public Timespan Timespan { get; init; }
        public ArtworkMeta(string name, string start_date, string end_date, string color, string font_color)
        {
            Name = name;
            Timespan = new Timespan(start_date, end_date);
            Color = color;
            FontColor = font_color switch
            {
                "white" => System.Drawing.Color.White,
                "black" => System.Drawing.Color.Black,
                "gray" => System.Drawing.Color.Gray,
                "red" => System.Drawing.Color.Red,
                "blue" => System.Drawing.Color.Blue,
                "green" => System.Drawing.Color.Green,
                "yellow" => System.Drawing.Color.Yellow,
                "pink" => System.Drawing.Color.Pink,
                _ => System.Drawing.Color.White
            };
        }
        //[JsonConstructor]
        //public ArtworkMeta(string name, string color)
        //{
        //    Name = name;
        //    Color = color;
        //}
    }
    public class Artwork : ArtworkMeta
    {
        public byte[] Image { get; set; }

        public Artwork(string name, byte[] image, string color, string font_color, string start_date, string end_date) : base(name, start_date, end_date, color, font_color)
        {
            Image = image;
        }
        public Bitmap GetBitmap()
        {
            Bitmap bmp;
            using (var ms = new MemoryStream(Image))
            {
                bmp = new Bitmap(ms);
            }
            return bmp;

        }
    }

    
}
