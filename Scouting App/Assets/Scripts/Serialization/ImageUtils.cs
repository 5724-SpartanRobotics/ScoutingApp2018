using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

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
			image.Apply();

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


		public static Texture2D[] EncodeToQRCodes(Stream stream)
		{
			const int max = 1900;
			MD5 md5 = MD5.Create();
			Texture2D[] textures = new Texture2D[(int)Mathf.Ceil(stream.Length / max)];
			List<long> list = new List<long>();
			Stopwatch watch = new Stopwatch();
			watch.Start();
			for (int codeIdx = 0; codeIdx < Mathf.Ceil(stream.Length / max); codeIdx++)
			{
				list.Add(watch.ElapsedTicks);
				byte[] data = new byte[max + 18];
				stream.Read(data, 0, max);
				list.Add(watch.ElapsedTicks);

				byte[] hash = md5.ComputeHash(data, 0, max);
				list.Add(watch.ElapsedTicks);
				for (int i = 0; i < hash.Length; i++)
					data[max + i] = hash[i];
				data[data.Length - 2] = (byte)((codeIdx >> 8) & 0xFF);
				data[data.Length - 1] = (byte)(codeIdx & 0xFF);

				QRCodeWriter qrCoder = new QRCodeWriter();

				int width = 300;
				Dictionary<EncodeHintType, object> hints = new Dictionary<EncodeHintType, object>
				{
					{ EncodeHintType.MARGIN, -1 },
					{ EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.L }
				};
				list.Add(watch.ElapsedTicks);
				BitMatrix pixels = qrCoder.encode(GetStringFromBytes(data), BarcodeFormat.QR_CODE, width, width, hints);
				list.Add(watch.ElapsedTicks);

				bool[][] newPixels = StripBoarder(pixels);
				list.Add(watch.ElapsedTicks);
				width = newPixels.Length;

				textures[codeIdx] = new Texture2D(width, width, TextureFormat.RGB24, false);
				list.Add(watch.ElapsedTicks);

				for (int i = 0; i < width; i++)
					for (int j = 0; j < width; j++)
						textures[codeIdx].SetPixel(i, j, newPixels[i][j] ? Color.black : Color.white);
				list.Add(watch.ElapsedTicks);
				textures[codeIdx].Apply(false, true);
				list.Add(watch.ElapsedTicks);
				watch.Restart();
			}
			string s = "";
			foreach (int i in list)
				s += i + ",";

			return textures;
		}

		private static byte[] GetBytesFromString(string s)
		{
			char[] chars = s.ToCharArray();
			byte[] data = new byte[chars.Length];

			for (int i = 0; i < chars.Length; i++)
				data[i] = (byte)chars[i];

			return data;
		}

		private static string GetStringFromBytes(byte[] data)
		{
			char[] chars = new char[data.Length];
			for (int i = 0; i < data.Length; i++)
				chars[i] = (char)(data[i]);

			return new string(chars);
		}

		private static bool[][] StripBoarder(BitMatrix pixels)
		{
			bool[][] newPixels;
			int x;
			bool end = false;

			for (x = 0; x < pixels.Width; x++)
			{
				if (pixels[x, x])
				{
					end = true;
					break;
				}

				if (end)
					break;
			}
			x -= 5; // Leave a five pixel wide boarder

			int len = pixels.Dimension - x * 2;
			newPixels = new bool[len][];
			for (int i = 0; i < len; i++)
				newPixels[i] = new bool[len];

			for (int i = 0; i < len; i++)
				for (int j = 0; j < len; j++)
					newPixels[i][j] = pixels[i + x, j + x];

			return newPixels;
		}
	}

}
