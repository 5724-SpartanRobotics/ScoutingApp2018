using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

		// Yes I know it's at the end. I don't care. I'm calling it a header, not a footer.
		const int QR_HEADER_LEN = 20;

		public static byte[] DecodeQRCode(Texture2D texture, out int qrNumber, out int totalQRs)
		{
			QRCodeReader reader = new QRCodeReader();
			Color32[] color32Data = texture.GetPixels32();
			LuminanceSource source = new Color32LuminanceSource(color32Data, texture.width, texture.height);
			Binarizer binarizer = new HybridBinarizer(source);
			BinaryBitmap bmp = new BinaryBitmap(binarizer);

			Dictionary<DecodeHintType, object> decodeHints = new Dictionary<DecodeHintType, object>
			{
				{ DecodeHintType.TRY_HARDER, true },
				{ DecodeHintType.POSSIBLE_FORMATS, BarcodeFormat.QR_CODE }
			};
			Result result = reader.decode(bmp, decodeHints);

			if (result != null)
			{
				List<byte[]> byteSegments = (List<byte[]>)result.ResultMetadata[ResultMetadataType.BYTE_SEGMENTS];
				int totalLen = byteSegments.Select(x => x.Length).Sum();
				byte[] data = new byte[totalLen];
				int idx = 0;
				foreach (byte[] seg in byteSegments)
				{
					seg.CopyTo(data, idx);
					idx += seg.Length;
				}

				bool valid = true;
				MD5 md5 = MD5.Create();
				byte[] hash = md5.ComputeHash(data, 0, data.Length - QR_HEADER_LEN);

				for (int i = 0; i < hash.Length; i++)
				{
					if (data[data.Length - (QR_HEADER_LEN - i)] != hash[i])
					{
						valid = false;
						break;
					}
				}

				if (valid)
				{
					qrNumber = data[data.Length - 4] << 8;
					qrNumber |= data[data.Length - 3];
					totalQRs = data[data.Length - 2] << 8;
					totalQRs |= data[data.Length - 1];
					MemoryStream ret = new MemoryStream(data.Length - QR_HEADER_LEN);
					ret.Write(data, 0, data.Length - QR_HEADER_LEN);
					return ret.ToArray();
				}
			}
			qrNumber = 0;
			totalQRs = 0;
			return null;
		}

		public static readonly Dictionary<int, int> _QrVersionLSizes = new Dictionary<int, int>()
		{
			{ 20, 858 },
			{ 21, 929 },
			{ 22, 1003 },
			{ 23, 1091 },
			{ 24, 1171 },
			{ 25, 1273 },
			{ 26, 1367 },
			{ 27, 1465 },
			{ 28, 1528 },
			{ 29, 1628 },
			{ 30, 1732 },
			{ 31, 1840 },
			{ 32, 1952 },
			{ 33, 2068 },
			{ 34, 2188 },
			{ 35, 2303 },
			{ 36, 2431 },
			{ 37, 2563 },
			{ 38, 2699 },
			{ 39, 2809 },
			{ 40, 2953 }
		};

		public static readonly Dictionary<int, int> _QrVersionMSizes = new Dictionary<int, int>()
		{
			{ 20, 666 },
			{ 21, 711 },
			{ 22, 779 },
			{ 23, 857 },
			{ 24, 911 },
			{ 25, 997 },
			{ 26, 1059 },
			{ 27, 1125 },
			{ 28, 1190 },
			{ 29, 1264 },
			{ 30, 1370 },
			{ 31, 1452 },
			{ 32, 1538 },
			{ 33, 1628 },
			{ 34, 1722 },
			{ 35, 1809 },
			{ 36, 1911 },
			{ 37, 1989 },
			{ 38, 2099 },
			{ 39, 2213 },
			{ 40, 2331 }
		};

		public static readonly Dictionary<int, int> _QrVersionQSizes = new Dictionary<int, int>()
		{
			{ 20, 482 },
			{ 21, 509 },
			{ 22, 565 },
			{ 23, 611 },
			{ 24, 661 },
			{ 25, 715 },
			{ 26, 751 },
			{ 27, 805 },
			{ 28, 868 },
			{ 29, 908 },
			{ 30, 982 },
			{ 31, 1030 },
			{ 32, 1112 },
			{ 33, 1168 },
			{ 34, 1228 },
			{ 35, 1283 },
			{ 36, 1351 },
			{ 37, 1423 },
			{ 38, 1499 },
			{ 39, 1579 },
			{ 40, 1663 }
		};

		public static readonly Dictionary<int, int> _QrVersionHSizes = new Dictionary<int, int>()
		{
			{ 20, 382 },
			{ 21, 403 },
			{ 22, 439 },
			{ 23, 461 },
			{ 24, 511 },
			{ 25, 535 },
			{ 26, 593 },
			{ 27, 625 },
			{ 28, 658 },
			{ 29, 698 },
			{ 30, 742 },
			{ 31, 790 },
			{ 32, 842 },
			{ 33, 898 },
			{ 34, 958 },
			{ 35, 983 },
			{ 36, 1051 },
			{ 37, 1093 },
			{ 38, 1139 },
			{ 39, 1219 },
			{ 40, 1273 }
		};

		public static Texture2D[] EncodeToQRCodes(Stream stream, int qrVersion, ErrorCorrectionLevel errorLvl)
		{
			int max = -QR_HEADER_LEN;
			if (errorLvl == ErrorCorrectionLevel.L)
				max += _QrVersionLSizes[qrVersion];
			else if (errorLvl == ErrorCorrectionLevel.M)
				max += _QrVersionMSizes[qrVersion];
			else if (errorLvl == ErrorCorrectionLevel.Q)
				max += _QrVersionQSizes[qrVersion];
			else if (errorLvl == ErrorCorrectionLevel.H)
				max += _QrVersionHSizes[qrVersion];

			MD5 md5 = MD5.Create();

			int qrTotal = (int)Mathf.Ceil((float)stream.Length / max);

			Texture2D[] textures = new Texture2D[qrTotal];

			for (int codeIdx = 0; codeIdx < qrTotal; codeIdx++)
			{
				byte[] data = new byte[max + QR_HEADER_LEN];
				stream.Read(data, 0, max);

				byte[] hash = md5.ComputeHash(data, 0, max);
				for (int i = 0; i < hash.Length; i++)
					data[max + i] = hash[i];
				data[data.Length - 4] = (byte)((codeIdx >> 8) & 0xFF);
				data[data.Length - 3] = (byte)(codeIdx & 0xFF);
				data[data.Length - 2] = (byte)((qrTotal >> 8) & 0xFF);
				data[data.Length - 1] = (byte)(qrTotal & 0xFF);

				QRCodeWriter qrCoder = new QRCodeWriter();

				int width = 300;
				Dictionary<EncodeHintType, object> hints = new Dictionary<EncodeHintType, object>
				{
					{ EncodeHintType.MARGIN, 1 },
					{ EncodeHintType.ERROR_CORRECTION, errorLvl },
					{ EncodeHintType.QR_VERSION, qrVersion }
				};
				BitMatrix pixels = qrCoder.encode(GetStringFromBytes(data), BarcodeFormat.QR_CODE, width, width, hints);

				bool[][] newPixels = StripBoarder(pixels);
				width = newPixels.Length;

				textures[codeIdx] = new Texture2D(width, width, TextureFormat.RGB24, false)
				{
					filterMode = FilterMode.Point // Make resizing sharper so the reader can read better
				};

				for (int i = 0; i < width; i++)
					for (int j = 0; j < width; j++)
						textures[codeIdx].SetPixel(i, j, newPixels[i][j] ? Color.black : Color.white);
				textures[codeIdx].Apply(false, true);
			}

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

			for (x = 0; x < pixels.Width; x++)
				if (pixels[x, x])
					break;

			const int borderSize = 20;
			int sizeCovered = pixels.Dimension - x * 2;
			int newSize = sizeCovered + borderSize * 2;
			newPixels = new bool[newSize][];
			for (int i = 0; i < newSize; i++)
				newPixels[i] = new bool[newSize];

			for (int i = 0; i < sizeCovered; i++)
				for (int j = 0; j < sizeCovered; j++)
					newPixels[i + borderSize][j + borderSize] = pixels[i + x, j + x];

			return newPixels;
		}
	}

}
