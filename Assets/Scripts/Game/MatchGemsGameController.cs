using MatchGems.Core;
using MatchGems.View;
using MatchGems.Inputs;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            _boardFlowController.FillInitial(_boardModel);
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
                SpecialGemSpawnPlan spawnPlan = _boardFlowController.CreateSpawnPlan(result, _moveCells);

                //清除資料(依組別排除特殊石的資料)
                ClearStepResult clearStepResult = _boardFlowController.ClearStep(_boardModel, result, spawnPlan, out DetonationChain chain);

                comboCount++;//計算連鎖數
                await _boardView.AnimateClearAsync(clearStepResult.ClearedCoords, _clearAnimationDuration);
                
                //特殊寶石產生判斷
                for (int i = 0; i < spawnPlan.Count; i++)
                {
                    _boardView.RefreshGem(_boardModel, spawnPlan[i].SpawnCoord);
                }

                //特殊石引爆：獨立多層連鎖運算
                await RunDetonactionAsync(chain);


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

        /// <summary>
        /// 特殊能力石清板運算
        /// </summary>
        /// <param name="chain"></param>
        /// <returns></returns>
        private async Task RunDetonactionAsync(DetonationChain chain)
        {
            while (chain.HasFuses)
            {
                ClearStepResult result = _boardFlowController.DetonactionStep(chain);

                await _boardView.AnimateClearAsync(result.ClearedCoords, _clearAnimationDuration);
            }
        }
        #endregion 私有方法

        #region 生命週期
        private void Update()
        {
            if (_isBusy) return;
            //遊戲正在執行邏輯運算，阻擋任何即時性操作
        }
        #endregion 生命週期

        [ContextMenu("強制更新所有寶石")]
        public void RefreshAllGems()
        {
            for (int y = 0; y < _boardModel.Height; y++)
                for (int x = 0; x < _boardModel.Width; x++)
                {
                    CellCoord coord = new CellCoord(x, y);
                    _boardView.RefreshGem(_boardModel, coord);
                }
        }

        [ContextMenu("強制更新所有寶石變普通")]
        public void RefreshAllGemsToNormal()
        {
            for (int y = 0; y < _boardModel.Height; y++)
                for (int x = 0; x < _boardModel.Width; x++)
                {
                    CellCoord coord = new CellCoord(x, y);
                    _boardModel.GetGem(coord).SetPower();
                    _boardView.RefreshGem(_boardModel, coord);
                }
        }

        [ContextMenu("強制更新面板")]
        public void RefreshBoard()
        {
            _boardFlowController.FillInitial(_boardModel);
            for (int y = 0; y < _boardModel.Height; y++)
                for (int x = 0; x < _boardModel.Width; x++)
                {
                    CellCoord coord = new CellCoord(x, y);
                    _boardModel.GetGem(coord).SetPower();
                    _boardView.RefreshGem(_boardModel, coord);
                }
        }

        [ContextMenu("測試盤面/一步雙四連")]
        private void ArrangeDoubleFourMatchBoard()
        {
            ArrangeDoubleLineMatchBoard(4);
        }
        [ContextMenu("測試盤面/一步雙五連")]
        private void ArrangeDoubleFiveMatchBoard()
        {
            ArrangeDoubleLineMatchBoard(5);
        }

        private void ArrangeDoubleLineMatchBoard(int lineLength)
        {
            if (_boardModel == null || _boardView == null)
            {
                Debug.LogWarning("請先進入 Play Mode，等棋盤建立後再使用測試盤面快捷。");
                return;
            }
            if (_isBusy)
            {
                Debug.LogWarning("棋盤流程仍在運作，請等 State 回到 Idle 再排測試盤面。");
                return;
            }
            if (_boardModel.Width < lineLength || _boardModel.Height < 2)
            {
                Debug.LogWarning($"一步雙{lineLength}連至少需要 {lineLength} × 2 的棋盤。");
                return;
            }
            FillTestPattern();
            int firstRow = Mathf.Max(0, _boardModel.Height / 2 - 1);
            int secondRow = firstRow + 1;
            int swapColumn = 2;
            for (int x = 0; x < lineLength; x++)
            {
                _boardModel.SetGem(x, firstRow, GemType.Red);
                _boardModel.SetGem(x, secondRow, GemType.Blue);
            }
            _boardModel.SetGem(swapColumn, firstRow, GemType.Blue);
            _boardModel.SetGem(swapColumn, secondRow, GemType.Red);
            if (lineLength < _boardModel.Width)
            {
                _boardModel.SetGem(lineLength, firstRow, GemType.Purple);
                _boardModel.SetGem(lineLength, secondRow, GemType.Yellow);
            }
            CellCoord from = new CellCoord(swapColumn, firstRow);
            CellCoord to = new CellCoord(swapColumn, secondRow);
            bool presetIsValid = ValidateDoubleLinePreset(from, to, lineLength);
            RefreshAllGems();
            string result = presetIsValid ? "通過" : "失敗，請檢查盤面生成規則";
            Debug.Log(
                $"一步雙{lineLength}連盤面已建立。交換 {from.pos} 與 {to.pos}；資料預驗證：{result}。");
        }

        private void FillTestPattern()
        {
            for (int y = 0; y < _boardModel.Height; y++)
            {
                for (int x = 0; x < _boardModel.Width; x++)
                {
                    int typeIndex =
                        (x + y * 2) % FillService.GemTypes.Count;
                    _boardModel.SetGem(
                        x,
                        y,
                        FillService.GemTypes[typeIndex]);
                }
            }
        }
        private bool ValidateDoubleLinePreset(
            CellCoord from,
            CellCoord to,
            int expectedLength)
        {
            MatchResult beforeSwap = _boardFlowController.FindMatches(_boardModel);
            if (beforeSwap.HasMatch)
            {
                return false;
            }
            _boardModel.SwapGems(from, to);
            MatchResult afterSwap = _boardFlowController.FindMatches(_boardModel);
            _boardModel.SwapGems(from, to);
            if (afterSwap.LineCount != 2)
            {
                return false;
            }
            for (int i = 0; i < afterSwap.LineCount; i++)
            {
                if (afterSwap.Line[i].Length != expectedLength)
                {
                    return false;
                }
            }
            return true;
        }
    }
}