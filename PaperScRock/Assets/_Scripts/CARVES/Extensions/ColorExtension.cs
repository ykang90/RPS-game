using System.Drawing;
using System.Text;

namespace CARVES.Utls
{
    public static class ColorExtension
    {
            public static StringBuilder Sb(this object obj) => new(obj.ToString());
            public static StringBuilder Bold(this StringBuilder text)
            {
                text.Insert(0, "<b>");
                text.Append("</b>");
                return text;
            }
            public static StringBuilder Color(this StringBuilder text, Color color)
            {
                var colorText = $"<color={HexConverter(color)}>";
                text.Insert(0, colorText);
                text.Append("</color>");
                return text;
            }
            public static StringBuilder Color(this StringBuilder text, UnityEngine.Color color)
            {
                var colorText = $"<color={HexConverter(color)}>";
                text.Insert(0, colorText);
                text.Append("</color>");
                return text;
            }
            public static StringBuilder Color(this StringBuilder text, UnityEngine.Color32 color)
            {
                var colorText = $"<color={HexConverter(color)}>";
                text.Insert(0, colorText);
                text.Append("</color>");
                return text;
            }
            static string HexConverter(Color c) => "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
            static string HexConverter(UnityEngine.Color c) => "#" + c.r.ToString("X2") + c.g.ToString("X2") + c.b.ToString("X2");
            static string HexConverter(UnityEngine.Color32 c) => "#" + c.r.ToString("X2") + c.g.ToString("X2") + c.b.ToString("X2");
    }
}