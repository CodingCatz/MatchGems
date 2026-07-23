namespace MatchGems.Core
{
    /// <summary>
    /// 寶石單體資料
    /// </summary>
    public class GemData
    {
        #region 公開屬性
        /// <summary>
        /// 顏色種類
        /// </summary>
        public GemType Color { get; }
        /// <summary>
        /// 特殊能力
        /// </summary>
        public GemPower Power { get; }
        /// <summary>
        /// 是否為特殊寶石
        /// </summary>
        public bool IsSpecial => Power != GemPower.Normal;
        #endregion 公開屬性

        #region 建構式
        public GemData(GemType color, GemPower power = GemPower.Normal)
        {
            Color = color;
            Power = power;
        }
        #endregion 建構式
    }
}


