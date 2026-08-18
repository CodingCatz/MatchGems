using System;
using System.Collections.Generic;
using UnityEngine;

namespace MatchGems.Core
{
    public class BoardFlowController
    {
        #region 基本組件
        /// <summary>
        /// 配對檢查器
        /// </summary>
        private readonly MatchFinder _matchFinder = new MatchFinder();
        /// <summary>
        /// 落下解析器
        /// </summary>
        private readonly GravityResolver _gravityResolver = new GravityResolver();
        /// <summary>
        /// 寶石填充服務
        /// </summary>
        private readonly FillService _fillService = new FillService();
        /// <summary>
        /// 特殊石能力催化系統
        /// </summary>
        private readonly SpecialGemActivator _specialGemActivator = new SpecialGemActivator();
        #endregion 基本組件

        #region 公開參數
        /// <summary>
        /// 當前遊戲所處的狀態
        /// </summary>
        public BoardState State { get; private set; } = BoardState.Idle;
        #endregion 公開參數

        #region 公開方法
        /// <summary>
        /// 回到待命狀態
        /// </summary>
        public void SetIdle()
        {
            State = BoardState.Idle;

        }
        /// <summary>
        /// 嘗試執行玩家的資料交換操作
        /// </summary>
        /// <returns>是否成功</returns>
        public bool TrySwap(BoardModel board, CellCoord from, CellCoord to)
        {
            if (State != BoardState.Idle || !board.IsInside(to) || !board.IsAdjacent(from, to)) return false;
            //轉換狀態
            State = BoardState.Swapping;
            board.SwapGems(from, to);
            return true;
        }

        /// <summary>
        /// 搜索棋盤上全部的配對線結果
        /// </summary>
        /// <returns>配對線結果</returns>
        public MatchResult FindMatches(BoardModel board)
        {
            return _matchFinder.FindMatches(board);
        }
        /// <summary>
        /// 一組一拍式清除流程
        /// </summary>
        /// <param name="board"></param>
        /// <param name="result"></param>
        public ClearStepResult ClearStep(BoardModel board, MatchResult result, SpecialGemSpawnPlan spawnPlan, out DetonationChain chain)
        {
            State = BoardState.Clearing;
            List<CellCoord> coords = result.GetUniqueCoords();
            RemoveSpawnCoord(coords, spawnPlan);

            //連鎖演算觸發位子
            chain = _specialGemActivator.BeginChain(board, coords, spawnPlan);

            //設置特殊石
            ApplySpecialSpawn(board, spawnPlan);
            //清除資料
            board.ClearGems(coords);

            return new ClearStepResult(coords, ClearGemTypes(board, coords));
        }

        /// <summary>
        /// 引爆：炸開這層的特殊石連鎖結果
        /// </summary>
        /// <param name="chain"></param>
        /// <returns></returns>
        public ClearStepResult DetonactionStep(DetonationChain chain)
        {
            State = BoardState.Clearing;
            List<CellCoord> coords = _specialGemActivator.ExpandNextLayer(chain);          
            //Debug.Log($"炸板數量：{coords.Count}!!!");
            chain.Board.ClearGems(coords);
            return new ClearStepResult(coords, ClearGemTypes(chain.Board, coords));
        }

        /// <summary>
        /// 清除寶石的顏色
        /// </summary>
        /// <param name="board"></param>
        /// <param name="coords"></param>
        /// <returns></returns>
        private List<GemType> ClearGemTypes(BoardModel board, List<CellCoord> coords)
        {
            List <GemType> list = new List<GemType>();
            for (int i = 0; i < coords.Count; i++)
            {
                CellCoord coord = coords[i];
                if (board.HasGem(coord))
                {
                    list.Add(board.GetGemColor(coord));
                }
            }
            return list;
        }
        /// <summary>
        /// 從清單排除特殊石
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="spawnInfo"></param>
        private void RemoveSpawnCoord(List<CellCoord> coords, SpecialGemSpawnPlan spawnPlan)
        {
            for (int i = 0; i < coords.Count; i++)
            {
                if (spawnPlan.Contains(coords[i]))
                {
                    coords.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 確認特殊石的生成
        /// </summary>
        /// <param name="board"></param>
        /// <param name="spawnInfo"></param>
        private void ApplySpecialSpawn(BoardModel board, SpecialGemSpawnPlan spawnPlan)
        {
            for (int i = 0; i < spawnPlan.Count; i++)
            {
                board.SetGem(spawnPlan[i].SpawnCoord, spawnPlan[i].GemData);
            }
        }

        /// <summary>
        /// 初始化寶石
        /// </summary>
        /// <param name="board"></param>
        public void FillInitial(BoardModel board)
        {
            _fillService.FillInitial(board);
        }
        /// <summary>
        /// 補充寶石
        /// </summary>
        /// <param name="board"></param>
        public void Fill(BoardModel board)
        {
            _fillService.Fill(board);
        }

        /// <summary>
        /// 套用重力：落下
        /// </summary>
        /// <param name="board"></param>
        public List<TileMove> ApplyGravity(BoardModel board)
        {
            //移動資料
            State = BoardState.Falling;
            return _gravityResolver.Resolve(board);
        }
        /// <summary>
        /// 套用填補：天降
        /// </summary>
        /// <param name="board"></param>
        public List<TileMove> ApplyFill(BoardModel board)
        {
            State = BoardState.Filling;
            return _fillService.Fill(board);
        }
        /// <summary>
        /// 對外的公開接口
        /// </summary>
        /// <param name="result"></param>
        /// <param name="moveCells"></param>
        /// <returns></returns>
        public SpecialGemSpawnPlan CreateSpawnPlan(MatchResult result, IReadOnlyList<CellCoord> moveCells)
        {
            List<List<MatchLine>> groups = GroupLines(result.Line);
            SpecialGemSpawnPlan plan = new SpecialGemSpawnPlan();

            for (int i = 0; i < groups.Count; i++)
            {
                plan.Add(CreateSpawnForGroup(groups[i], moveCells));
            }

            return plan;
        }


        #endregion 公開方法

        #region 私有方法
        /// <summary>
        /// 線群組檢查比對
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        private List<List<MatchLine>> GroupLines(IReadOnlyList<MatchLine> lines)
        {
            List<List<MatchLine>> groups = new List<List<MatchLine>>();
            bool[] grouped = new bool[lines.Count];

            for(int i = 0; i < lines.Count; i++)
            {
                if (grouped[i]) continue;
                List<MatchLine> group = new List<MatchLine> { lines[i] };

                for (int current = 0; current < group.Count; current++)
                {
                    for (int l = 0; l < lines.Count; l++)
                    {
                        if (grouped[l] || !LineShareCoord(group[current], lines[l]))
                        {
                            continue;
                        }
                        grouped[l] = true;
                        group.Add(lines[l]);

                    }
                }
                groups.Add(group);
            }
            return groups;
        }

        /// <summary>
        /// 將所有移動的座標資料跟產生配對的Line比對，抓出KeyGem
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="moveCells"></param>
        private SpecialGemSpawnInfo CreateSpawnForGroup(List<MatchLine> lines, IReadOnlyList<CellCoord> moveCells)
        {
            //搜尋是否含特殊連線
            MatchLine matchLine = FindSpecialLine(lines, moveCells, out CellCoord bestCoord);
            //優先序：5連 > TL5連 > 4連
            if (matchLine != null && matchLine.Length >= 5)
            {//Debug.Log("5連");
                return GemFactory.CreateSpawnInfo(matchLine.Color, matchLine.Length, matchLine.Direction, true, matchLine.CenterCoord);
            }

            if (TryFindBombSpawn(lines, out SpecialGemSpawnInfo bombSpawn))
            {//Debug.Log("TL5連");
                return bombSpawn;
            }

            if (matchLine != null)
            {//Debug.Log("4連");
                return GemFactory.CreateSpawnInfo(matchLine.Color, matchLine.Length, matchLine.Direction, true, bestCoord);
            }

            //Debug.Log("一般");
            return SpecialGemSpawnInfo.None;
        }
        /// <summary>
        /// 兩線是否有重疊的格子
        /// </summary>
        /// <param name="lineA"></param>
        /// <param name="lineB"></param>
        /// <returns></returns>
        private bool LineShareCoord(MatchLine lineA, MatchLine lineB)
        {
            for (int i = 0; i < lineA.Coords.Count; i++)
            {
                if (lineB.Contain(lineA.Coords[i]))
                { 
                    return true; 
                }
            }
            return false;
        }

        /// <summary>
        /// 嘗試找到TL炸彈的組合
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="bombSpawn"></param>
        /// <returns></returns>
        private bool TryFindBombSpawn(IReadOnlyList<MatchLine> lines, out SpecialGemSpawnInfo bombSpawn)
        {
            bombSpawn = SpecialGemSpawnInfo.None;

            for (int a = 0; a < lines.Count; a++)
            {
                for (int b = a + 1; b < lines.Count; b++)
                {
                    MatchLine lineA = lines[a];
                    MatchLine lineB = lines[b];
                    //兩線同向或不同色無法構成TL型
                    if (lineA.Direction == lineB.Direction || lineA.Color != lineB.Color) continue;
                    //產生炸彈訂單
                    bombSpawn = new SpecialGemSpawnInfo(true, GemFactory.CreateBomb(lineA.Color), TryGetIntersection(lineA, lineB));
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 找特殊直線 4｜5 連
        /// </summary>
        private MatchLine FindSpecialLine(IReadOnlyList<MatchLine> lines, IReadOnlyList<CellCoord> moveCells, out CellCoord bestCoord)
        {
            MatchLine line = null;
            bestCoord = new CellCoord(0, 0);

            for (int i = 0; i < lines.Count; i++)
            {
                MatchLine matchLine = lines[i];
                if (matchLine.Length < 4) continue;

                bool gotkeyGem = TryGetKeyGemCoord(matchLine, moveCells, out CellCoord crood);
                if (!gotkeyGem) continue;

                line = matchLine;
                bestCoord = crood;
            }

            return line;
        }

        /// <summary>
        /// 嘗試從連線取得KeyGem的座標
        /// </summary>
        /// <param name="line"></param>
        /// <param name="moveCells"></param>
        /// <returns></returns>
        private bool TryGetKeyGemCoord(MatchLine line, IReadOnlyList<CellCoord> moveCells, out CellCoord coord)
        {
            if (moveCells != null)
            {
                for (int i = 0; i < moveCells.Count; i++)
                {//移動的座標清單有沒有在線內
                    if (line.Contain(moveCells[i]))
                    {
                        coord = moveCells[i];
                        return true;
                    }
                }
            }
            coord = line.CenterCoord;
            return false;//備案：直接給中間
        }

        /// <summary>
        /// 嘗試取得一條橫線和一條直線的交叉點
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        private CellCoord TryGetIntersection(MatchLine A, MatchLine B)
        {
            MatchLine lineH = A.Direction == MatchDirection.Horizontal ? A : B;
            MatchLine lineV = A.Direction == MatchDirection.Vertical ? A : B;

            return new CellCoord(lineV.CenterCoord.X, lineH.CenterCoord.Y); 
        }
        #endregion 私有方法
    }
}