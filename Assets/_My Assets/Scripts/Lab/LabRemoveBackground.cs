using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LabRemoveBackground : MonoBehaviour
{
    public SpriteRenderer inputSpriteRenderer;
    public SpriteRenderer outputSpriteRenderer;

    void Start()
    {
        Debug.Log("Start");

        Texture2D inputTexture = new(0, 0);
        inputTexture.LoadImage(File.ReadAllBytes("Assets\\_My Assets\\Sprites\\Creature.png"));

        Texture2D outputTexture = RemoveBackground(inputTexture);
        Sprite outputSprite = Sprite.Create(outputTexture, new Rect(0, 0, outputTexture.width, outputTexture.height), Vector2.zero);
        outputSpriteRenderer.sprite = outputSprite;
        outputSpriteRenderer.size = new(5.0f, 5.0f);

        Debug.Log("Done");
    }

    private Texture2D RemoveBackground(Texture2D source)
    {
        int width = source.width;
        int height = source.height;

        // Create empty output texture and make it transparent
        Texture2D output = new(width, height);
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                output.SetPixel(x, y, Color.clear);

        // Postprocessing setup
        Color backgroundColor = source.GetPixel(width / 10, height / 10); // Assume near top-left is background color
        Queue<Vector2Int> pixelQueue = new();
        bool[,] marked = new bool[width, height];

        // Find the pixels, starting from the center and moving out vertically, that is not the background
        // Enqueue these two non-background pixels to start the postprocessing from
        Vector2Int middle = new Vector2Int(width / 2, height / 2);
        Vector2Int top = new Vector2Int(middle.x, middle.y + 1);
        Vector2Int bottom = new Vector2Int(middle.x, middle.y - 1);
        bool foundTop = false;
        bool foundBottom = false;
        do
        {
            // Top
            if (top.y < height)
            {
                Color topColor = source.GetPixel(top.x, top.y);
                if (IsSimilarColor(topColor, backgroundColor))
                    top.y++;
                else
                {
                    pixelQueue.Enqueue(top);
                    foundTop = true;
                }
            }
            else
                foundTop = true;
            // Bottom
            if (bottom.y >= 0)
            {
                Color bottomColor = source.GetPixel(bottom.x, bottom.y);
                if (IsSimilarColor(bottomColor, backgroundColor))
                    bottom.y--;
                else
                {
                    pixelQueue.Enqueue(bottom);
                    foundBottom = true;
                }
            }
            else
                foundBottom = true;
        }
        while (!foundTop || !foundBottom);

        // Postprocessing algorithm (spread outwards from middle)
        while (pixelQueue.Count > 0)
        {
            // Take out the next pixel in the queue and add it to the output texture
            Vector2Int pixelPosition = pixelQueue.Dequeue();
            Color pixelColor = source.GetPixel(pixelPosition.x, pixelPosition.y);
            output.SetPixel(pixelPosition.x, pixelPosition.y, pixelColor);

            // Queue neighbouring pixels if they are in bounds, not marked, and not the white background
            Vector2Int[] neighbouringPixels =
            {
                new(pixelPosition.x - 1, pixelPosition.y),
                new(pixelPosition.x + 1, pixelPosition.y),
                new(pixelPosition.x, pixelPosition.y + 1),
                new(pixelPosition.x, pixelPosition.y - 1)
            };
            for (int i = 0; i < neighbouringPixels.Length; i++)
            {
                Vector2Int neighbourPosition = neighbouringPixels[i];
                if (neighbourPosition.x >= 0 && neighbourPosition.x < width && neighbourPosition.y >= 0 && neighbourPosition.y < height && !marked[neighbourPosition.x, neighbourPosition.y])
                {
                    Color neighbourColor = source.GetPixel(neighbourPosition.x, neighbourPosition.y);
                    if (IsSimilarColor(neighbourColor, backgroundColor) == false)
                    {
                        pixelQueue.Enqueue(neighbourPosition);
                        marked[neighbourPosition.x, neighbourPosition.y] = true;
                    }
                }
            }
        }

        // Apply changes to the output texture and return it
        output.Apply();
        return output;
    }

    private bool IsSimilarColor(Color colorA, Color colorB)
    {
        float similarColorThreshold = 0.04f;

        float rDistance = Mathf.Abs(colorA.r - colorB.r);
        float gDistance = Mathf.Abs(colorA.g - colorB.g);
        float bDistance = Mathf.Abs(colorA.b - colorB.b);

        return rDistance <= similarColorThreshold && gDistance <= similarColorThreshold && bDistance <= similarColorThreshold;
    }
}
