using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MatchGems.Core
{
    /// <summary>
    /// 單次的配對結果(多線整理)
    /// </summary>
    public class MatchResult
    {
        #region 唯讀紀錄
        /// <summary>
        /// 連線清單
        /// </summary>
        private readonly List<MatchLine> _lines = new List<MatchLine>();
        #endregion 唯讀紀錄

        #region 公開資訊
        /// <summary>
        /// 連線數
        /// </summary>
        public int LineCount => _lines.Count;
        /// <summary>
        /// 是否有產生任何配對
        /// </summary>
        public bool HasMatch => LineCount > 0;
        /// <summary>
        /// 對外公開的分組清單唯讀接口
        /// </summary>
        public IReadOnlyList<MatchLine> Line => _lines;
        #endregion 公開資訊

        #region 公開方法
        /// <summary>
        /// 加入配對連線
        /// </summary>
        public void AddLine(MatchLine line)
        {
            _lines.Add(line);
        }

        /// <summary>
        /// 取得完全不重複的配對座標格清單
        /// </summary>
        /// <returns>不重複的配對座標格清單</returns>
        public List<CellCoord> GetUniqueCoords()
        {
            List<CellCoord> coords = new List<CellCoord>();

            for (int i = 0; i < _lines.Count; i++)
            {//抽線
                IReadOnlyList<CellCoord> lineCroods = _lines[i].Coords;
                for (int j = 0; j < lineCroods.Count; j++)
                {//抽格
                    AddUnique(coords, lineCroods[j]);
                }
            }

            return coords;
        }
        #endregion 公開方法

        #region 私有方法
        /// <summary>
        /// 剔除重複的格子加入消除清單組
        /// </summary>
        /// <param name="coords">母體清單</param>
        /// <param name="coord">被併入的單體</param>
        private void AddUnique(List<CellCoord> coords, CellCoord coord)
        {
            for (int i = 0; i < coords.Count; i++)
            {
                if (coords[i].X == coord.X && coords[i].Y == coord.Y)
                    return;
            }
            coords.Add(coord);
        }
        #endregion 私有方法
    }
}
