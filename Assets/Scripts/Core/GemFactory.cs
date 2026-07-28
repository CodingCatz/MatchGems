using UnityEngine;

namespace MatchGems.Core
{
    /// <summary>
    /// 特殊寶石製造工廠(靜態公式)
    /// </summary>
    public static class GemFactory
    {
        /// <summary>
        /// 依照配對長度和方向產生能力寶石
        /// </summary>
        /// <param name="color">顏色</param>
        /// <param name="matchLength">長度</param>
        /// <param name="direction">方向</param>
        /// <returns>能力寶石</returns>
        public static GemData CreateFromMatch(GemType color, int matchLength, MatchDirection direction)
        {
            if (matchLength >= 5) return new GemData(color, GemPower.Rainbow);
            if (matchLength == 4)
            {//依方向決定特殊能力(橫直排)
                GemPower power = 
                    direction == MatchDirection.Horizontal
                    ? GemPower.HLine
                    : GemPower.VLine;
                return new GemData(color, power);
            }
            return new GemData(color, GemPower.Normal);
        }

        public static GemData CreateBomb(GemType color)
        {
            return new GemData(color, GemPower.Bomb);
        }

        /// <summary>
        /// 依照配對資訊建立特殊寶石(優先使用KeyGem)
        /// </summary>
        /// <param name="color"></param>
        /// <param name="matchLength"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static SpecialGemSpawnInfo CreateSpawnInfo(GemType color, int matchLength, MatchDirection direction, bool isSpecial, CellCoord coord)
        {
            if (matchLength < 4) return SpecialGemSpawnInfo.None;
            GemData gemData = CreateFromMatch(color, matchLength, direction);
            //計算KeyGem的座標
            CellCoord spawnCoord = coord;
            return new SpecialGemSpawnInfo(true, gemData, spawnCoord);
        }
    }
}