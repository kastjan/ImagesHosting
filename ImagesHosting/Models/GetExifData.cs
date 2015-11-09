using ExifLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImagesHosting.Models
{
    //Data format. Uset to form JSON AJAX answers
    public class JSONDataFormat
    {
        public string parameter { get; set; }
        public string data { get; set; }
    }

    //Decode Exif information from image
    public class GetExifData
    {
        private static string RenderTag(object tagValue)
        {
            var array = tagValue as Array;
            if (array != null)
            {
                if (array.Length > 20 && array.GetType().GetElementType() == typeof(byte))
                    return "0x" + string.Join("", array.Cast<byte>().Select(x => x.ToString("X2")).ToArray());

                return string.Join(" ", array.Cast<object>().Select(x => x.ToString()).ToArray());
            }

            return tagValue.ToString();
        }
        //Send list JSON objects with full image Exif information 
        public List<JSONDataFormat> GetFullEXIF(ImageBase img)
        {
            List<JSONDataFormat> data = new List<JSONDataFormat>();
            try
            {
                using (var reader = new ExifReader(img.url))
                {
                    var props = Enum.GetValues(typeof(ExifTags)).Cast<ushort>().Select(tagID =>
                    {
                        object val;
                        if (reader.GetTagValue(tagID, out val))
                        {
                            if (val is double)
                            {
                                int[] rational;
                                if (reader.GetTagValue(tagID, out rational))
                                    val = string.Format("{0} ({1}/{2})", val, rational[0], rational[1]);
                            }
                            var inf = new JSONDataFormat();
                            inf.parameter = Enum.GetName(typeof(ExifTags), tagID);
                            inf.data = RenderTag(val);
                            return inf;
                        }

                        return null;

                    }).Where(x => x != null).ToList();
                    data.Add(new JSONDataFormat { parameter = "EXIF FROM IMAGE", data = null });
                    data.AddRange(props);
                }
            }
            catch (Exception ex)
            {
                data.Add(new JSONDataFormat { parameter = "EXIF FROM IMAGE", data = ex.Message.ToString() });
            }
            return data;
        }

        //Send list JSON objects with image GPS coordinats  
        public List<JSONDataFormat> GetGPS(ImageBase img)
        {
            var gps = new List<JSONDataFormat>();
            try
            {
                using (var reader = new ExifReader(img.url))
                {
                    object val;
                    reader.GetTagValue(ExifTags.GPSLatitudeRef, out val);
                    gps.Add(new JSONDataFormat { parameter = "GPSLatitudeRef", data = RenderTag(val) });
                    reader.GetTagValue(ExifTags.GPSLatitude, out val);
                    gps.Add(new JSONDataFormat { parameter = "GPSLatitude", data = RenderTag(val) });
                    reader.GetTagValue(ExifTags.GPSLongitudeRef, out val);
                    gps.Add(new JSONDataFormat { parameter = "GPSLongitudeRef", data = RenderTag(val) });
                    reader.GetTagValue(ExifTags.GPSLongitude, out val);
                    gps.Add(new JSONDataFormat { parameter = "GPSLongitude", data = RenderTag(val) });
                }
            }
            catch (Exception ex)
            {
                gps.Add(new JSONDataFormat { parameter = null, data = ex.Message.ToString() });
            }
            return gps;
        }

    }
}