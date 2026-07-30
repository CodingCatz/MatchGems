using MatchGems.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace MatchGems.View
{
    /// <summary>
    /// 棋盤和寶石視覺整體
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        #region 基本參數
        [SerializeField] private int _cellSize = 64;
        [SerializeField] private float _pixelPerUnit = 64f;
        [SerializeField] private GemTile _tilePrefab;
        private GridMapper _gridMapper;

        private GemTile[,] _tiles;
        private GemTilePool _tilePool;
        #endregion 基本參數

        #region 公開屬性
        /// <summary>
        /// 單一格的像素尺寸
        /// </summary>
        public int CellSize => _cellSize;
        /// <summary>
        /// 一個Unity單位對應的像素值
        /// </summary>
        public float PixelPerUnit => _pixelPerUnit;
        /// <summary>
        /// 單一格在世界座標的單位比例
        /// </summary>
        public float CellWorldSize => _cellSize / _pixelPerUnit;
        #endregion 公開屬性

        #region 生命週期
        private void Start()
        {
            _tilePool = new GemTilePool(_tilePrefab, transform);
        }
        #endregion 生命週期

        #region 公開方法
        /// <summary>
        /// 依棋盤資料建立全部Tile視覺
        /// </summary>
        /// <param name="board">棋盤資料</param>
        public void Build(BoardModel board, GridMapper gridMapper)
        {
            _gridMapper = gridMapper;
            ReleaseGemTiles();//初建安全清除
            //產生與資料相同的視覺尺寸
            _tiles = new GemTile[board.Width, board.Height];

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    //從棋盤取回資料
                    GemData gemData = board.GetGem(x, y);
                    //建立視覺物件
                    _tiles[x, y] = CreateTile(x, y);
                    //設定對應視覺(顏色)
                    _tiles[x, y].SetGem(gemData);
                }
            }
        }
        /// <summary>
        /// 活珠掉落移動
        /// </summary>
        /// <param name="board"></param>
        /// <param name="falls"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public async Task AnimateFallAsync(BoardModel board, List<TileMove> falls, float duration)
        {
            GemTile[,] next = new GemTile[board.Width, board.Height];
            List<Task> tasks = new List<Task>();
            foreach (TileMove fall in falls)
            {//舊珠的移動
                GemTile tile = _tiles[fall.From.X, fall.From.Y];
                next[fall.To.X, fall.To.Y] = tile;
                tasks.Add(tile.MoveToAsync(_gridMapper.ToWorld(fall.To), duration));
            }
            _tiles = next;
            await Task.WhenAll(tasks);
        }
        /// <summary>
        /// 新珠掉落移動
        /// </summary>
        /// <param name="board"></param>
        /// <param name="fills"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public async Task AnimateFillAsync(BoardModel board, List<TileMove> fills, float duration)
        {
            List<Task> tasks = new List<Task>();
            foreach (TileMove fill in fills)
            {//新珠的掉落
                Vector3 target = _gridMapper.ToWorld(fill.To);//落點
                GemTile tile = _tilePool.Get(SpawnAbove(board, fill.From), board.GetGem(fill.From));//算出頂點和置入新資料
                _tiles[fill.To.X, fill.To.Y] = tile;//紀錄最終位子
                tasks.Add(tile.MoveToAsync(target, duration));
            }
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 重畫指定格的寶石外觀
        /// </summary>
        /// <param name="board"></param>
        /// <param name="coord"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void RefreshGem(BoardModel board, CellCoord coord)
        {
            GemTile tile = GetTile(coord);//定位要重繪的Tile
            if (tile == null || !board.HasGem(coord)) return;
            tile.SetGem(board.GetGem(coord));
        }

        public void GemTileAsync(CellCoord from, CellCoord to)
        {
            GemTile tmp = _tiles[to.X, to.Y];
            _tiles[to.X, to.Y] = _tiles[from.X, from.Y];
            _tiles[from.X, from.Y] = tmp;
        }
        /// <summary>
        /// [等候型任務]寶石交換動畫
        /// </summary>
        /// <param name="A">A石</param>
        /// <param name="B">B石</param>
        /// <param name="duration">運作時長</param>
        /// <returns>任務狀態</returns>
        public async Task AnimateSwapAsync(CellCoord A, CellCoord B, float duration)
        {
            GemTile tileA = GetTile(A);
            GemTile tileB = GetTile(B);
            if (tileA == null || tileB == null) return;//任務結束
            //各別建立交換任務
            Task moveA = tileA.MoveToAsync(_gridMapper.ToWorld(B), duration);
            Task moveB = tileB.MoveToAsync(_gridMapper.ToWorld(A), duration);
            //等待任務都完成
            await Task.WhenAll(moveA, moveB);
            GemTileAsync(A, B);//實體視覺資料位置同步
        }
        /// <summary>
        /// [等候型任務]寶石清除動畫
        /// </summary>
        /// <param name="list">配對寶石清單</param>
        /// <param name="duration">運作時長</param>
        /// <returns>任務狀態</returns>
        public async Task AnimateClearAsync(IReadOnlyList<CellCoord> list, float duration)
        {//準備對應的任務清單：執行消除任務 * N
            List<Task> pops = new List<Task>();
            for (int i = 0; i < list.Count; i++)
            {
                GemTile tile = GetTile(list[i]);
                if (tile == null) continue;//防呆：避免回收空物件
                ReleaseeGemTile(list[i]);//物件池回收
                pops.Add(tile.PopAsync(duration));//加入任務待辦
            }
            //等待整組任務都完成
            await Task.WhenAll(pops);
        }
        #endregion 公開方法

        #region 私有方法
        /// <summary>
        /// 回物件池的 GemTile 消除紀錄
        /// </summary>
        /// <param name="coord"></param>
        private void ReleaseeGemTile(CellCoord coord)
        {
            _tilePool.Release(_tiles[coord.X, coord.Y]);//收回物件池
            _tiles[coord.X, coord.Y] = null;//消除紀錄
        }

        private void ReleaseGemTiles()
        {
            if (_tilePool == null || _tiles == null) return;

            for (int x = 0; x < _tiles.GetLength(0); x++)
            {
                for (int y = 0; y < _tiles.GetLength(1); y++)
                {
                    _tilePool.Release(_tiles[x, y]);
                }
            }
        }
        /// <summary>
        /// 依照定位實例化寶石磚
        /// </summary>
        /// <param name="x">座標X</param>
        /// <param name="y">座標Y</param>
        /// <returns>寶石磚</returns>
        private GemTile CreateTile(int x, int y)
        {
            //Vector3 position = new Vector3(x * CellWorldSize, y * CellWorldSize, 0);
            return CreateTileAt(_gridMapper.ToWorld(new CellCoord(x, y)));
        }

        /// <summary>
        /// 建立寶石磚在世界座標
        /// </summary>
        /// <param name="pos">指定世界座標</param>
        /// <returns>寶石磚</returns>
        private GemTile CreateTileAt(Vector3 pos)
        {
            return Instantiate(_tilePrefab, pos, Quaternion.identity, transform);
        }

        /// <summary>
        /// 取得指定位置的寶石元件
        /// </summary>
        /// <param name="coord">定位</param>
        /// <returns>寶石元件</returns>
        private GemTile GetTile(CellCoord coord)
        {
            if (_tiles == null || coord.X < 0 || coord.Y < 0 || coord.X >= _tiles.GetLength(0) || coord.Y >= _tiles.GetLength(1)) return null;
            //檢查視覺圖陣列是否存在，以及訪問座標在陣列內
            return _tiles[coord.X, coord.Y];
        }
        /// <summary>
        /// 補珠的起點：棋盤的頂端+1單位
        /// </summary>
        /// <param name="board">棋盤資料</param>
        /// <param name="coord">定位</param>
        /// <returns>新座標</returns>
        private Vector3 SpawnAbove(BoardModel board, CellCoord coord)
        {
            Vector3 top = _gridMapper.ToWorld(new CellCoord(coord.X, board.Height));
            return top;
        }
        #endregion 私有方法
    }
}

