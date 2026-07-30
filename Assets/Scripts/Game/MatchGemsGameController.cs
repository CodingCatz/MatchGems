using MatchGems.Core;
using MatchGems.View;
using MatchGems.Inputs;
using UnityEngine;
using System.Collections.Generic;

namespace MatchGems.Game
{
    /// <summary>
    /// 遊戲流程主程式(控制)
    /// </summary>
    public class MatchGemsGameController : MonoBehaviour
    {
        #region 基本參數
        [SerializeField] private BoardView _boardView;
        [SerializeField] private BoardInput _boardInput;
        [SerializeField] private int _width = 8;
        [SerializeField] private int _height = 8;
        /// <summary>
        /// 交換珠的移動時間
        /// </summary>
        [SerializeField] private float _swapAnimationDuration = 0.3f;
        /// <summary>
        /// 清除珠的時間
        /// </summary>
        [SerializeField] private float _clearAnimationDuration = 0.2f;
        /// <summary>
        /// 落下移動時間
        /// </summary>
        [SerializeField] private float _buildAnimationDuration = 0.3f;
        private readonly List<CellCoord> _moveCells = new List<CellCoord>();
        private BoardModel _boardModel;
        private GridMapper _gridMapper;
        /// <summary>
        /// 預建立的流程控制器
        /// </summary>
        private readonly BoardFlowController _boardFlowController = new BoardFlowController();
        /// <summary>
        /// 主流程是否正在忙碌：
        /// 非處於待機狀態(運算中)
        /// </summary>
        private bool _isBusy => _boardFlowController.State != BoardState.Idle;
        #endregion 基本參數

        #region 生命週期
        void Start()
        {
            CreateBoard();
            CreateMapper();
            BuildView();
            ConfigureInput();
        }
        #endregion 生命週期

        #region 私有方法
        /// <summary>
        /// 建立棋盤
        /// </summary>
        private void CreateBoard()
        {
            _boardModel = new BoardModel(_width, _height);
            //流程控制：補珠
            _boardFlowController.Fill(_boardModel);
        }
        /// <summary>
        /// 建立轉換器
        /// </summary>
        private void CreateMapper()
        {
            //建構Root物件座標即為原點
            _gridMapper = new GridMapper(_boardView.transform.position, _boardView.CellWorldSize);
        }
        /// <summary>
        /// 以資料驅動視覺
        /// </summary>
        private void BuildView()
        {
            _boardView.Build(_boardModel, _gridMapper);
        }
        /// <summary>
        /// 設置輸入操作
        /// </summary>
        private void ConfigureInput()
        {
            _boardInput.Configure(_gridMapper);//CellSize先走預設
            _boardInput.SwapAction = TrySwap;
        }

        /// <summary>
        /// 嘗試交換兩格的寶石資料
        /// </summary>
        /// <param name="from">起始</param>
        /// <param name="to">目標</param>
        private async void TrySwap(CellCoord from, CellCoord to)
        {
            //嘗試執行交換資料(純資料)
            if (!_boardFlowController.TrySwap(_boardModel, from, to)) return;
            //嘗試執行交換動畫(純視覺)
            await _boardView.AnimateSwapAsync(from, to, _swapAnimationDuration);
            //動畫任務結束：檢查是否為無效移動(沒配對)
            MatchResult result = _boardFlowController.FindMatches(_boardModel);

            if (!result.HasMatch)
            {//沒配到：資料換回，動畫回彈
                _boardModel.SwapGems(from, to);
                await _boardView.AnimateSwapAsync(from, to, _swapAnimationDuration);
                _boardFlowController.SetIdle();//回到待機
                return;//任務中斷
            }

            //資訊重置
            int comboCount = 0;
            _moveCells.Clear();
            _moveCells.Add(from);
            _moveCells.Add(to);

            //有配對：進入循環(進到忙碌計算)
            while (result.HasMatch)
            {
                SpecialGemSpawnInfo spawnInfo = _boardFlowController.CreateSpawn(result, _moveCells);

                //清除資料(整體)
                _boardFlowController.ClearStep(_boardModel, result, spawnInfo);
                //消除動態表演(逐線處理)
                foreach (MatchLine line in result.Line)
                {
                    comboCount++;//正確計算連鎖的位子
                    Debug.Log($"連鎖：{comboCount} !!");
                    await _boardView.AnimateClearAsync(line.Coords, _clearAnimationDuration);
                    //視需求決定是否停置時間
                }
                //特殊寶石產生判斷
                if (spawnInfo.HasSpecialGem)
                {
                    _boardView.RefreshGem(_boardModel, spawnInfo.SpawnCoord);
                }


                //套用重力：落下資料
                List<TileMove> falls = _boardFlowController.ApplyGravity(_boardModel);
                await _boardView.AnimateFallAsync(_boardModel, falls, _buildAnimationDuration);

                //套用天降：填充資料
                List<TileMove> fills = _boardFlowController.ApplyFill(_boardModel);
                await _boardView.AnimateFillAsync(_boardModel, fills, _buildAnimationDuration);

                //再次檢查有無配對
                result = _boardFlowController.FindMatches(_boardModel);
            }
            //無任何天降配對後
            _boardFlowController.SetIdle();//回到待機
        }
        #endregion 私有方法

        #region 生命週期
        private void Update()
        {
            if (_isBusy) return;
            //遊戲正在執行邏輯運算，阻擋任何即時性操作
        }
        #endregion 生命週期
    }
}