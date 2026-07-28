using System.Collections.Generic;
using UnityEngine;

namespace MatchGems.Core
{
    /// <summary>
    /// 消除配方向
    /// </summary>
    public enum MatchDirection
    {
        /// <summary>
        /// 橫向
        /// </summary>
        Horizontal, 
        /// <summary>
        /// 直向
        /// </summary>
        Vertical
    }
    /// <summary>
    /// 連線的寶石配對資料(單線)
    /// </summary>
    public class MatchLine
    {
        #region 唯讀紀錄
        /// <summary>
        /// 連線的棋盤座標清單
        /// </summary>
        private readonly List<CellCoord> _coords;
        #endregion 唯讀紀錄

        #region 公開資訊
        /// <summary>
        /// 連線座標組合(單條)公開接口
        /// </summary>
        public IReadOnlyList<CellCoord> Coords => _coords;
        /// <summary>
        /// 連線方向
        /// </summary>
        public MatchDirection Direction { get; }
        /// <summary>
        /// 連線的顏色
        /// </summary>
        public GemType Color { get; }
        /// <summary>
        /// 連線長度(資料紀錄數量)
        /// </summary>
        public int Length => _coords.Count;
        /// <summary>
        /// 中間石座標
        /// </summary>
        public CellCoord CenterCoord => _coords[_coords.Count / 2];
        #endregion 公開資訊

        #region 建構式
        public MatchLine(GemType color, MatchDirection direction, List<CellCoord> coords)
        {
            Color = color;
            Direction = direction;
            _coords = coords;
        }
        #endregion 建構式

        /// <summary>
        /// 檢查此連線是否包含指定座標
        /// </summary>
        /// <param name="coord">指定座標</param>
        /// <returns>是/否</returns>
        public bool Contain(CellCoord coord)
        {
            for (int i = 0; i < _coords.Count; i++)
            {//檢查是否包含該座標
                if (_coords[i].X == coord.X && _coords[i].Y == coord.Y) return true;
            }
            return false;
        }
    }
}