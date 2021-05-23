using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefBrowserBot.Extensions.Downloader
{
    public static class FileHeader
    {
        public static string GetImageExtension(byte[] data)
        {
            if (data.Length > 2 && data[0] == 0x42 && data[1] == 0x4d)
                return ".bmp";
            else if (data.Length > 4 && data[0] == 0xff && data[1] == 0xd8)
                return ".jpg";
            else if (data.Length > 4 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4e && data[3] == 0x47)
                return ".png";
            else if (data.Length > 4 && data[0] == 0x49 && data[1] == 0x49 && data[2] == 0x2a && data[3] == 0x00) // little-endian
                return ".tif";
            else if (data.Length > 4 && data[0] == 0x4d && data[1] == 0x4d && data[2] == 0x00 && data[3] == 0x2a) // big-endian
                return ".tif";
            else if (data.Length > 4 && data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x38)
                return ".gif";
            else if (data.Length > 11 && data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46 && data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50)
                return ".webp";

            return null;
        }
    }
}
