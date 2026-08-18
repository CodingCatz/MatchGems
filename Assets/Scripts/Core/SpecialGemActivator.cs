using System.Collections.Generic;
using UnityEngine;

namespace MatchGems.Core
{
    /// <summary>
    /// 特殊石的能力催化劑(能力觸發)
    /// </summary>
    public class SpecialGemActivator
    {
        #region 公開方法
        /// <summary>
        /// 開始記錄連鎖，回報連鎖資料物件
        /// </summary>
        public DetonationChain BeginChain(BoardModel board, IReadOnlyList<CellCoord> coords, SpecialGemSpawnPlan spawnPlan)
        {
            // [多組特殊石修正] 把整份 Plan 交給 Chain 保護全部新生格。
            DetonationChain chain = new DetonationChain(board, spawnPlan);
            for (int i = 0; i < coords.Count; i++)
            {
                chain.TryRegister(coords[i]);
            }

            return chain;
        }
        /// <summary>
        /// 展開下一層能被特殊石引爆的格子清單
        /// </summary>
        public List<CellCoord> ExpandNextLayer(DetonationChain chain)
        {
            List<CellCoord> layer = new List<CellCoord>();
            IReadOnlyList<DetonationFuse> fuses = chain.TakeFuses();

            for (int i = 0; i < fuses.Count; i++)
            {
                AddEffectCells(chain, fuses[i], layer);
            }

            return layer;
        }
        #endregion 公開方法

        #region 私有方法
        /// <summary>
        /// 依照特殊寶石的能力影響範圍，加進要清除的格子
        /// </summary>
        private void AddEffectCells(DetonationChain chain, DetonationFuse fuse, List<CellCoord> layer)
        {
            switch (fuse.GemData.Power)
            {
                case GemPower.HLine:
                    AddRow(chain, fuse.Coord.Y, layer);
                    break;
                case GemPower.VLine:
                    AddCol(chain, fuse.Coord.X, layer);
                    break;
                case GemPower.Bomb:
                    AddSquare(chain, fuse.Coord, 1, layer);
                    break;
                case GemPower.Rainbow:
                    AddColor(chain, fuse.GemData.Color, layer);
                    break;
            }
        }
        /// <summary>
        /// 單一格加入同一層鏈的合法演算
        /// </summary>
        private void Add(DetonationChain chain, CellCoord coord, List<CellCoord> layer)
        {
            if (!chain.TryRegister(coord)) return;
            layer.Add(coord);
        }
        /// <summary>
        /// 加入一列
        /// </summary>
        private void AddRow(DetonationChain chain, int y, List<CellCoord> layer)
        {
            for (int x = 0; x < chain.Board.Width; x++)
            {
                Add(chain, new CellCoord(x, y), layer);
            }
        }
        /// <summary>
        /// 加入一欄
        /// </summary>
        private void AddCol(DetonationChain chain, int x, List<CellCoord> layer)
        {
            for (int y = 0; y < chain.Board.Height; y++)
            {
                Add(chain, new CellCoord(x, y), layer);
            }
        }
        /// <summary>
        /// 加入半徑周圍方型
        /// </summary>
        private void AddSquare(DetonationChain chain, CellCoord origin, int radius, List<CellCoord> layer)
        {
            for (int y = -radius; y <= radius; y++)
            {//從負值做到正值
                for (int x = -radius; x <= radius; x++)
                {//從負值做到正值
                    Add(chain, new CellCoord(origin.X + x, origin.Y + y), layer);
                }
            }
        }
        /// <summary>
        /// 加入所有同色
        /// </summary>
        private void AddColor(DetonationChain chain, GemType color, List<CellCoord> layer)
        { 
            BoardModel board = chain.Board;
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {//整盤搜索：同色
                    CellCoord coord = new CellCoord(x, y);
                    if (board.HasGem(coord) && board.GetGemColor(coord) == color)
                    {//顏色吻合：納入連鎖
                        Add(chain, coord, layer);
                    }
                }
            }
        }
        #endregion 私有方法
    }

}
