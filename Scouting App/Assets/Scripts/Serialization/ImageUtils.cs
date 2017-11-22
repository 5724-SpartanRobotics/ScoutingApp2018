using System;
using UnityEngine;

namespace ScoutingApp
{
	public class ImageUtils
	{
		/// <summary>
		/// Encodes a byte[] into a png image and returns its bytes.
		/// </summary>
		/// <param name="dataArg">The data to encode into an image.</param>
		/// <returns>The png image bytes.</returns>
		public static byte[] EncodeToImage(byte[] dataArg)
		{
			int numPixels = (int)Math.Ceiling(dataArg.Length / 4F) + 1;

			Color32[] pixelData = new Color32[numPixels];

			pixelData[0].r = (byte)((dataArg.Length >> 24) & 0xFF);
			pixelData[0].g = (byte)((dataArg.Length >> 16) & 0xFF);
			pixelData[0].b = (byte)((dataArg.Length >> 8) & 0xFF);
			pixelData[0].a = (byte)(dataArg.Length & 0xFF);

			for (int i = 0; i < dataArg.Length; i++)
			{
				if (i % 4 == 0)
					pixelData[i / 4 + 1].r = dataArg[i];
				else if (i % 4 == 1)
					pixelData[i / 4 + 1].g = dataArg[i];
				else if (i % 4 == 2)
					pixelData[i / 4 + 1].b = dataArg[i];
				else
					pixelData[i / 4 + 1].a = dataArg[i];
			}


			int width = numPixels;
			int height = 1;
			int lastDiff = Math.Abs(width - height);

			// Makes the image less one-dimensional by finding the closest pair
			// of numbers that multiplies to the total number of pixels. It
			// will still be flat if it is a prime number or something like that.
			for (int i = 1; i < numPixels; i++)
			{
				if (numPixels % i == 0)
				{
					int w = numPixels / i;
					int newDiff = Math.Abs(w - i);
					if (newDiff < lastDiff)
					{
						lastDiff = newDiff;
						width = w;
						height = i;
					}
					else if (newDiff > lastDiff)
					{
						break;
					}
				}
			}

			Texture2D image = new Texture2D(width, height, TextureFormat.ARGB32, mipmap: false);
			image.SetPixels32(pixelData);

			return image.EncodeToPNG();
		}

		/// <summary>
		/// Extracts encoded data from a png image loaded from the byte[].
		/// </summary>
		/// <param name="imageData">The png image bytes.</param>
		/// <returns>The data that was encoded in the png image.</returns>
		public static byte[] DecodeFromImage(byte[] imageData)
		{
			Texture2D image = new Texture2D(0, 0, TextureFormat.ARGB32, mipmap: false);
			image.LoadImage(imageData);

			Color32[] pixels = image.GetPixels32();
			int len = pixels[0].r << 24;
			len |= pixels[0].g << 16;
			len |= pixels[0].b << 8;
			len |= pixels[0].a;

			int width = image.width;
			byte[] data = new byte[len];
			for (int i = 0; i < len; i++)
			{
				int idx = i / 4 + 1;
				if (i % 4 == 0)
					data[i] = pixels[idx].r;
				else if (i % 4 == 1)
					data[i] = pixels[idx].g;
				else if (i % 4 == 2)
					data[i] = pixels[idx].b;
				else
					data[i] = pixels[idx].a;
			}

			return data;
		}
	}
}
