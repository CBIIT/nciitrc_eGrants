using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.UserModel;

namespace EmailConcatenationPOC
{
    internal class ColorConverter
    {
        private static readonly Dictionary<short, string> IndexedColorsToHex = new Dictionary<short, string>
        {
            {(short)NPOI.SS.UserModel.IndexedColors.Black.Index, "000000" },
            {(short)NPOI.SS.UserModel.IndexedColors.White.Index, "FFFFFF" },
            {(short)NPOI.SS.UserModel.IndexedColors.Red.Index, "FF0000" },
            {(short)NPOI.SS.UserModel.IndexedColors.BrightGreen.Index, "00FF00" },
            {(short)NPOI.SS.UserModel.IndexedColors.Blue.Index, "0000FF" },
            {(short)NPOI.SS.UserModel.IndexedColors.Yellow.Index, "FFFF00" },
            {(short)NPOI.SS.UserModel.IndexedColors.Pink.Index, "FFC0CB" },
            {(short)NPOI.SS.UserModel.IndexedColors.Turquoise.Index, "40E0D0" },
            {(short)NPOI.SS.UserModel.IndexedColors.DarkRed.Index, "8B0000" },
            {(short)NPOI.SS.UserModel.IndexedColors.Green.Index, "008000" },
            {(short)NPOI.SS.UserModel.IndexedColors.DarkBlue.Index, "00008b" },
            {(short)NPOI.SS.UserModel.IndexedColors.DarkYellow.Index, "808000" },
            {(short)NPOI.SS.UserModel.IndexedColors.Violet.Index, "EE82EE" },
            {(short)NPOI.SS.UserModel.IndexedColors.Teal.Index, "008080" },
            {(short)NPOI.SS.UserModel.IndexedColors.Grey25Percent.Index, "C0C0C0" },
            {(short)NPOI.SS.UserModel.IndexedColors.Grey50Percent.Index, "808080" },
            {(short)NPOI.SS.UserModel.IndexedColors.CornflowerBlue.Index, "6495ED" },
            {(short)NPOI.SS.UserModel.IndexedColors.Maroon.Index, "800000" },
            {(short)NPOI.SS.UserModel.IndexedColors.LemonChiffon.Index, "FFFACD" },
            {(short)NPOI.SS.UserModel.IndexedColors.SeaGreen.Index, "2E8B57" },
            {(short)NPOI.SS.UserModel.IndexedColors.SkyBlue.Index, "87CEEB" },
            {(short)NPOI.SS.UserModel.IndexedColors.Plum.Index, "DDA0DD" },
            {(short)NPOI.SS.UserModel.IndexedColors.LightTurquoise.Index, "AFEEEE" },
            {(short)NPOI.SS.UserModel.IndexedColors.LightGreen.Index, "90EE90" },
            {(short)NPOI.SS.UserModel.IndexedColors.LightYellow.Index, "FFFFE0" },
            {(short)NPOI.SS.UserModel.IndexedColors.PaleBlue.Index, "ADD8E6" },
            {(short)NPOI.SS.UserModel.IndexedColors.Rose.Index, "FF007F" },
            {(short)NPOI.SS.UserModel.IndexedColors.Lavender.Index, "E6E6FA" },
            {(short)NPOI.SS.UserModel.IndexedColors.Tan.Index, "D2B48C" },
            {(short)NPOI.SS.UserModel.IndexedColors.LightBlue.Index, "ADD8E6" },
            {(short)NPOI.SS.UserModel.IndexedColors.Gold.Index, "FFD700" },
            {(short)NPOI.SS.UserModel.IndexedColors.Orange.Index, "FFA500" },
            {(short)NPOI.SS.UserModel.IndexedColors.BlueGrey.Index, "6699CC" },
            {(short)NPOI.SS.UserModel.IndexedColors.Grey40Percent.Index, "666666" },
            {(short)NPOI.SS.UserModel.IndexedColors.DarkTeal.Index, "008080" },
            {(short)NPOI.SS.UserModel.IndexedColors.Indigo.Index, "4B0082" },
            {(short)NPOI.SS.UserModel.IndexedColors.Automatic.Index, "000000" }
        };

        public static string GetHexColor(short colorIndex)
        {
            if (IndexedColorsToHex.TryGetValue(colorIndex, out string hexColor))
            {
                return hexColor;
            } else
            {
                return "000000";
            }
        }

        // example :
        // short colorIndex = font.Color
        // string hexColor = ColorConverter.GetHexColor(colorIndex);
    }
}
