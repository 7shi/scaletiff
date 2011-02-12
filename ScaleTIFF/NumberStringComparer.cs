using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;

namespace Scale_TIFF
{
	/// <summary>
	/// 文字列中の任意の桁の数字を認識して比較します。
	/// </summary>
	public class NumberStringComparer : IComparer<string>
	{
		/// <summary>
		/// コンストラクタです。
		/// </summary>
        public int Compare(string x, string y)
		{
			int cmp;
			object xo, yo;
			string xs, ys;
			char xc, yc;
			ArrayList xl = NumberStringComparer.SplitString(x.ToString());
			ArrayList yl = NumberStringComparer.SplitString(y.ToString());
			for (int i = 0; i < xl.Count; i++)
			{
				xo = xl[i];
				yo = yl[i];
				if (xo is string && yo is string)
				{
					xs = xo as string;
					ys = yo as string;
					if (xs.Length < ys.Length)
					{
						return -1;
					}
					else if (xs.Length > ys.Length)
					{
						return 1;
					}
					cmp = xs.CompareTo(ys);
				}
				else
				{
					if (xo is char)
					{
						xc =(char) xo;
					}
					else
					{
						xc = xo.ToString()[0];
					}
					if (yo is char)
					{
						yc =(char) yo;
					}
					else
					{
						yc = yo.ToString()[0];
					}
					cmp = xc.CompareTo(yc);
				}
				if (cmp != 0) return cmp;
			}
			if (xl.Count < yl.Count)
			{
				return -1;
			}
			else if (xl.Count > yl.Count)
			{
				return 1;
			}
			return 0;
		}

		/// <summary>
		/// デストラクタです。
		/// </summary>
		public static ArrayList SplitString(string s)
		{
			ArrayList ret = new ArrayList();
			StringBuilder num = new StringBuilder();
			foreach (char ch in s)
			{
				if ('0' <= ch && ch <= '9')
				{
					num.Append(ch);
				}
				else if ('０' <= ch && ch <= '９')
				{
					num.Append((char) ('0' +(ch - '０')));
				}
				else
				{
					if (num.Length > 0)
					{
						ret.Add(num.ToString());
						num.Length = 0;
					}
					ret.Add(ch);
				}
			}
			if (num.Length > 0) ret.Add(num.ToString());
			return ret;
		}
	}
}
