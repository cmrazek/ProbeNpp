﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ProbeNpp
{
	internal static class Util
	{
		/// <summary>
		/// Splits a string containing semi-colon delimited paths into a list of strings.
		/// </summary>
		/// <param name="str">The path string.</param>
		/// <returns>A list of paths contained in the string.</returns>
		public static List<string> ParsePathString(string str)
		{
			var ret = new List<string>();
			foreach (var path in str.Split(';'))
			{
				if (!string.IsNullOrWhiteSpace(path)) ret.Add(path.Trim());
			}
			return ret;
		}

		/// <summary>
		/// Splits a string containing distinct words delimited by whitespace.
		/// </summary>
		/// <param name="wordList">The string to be split.</param>
		/// <returns>A hash set containing the word list.</returns>
		public static HashSet<string> ParseWordList(string wordList)
		{
			var ret = new HashSet<string>();
			foreach (var word in (from w in wordList.Split(' ', '\t', '\r', '\n') where !string.IsNullOrEmpty(w) select w))
			{
				ret.Add(word);
			}
			return ret;
		}

		public static SizeF MeasureString(Graphics g, string str, Font font, Rectangle rect, StringFormat sf)
		{
			var sfTemp = sf.Clone() as StringFormat;
			var ranges = new CharacterRange[] { new CharacterRange(0, str.Length) };
			sfTemp.SetMeasurableCharacterRanges(ranges);

			var regions = g.MeasureCharacterRanges(str, font, rect, sfTemp);
			if (regions != null && regions.Length > 0) return regions[0].GetBounds(g).Size;
			return new SizeF();
		}

		public static Color MixColor(Color a, Color b, float ratio)
		{
			if (ratio < 0.0f) ratio = 0.0f;
			else if (ratio > 1.0f) ratio = 1.0f;

			var minusRatio = 1.0f - ratio;

			return Color.FromArgb(
				(int)(((a.A / 255.0f) * minusRatio + (b.A / 255.0f) * ratio) * 255.0f),
				(int)(((a.R / 255.0f) * minusRatio + (b.R / 255.0f) * ratio) * 255.0f),
				(int)(((a.G / 255.0f) * minusRatio + (b.G / 255.0f) * ratio) * 255.0f),
				(int)(((a.B / 255.0f) * minusRatio + (b.B / 255.0f) * ratio) * 255.0f));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static Bitmap ToBitmap(this Icon icon)
		{
			var bmp = new Bitmap(icon.Width, icon.Height);
			using (var g = Graphics.FromImage(bmp))
			{
				g.Clear(Color.Transparent);
				g.DrawIcon(icon, new Rectangle(0, 0, icon.Width, icon.Height));
			}
			return bmp;
		}

		public static Font GetMonospaceFont(float size)
		{
			var preferredFonts = new string[] { "Courier", "Courier New", "Lucida Console", "Consolas" };

			FontFamily bestFF = null;
			int bestRank = -1;

			foreach (var ff in FontFamily.Families)
			{
				int rank = -1;
				for (int i = 0, ii = preferredFonts.Length; i < ii; i++)
				{
					if (string.Equals(preferredFonts[i], ff.Name, StringComparison.InvariantCultureIgnoreCase))
					{
						rank = i;
						break;
					}
				}

				if (rank != -1)
				{
					if (rank > bestRank)
					{
						bestFF = ff;
						bestRank = rank;
					}
				}
			}

			if (bestRank == -1) return new Font(FontFamily.GenericMonospace, size);
			else return new Font(bestFF, size);
		}
	}
}
