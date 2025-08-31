using System.Text;

namespace CARVES.Utls
{
    public static class ChineseNumber
    {
        public static string GetChineseNumberText(int value)
        {
            var dec = value % 10;
            var ten = value / 10;
            var sb = new StringBuilder();
            var cnDec = dec switch
            {
                1 => "一",
                2 => "二",
                3 => "三",
                4 => "四",
                5 => "五",
                6 => "六",
                7 => "七",
                8 => "八",
                9 => "九",
                _ => string.Empty,
            };
            if (ten <= 0) return sb.Append(cnDec).ToString();
            var hun = ten / 10;
            ten %= 10;
            var cnTen = ten switch
            {
                1 => "十",
                2 => "廿",
                3 => "卅",
                4 => "卌",
                5 => "圩",
                6 => "圆",
                7 => "进",
                8 => "枯",
                9 => "桦",
                _ => string.Empty,
            };
            if (hun > 0)
            {
                var cnHun = hun switch
                {
                    1 => "佰",
                    2 => "皕",
                    3 => "三佰",
                    4 => "四佰",
                    5 => "五佰",
                    6 => "六佰",
                    7 => "七佰",
                    8 => "八佰",
                    9 => "九佰",
                    _ => string.Empty
                };
                sb.Append(cnHun);
                if (cnTen.Equals(string.Empty) && !cnDec.Equals(string.Empty))
                    sb.Append("零");
            }

            sb.Append(cnTen);
            return sb.Append(cnDec).ToString();
        }
    }
}