using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace autonet.Extensions {
    public static class ColorExtensions {
        public static System.Drawing.Color ToDrawingColor(this Autodesk.AutoCAD.Colors.Color c) {
            var oc = System.Drawing.Color.FromArgb(c.Red, c.Green, c.Blue);
            return oc;
        }
    }

    public class ColorRepresentive {
        public byte Red;
        public byte Green;
        public byte Blue;
    }

    public class DrawingColorConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return (objectType == typeof(System.Drawing.Color) || (objectType == typeof(Autodesk.AutoCAD.Colors.Color)));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            if (value is Autodesk.AutoCAD.Colors.Color c)
                value = c.ToDrawingColor();
            writer.WriteValue(((System.Drawing.Color) value).ToArgb());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if (existingValue is Autodesk.AutoCAD.Colors.Color c)
                existingValue = c.ToDrawingColor();
            return System.Drawing.Color.FromArgb(Convert.ToInt32(reader.Value));
        }
    }
}