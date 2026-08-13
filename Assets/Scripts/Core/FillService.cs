using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace MatchGems.Core
{

    /// <summary>
    /// 寶石填充服務
    /// </summary>
    public class FillService
    {
        /// <summary>
        /// 預建立好的Index轉Type清單
        /// </summary>
        public static IReadOnlyList<GemType> GemTypes { get; } = 
            new GemType[] 
            { 
                GemType.Red, 
                GemType.Blue,
                GemType.Green,
                GemType.Yellow,
                GemType.Purple,
                GemType.Pink
            };

        #region 系統屬性參數
        /// <summary>
        /// 重力移動的暫存清單
        /// </summary>
        private readonly List<TileMove> moves = new List<TileMove>();
        /// <summary>
        /// 定位用的座標暫存
        /// </summary>
        private CellCoord coord;
        private readonly List<int> _ruleoutType = new List<int>();
        #endregion 系統屬性參數

        #region 公開方法
        /// <summary>
        /// 棋盤建立初始填滿(開局避免3連)
        /// </summary>
        /// <param name="board"></param>
        public void FillInitial(BoardModel board)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    CellCoord target = new CellCoord(x, y);
                    //空位補珠
                    board.SetGem(target, PickRandomGem(board, target));
                }
            }
        }

        private GemType PickRandomGem(BoardModel board, CellCoord target)
        {
            //左和下連續的兩顆是否同色
            int checkH = HasSameColor(board, 
                new CellCoord(target.X - 1, target.Y), 
                new CellCoord(target.X - 2, target.Y));
            int checkV = HasSameColor(board,
                new CellCoord(target.X, target.Y - 1),
                new CellCoord(target.X, target.Y - 2));
            //重置排除Color清單(Index)
            _ruleoutType.Clear();
            //第一個值：無條件加入
            _ruleoutType.Add(checkH);
            if (!_ruleoutType.Contains(checkV)) _ruleoutType.Add(checkV);
            //預設在Color的Index外
            int selectIndex = GemTypes.Count;
            while(selectIndex == GemTypes.Count)
            {//selectIndex有變化就離開迴圈
                int randomIndex = Random.Range(0, GemTypes.Count);
                if (!_ruleoutType.Contains(randomIndex)) 
                {
                    selectIndex = randomIndex;
                    break;
                }
            }

            return GemTypes[selectIndex];
        }
        /// <summary>
        /// 兩個位置的寶石同色檢查
        /// </summary>
        /// <param name="board"></param>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns>檢查碼</returns>
        private int HasSameColor(BoardModel board, CellCoord A, CellCoord B)
        {
            if (!board.HasGem(A) || !board.HasGem(B))
            {//任意格位置無石：出界
                return -1;//哨兵值
            }
            GemType aType = board.GetGemColor(A);
            if (aType != board.GetGemColor(B))
            {//連續的兩顆顏色不同
                return -1;//哨兵值
            }
            return (int)aType;
        }

        /// <summary>
        /// PlaneA：將棋盤補滿寶石
        /// </summary>
        /// <param name="board">棋盤資料</param>
        /*public void Fill(BoardModel board)
        {
            for (int y = 0; y < board.Height; y++) 
            {
                for (int x = 0; x < board.Width; x++)
                {
                    if (board.HasGem(x, y)) continue;
                    //空位補珠
                    board.SetGem(x, y, CreateRandomGem());
                }
            }
        }*/

        /// <summary>
        /// PlaneB：將棋盤補滿寶石
        /// </summary>
        /// <param name="board">棋盤資料</param>
        /// <returns>移動紀錄清單</returns>
        public List<TileMove> Fill(BoardModel board)
        {
            moves.Clear();

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    coord.Set(x, y);
                    if (board.HasGem(coord)) continue;
                    
                    //空位補珠
                    board.SetGem(coord, CreateRandomGem());
                    moves.Add(new TileMove(coord));
                }
            }
            return moves;
        }

        /// <summary>
        /// 建立隨機的寶石類型
        /// </summary>
        /// <returns>隨機的寶石類型</returns>
        public GemType CreateRandomGem()
        {
            //利用C#系統原生Enun取得列舉長度
            int gemCount = Enum.GetValues(typeof(GemType)).Length;
            return (GemType)Random.Range(0, gemCount);
        }
        #endregion 公開方法
    }
}