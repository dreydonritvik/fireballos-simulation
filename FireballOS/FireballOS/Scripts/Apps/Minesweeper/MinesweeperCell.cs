using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FireballOS
{
    public class MinesweeperCell : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI References")]
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI cellText;

        [Header("Colors")]
        [SerializeField] private Color unrevealedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        [SerializeField] private Color revealedColor = new Color(0.88f, 0.88f, 0.88f, 1f);
        [SerializeField] private Color explodedColor = new Color(0.9f, 0.2f, 0.2f, 1f);

        public int X { get; private set; }
        public int Y { get; private set; }
        public bool IsMine { get; set; }
        public bool IsRevealed { get; private set; }
        public bool IsFlagged { get; private set; }
        public int AdjacentMines { get; set; }

        public Image Background => background;
        public TextMeshProUGUI CellText => cellText;

        private MinesweeperGame game;

        private void Awake()
        {
            EnsureReferences();
        }

        public void EnsureReferences()
        {
            if (background == null)
            {
                background = GetComponent<Image>();
            }

            if (cellText == null)
            {
                cellText = GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        public void Init(MinesweeperGame gameInstance, int x, int y)
        {
            game = gameInstance;
            X = x;
            Y = y;
            ResetCell();
        }

        public void ResetCell()
        {
            EnsureReferences();

            IsMine = false;
            IsRevealed = false;
            IsFlagged = false;
            AdjacentMines = 0;

            if (background != null)
            {
                background.color = unrevealedColor;
            }

            if (cellText != null)
            {
                cellText.text = string.Empty;
                cellText.color = Color.black;
            }
        }

        private string GetFlagSymbol()
        {
            if (cellText != null && cellText.font != null && cellText.font.HasCharacter(0x1F6A9))
                return "🚩";
            return "⚑";
        }

        private string GetMineSymbol()
        {
            if (cellText != null && cellText.font != null && cellText.font.HasCharacter(0x1F4A3))
                return "💣";
            return "✹";
        }

        private string GetWrongFlagSymbol()
        {
            if (cellText != null && cellText.font != null && cellText.font.HasCharacter(0x274C))
                return "❌";
            return "✖";
        }

        public void SetFlagged(bool flagged)
        {
            if (IsRevealed) return;

            IsFlagged = flagged;
            EnsureReferences();

            if (cellText != null)
            {
                cellText.text = IsFlagged ? GetFlagSymbol() : string.Empty;
                cellText.color = Color.red;
            }
        }

        public void Reveal(bool isExplodedMine = false)
        {
            IsRevealed = true;
            EnsureReferences();

            if (IsMine)
            {
                if (background != null)
                {
                    background.color = isExplodedMine ? explodedColor : revealedColor;
                }

                if (cellText != null)
                {
                    cellText.text = GetMineSymbol();
                    cellText.color = Color.black;
                }
            }
            else
            {
                if (background != null)
                {
                    background.color = revealedColor;
                }

                if (cellText != null)
                {
                    if (AdjacentMines > 0)
                    {
                        cellText.text = AdjacentMines.ToString();
                        cellText.color = GetNumberColor(AdjacentMines);
                    }
                    else
                    {
                        cellText.text = string.Empty;
                    }
                }
            }
        }

        public void ShowWrongFlag()
        {
            EnsureReferences();
            if (cellText != null)
            {
                cellText.text = GetWrongFlagSymbol();
                cellText.color = Color.red;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (game == null) return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                game.OnCellLeftClick(this);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                game.OnCellRightClick(this);
            }
        }

        private Color GetNumberColor(int number)
        {
            switch (number)
            {
                case 1: return new Color(0.0f, 0.0f, 0.9f);       // 1 = Mavi
                case 2: return new Color(0.0f, 0.5f, 0.0f);       // 2 = Yeşil
                case 3: return new Color(0.85f, 0.0f, 0.0f);      // 3 = Kırmızı
                case 4: return new Color(0.0f, 0.0f, 0.5f);       // 4 = Koyu Mavi
                case 5: return new Color(0.5f, 0.0f, 0.0f);       // 5 = Bordo / Koyu Kırmızı
                case 6: return new Color(0.0f, 0.5f, 0.5f);       // 6 = Turkuaz / Cyan
                case 7: return Color.black;                       // 7 = Siyah
                case 8: return new Color(0.45f, 0.45f, 0.45f);   // 8 = Gri
                default: return Color.black;
            }
        }
    }
}
