using UnityEngine;

namespace MatchGems.Core
{
    /// <summary>
    /// 待引爆的寶石資料引信
    /// </summary>
    public readonly struct DetonationFuse
    {
        #region 公開屬性
        /// <summary>
        /// 特殊石定位
        /// </summary>
        public CellCoord Coord { get; }
        /// <summary>
        /// 特殊石的資料
        /// </summary>
        public GemData GemData { get; }
        #endregion 公開屬性

        /// <summary>
        /// 建構一顆待爆的引信
        /// </summary>
        public DetonationFuse (CellCoord coord, GemData gemData)
        {
            Coord = coord;
            GemData = gemData;
        }
    }
}