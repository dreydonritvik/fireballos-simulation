using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace FireballOS
{
    [RequireComponent(typeof(RawImage))]
    public class PaintCanvas : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        [Header("Canvas Resolution")]
        [SerializeField] private int textureWidth = 512;
        [SerializeField] private int textureHeight = 384;

        [Header("Brush Settings")]
        [SerializeField] private Color brushColor = Color.black;
        [SerializeField] private int brushSize = 3;
        [SerializeField] private Color backgroundColor = Color.white;

        private RawImage rawImage;
        private Texture2D drawableTexture;
        private RectTransform rectTransform;
        private Vector2? lastPixelPos = null;

        private void Awake()
        {
            rawImage = GetComponent<RawImage>();
            rectTransform = GetComponent<RectTransform>();

            InitTexture();
        }

        private void InitTexture()
        {
            drawableTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
            drawableTexture.filterMode = FilterMode.Point;

            ClearCanvas();
            rawImage.texture = drawableTexture;
        }

        public void ClearCanvas()
        {
            Color[] pixels = new Color[textureWidth * textureHeight];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = backgroundColor;
            }
            drawableTexture.SetPixels(pixels);
            drawableTexture.Apply();
            lastPixelPos = null;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            if (GetPixelCoord(eventData, out Vector2 pixelPos))
            {
                DrawCircle((int)pixelPos.x, (int)pixelPos.y, brushSize, brushColor);
                drawableTexture.Apply();
                lastPixelPos = pixelPos;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            if (GetPixelCoord(eventData, out Vector2 pixelPos))
            {
                if (lastPixelPos.HasValue)
                {
                    DrawLine((int)lastPixelPos.Value.x, (int)lastPixelPos.Value.y, (int)pixelPos.x, (int)pixelPos.y, brushColor, brushSize);
                }
                else
                {
                    DrawCircle((int)pixelPos.x, (int)pixelPos.y, brushSize, brushColor);
                }

                drawableTexture.Apply();
                lastPixelPos = pixelPos;
            }
        }

        private bool GetPixelCoord(PointerEventData eventData, out Vector2 pixelCoord)
        {
            pixelCoord = Vector2.zero;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                return false;
            }

            Rect rect = rectTransform.rect;
            float normalizedX = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
            float normalizedY = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);

            int px = Mathf.Clamp(Mathf.FloorToInt(normalizedX * textureWidth), 0, textureWidth - 1);
            int py = Mathf.Clamp(Mathf.FloorToInt(normalizedY * textureHeight), 0, textureHeight - 1);

            pixelCoord = new Vector2(px, py);
            return true;
        }

        private void DrawLine(int x0, int y0, int x1, int y1, Color col, int radius)
        {
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                DrawCircle(x0, y0, radius, col);

                if (x0 == x1 && y0 == y1) break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        private void DrawCircle(int cx, int cy, int r, Color col)
        {
            int minX = Mathf.Max(0, cx - r);
            int maxX = Mathf.Min(textureWidth - 1, cx + r);
            int minY = Mathf.Max(0, cy - r);
            int maxY = Mathf.Min(textureHeight - 1, cy + r);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r)
                    {
                        drawableTexture.SetPixel(x, y, col);
                    }
                }
            }
        }

        public void SetBrushColor(Color newColor) => brushColor = newColor;
        public void SetBrushSize(int newSize) => brushSize = Mathf.Clamp(newSize, 1, 20);
    }
}