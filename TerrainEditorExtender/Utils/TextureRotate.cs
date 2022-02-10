using UnityEngine;

namespace Megalith
{
    public class TextureRotate
    {
        public static Color[] RotateTexture(Color[] textureArray, int width, int height, float angle, out Vector2Int newBounds)
        {
            float phi = -angle * Mathf.Deg2Rad;
            int i, j;
            int x, y;
            float fDistance, fPolarAngle;
            int iFloorX, iCeilingX, iFloorY, iCeilingY;
            float fTrueX, fTrueY;
            float fDeltaX, fDeltaY;
            Color clrTopLeft, clrTopRight, clrBottomLeft, clrBottomRight;
            float fTopRed, fTopGreen, fTopBlue, fTopAlpha;
            float fBottomRed, fBottomGreen, fBottomBlue, fBottomAlpha;
            int iRed, iGreen, iBlue, iAlpha;
            int iCentreX, iCentreY;
            int iDestCentre;
            int iWidth, iHeight;
            int iDiagonal;
            int cnSizeBuffer = 2;
            iWidth = width;
            iHeight = height;
            iDiagonal = (int)(Mathf.Ceil(Mathf.Sqrt((width * width + height * height)))) + cnSizeBuffer;
            Vector2Int sourceSize = new Vector2Int(iWidth, iHeight);


            iCentreX = iWidth / 2;
            iCentreY = iHeight / 2;
            iDestCentre = iDiagonal / 2;

            Color[] bilinearInterpolation = new Color[iDiagonal * iDiagonal];
            Vector2Int destSize = new Vector2Int(iDiagonal, iDiagonal);

            for (i = 0; i < bilinearInterpolation.Length; ++i)
            {
                bilinearInterpolation[i] = Color.clear;
            }

            for (i = 0; i < iDiagonal; ++i)
            {
                for (j = 0; j < iDiagonal; ++j)
                {
                    // convert raster to Cartesian
                    x = j - iDestCentre;
                    y = iDestCentre - i;

                    // convert Cartesian to polar
                    fDistance = Mathf.Sqrt(x * x + y * y);
                    fPolarAngle = 0f;
                    if (x == 0)
                    {
                        if (y == 0)
                        {
                            // centre of image, no rotation needed
                            bilinearInterpolation.SetPixel(j, i, destSize, textureArray.GetPixel(j, i, sourceSize));
                            continue;
                        }
                        else if (y < 0)
                        {
                            fPolarAngle = 1.5f * Mathf.PI;
                        }
                        else
                        {
                            fPolarAngle = 0.5f * Mathf.PI;
                        }
                    }
                    else
                    {
                        fPolarAngle = Mathf.Atan2(y, x);
                    }

                    fPolarAngle -= phi;

                    // convert polar to Cartesian
                    fTrueX = fDistance * Mathf.Cos(fPolarAngle);
                    fTrueY = fDistance * Mathf.Sin(fPolarAngle);

                    // convert Cartesian to raster
                    fTrueX = fTrueX + iCentreX;
                    fTrueY = iCentreY - fTrueY;

                    iFloorX = (int)(Mathf.Floor(fTrueX));
                    iFloorY = (int)(Mathf.Floor(fTrueY));
                    iCeilingX = (int)(Mathf.Ceil(fTrueX));
                    iCeilingY = (int)(Mathf.Ceil(fTrueY));

                    // check bounds
                    if (iFloorX < 0 || iCeilingX < 0 || iFloorX >= iWidth || iCeilingX >= iWidth || iFloorY < 0 || iCeilingY < 0 || iFloorY >= iHeight || iCeilingY >= iHeight) continue;

                    fDeltaX = fTrueX - (float)iFloorX;
                    fDeltaY = fTrueY - (float)iFloorY;

                    clrTopLeft = textureArray.GetPixel(iFloorX, iFloorY, sourceSize);
                    clrTopRight = textureArray.GetPixel(iCeilingX, iFloorY, sourceSize);
                    clrBottomLeft = textureArray.GetPixel(iFloorX, iCeilingY, sourceSize);
                    clrBottomRight = textureArray.GetPixel(iCeilingX, iCeilingY, sourceSize);

                    // linearly interpolate horizontally between top neighbours
                    fTopRed = (1 - fDeltaX) * clrTopLeft.r + fDeltaX * clrTopRight.r;
                    fTopGreen = (1 - fDeltaX) * clrTopLeft.g + fDeltaX * clrTopRight.g;
                    fTopBlue = (1 - fDeltaX) * clrTopLeft.b + fDeltaX * clrTopRight.b;
                    fTopAlpha = (1 - fDeltaX) * clrTopLeft.a + fDeltaX * clrTopRight.a;

                    // linearly interpolate horizontally between bottom neighbours
                    fBottomRed = (1 - fDeltaX) * clrBottomLeft.r + fDeltaX * clrBottomRight.r;
                    fBottomGreen = (1 - fDeltaX) * clrBottomLeft.g + fDeltaX * clrBottomRight.g;
                    fBottomBlue = (1 - fDeltaX) * clrBottomLeft.b + fDeltaX * clrBottomRight.b;
                    fBottomAlpha = (1 - fDeltaX) * clrBottomLeft.a + fDeltaX * clrBottomRight.a;

                    // linearly interpolate vertically between top and bottom interpolated results
                    iRed = (int)Mathf.Round(((1 - fDeltaY) * fTopRed + fDeltaY * fBottomRed) * 255);
                    iGreen = (int)Mathf.Round(((1 - fDeltaY) * fTopGreen + fDeltaY * fBottomGreen) * 255);
                    iBlue = (int)Mathf.Round(((1 - fDeltaY) * fTopBlue + fDeltaY * fBottomBlue) * 255);
                    iAlpha = (int)Mathf.Round(((1 - fDeltaY) * fTopAlpha + fDeltaY * fBottomAlpha) * 255);

                    // make sure colour values are valid
                    if (iRed < 0) iRed = 0;
                    if (iRed > 255) iRed = 255;
                    if (iGreen < 0) iGreen = 0;
                    if (iGreen > 255) iGreen = 255;
                    if (iBlue < 0) iBlue = 0;
                    if (iBlue > 255) iBlue = 255;
                    if (iAlpha < 0) iAlpha = 0;
                    if (iAlpha > 255) iAlpha = 255;

                    bilinearInterpolation.SetPixel(j, i, destSize, new Color32((byte)iRed, (byte)iGreen, (byte)iBlue, (byte)iAlpha));
                }
            }
            newBounds = destSize;
            return bilinearInterpolation;
        }

    }

    public static class ColorExtensions
    {
        public static Color GetPixel(this Color[] colorArray, int x, int y, Vector2Int size)
        {
            if (x < 0 || y < 0 || x >= size.x || y >= size.y || colorArray.Length < (size.x * size.y))
                return Color.clear;
            return colorArray[y * size.x + x];
        }

        public static void SetPixel(this Color[] colorArray, int x, int y, Vector2Int size, Color pixel)
        {
            if (x < 0 || y < 0 || x >= size.x || y >= size.y || colorArray.Length < (size.x * size.y))
                return;
            colorArray[y * size.x + x] = pixel;
        }
    }
}