using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Girl.BMPtoEMF
{
	/// <summary>
	/// ViewerBox の概要の説明です。
	/// </summary>
	public class ViewerBox : System.Windows.Forms.Panel, IMessageFilter
	{
		private System.Windows.Forms.PictureBox pictureBox;
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;
		private static int DefaultZoom = 5;
		private static int[] zooms = new int[] { 10, 25, 33, 50, 75, 100, 150, 200, 300, 400, 800 };
		private int zoom = 3;

		public ViewerBox()
		{
			// この呼び出しは、Windows.Forms フォーム デザイナで必要です。
			InitializeComponent();

			// TODO: InitializeComponent 呼び出しの後に初期化処理を追加します。
			Application.AddMessageFilter(this);
		}

		/// <summary>
		/// 使用されているリソースに後処理を実行します。
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region コンポーネント デザイナで生成されたコード 
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this.pictureBox = new System.Windows.Forms.PictureBox();
			this.SuspendLayout();
			// 
			// pictureBox
			// 
			this.pictureBox.Location = new System.Drawing.Point(17, 17);
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(5, 5);
			this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox.TabIndex = 0;
			this.pictureBox.TabStop = false;
			this.pictureBox.Visible = false;
			this.pictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseUp);
			this.pictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseMove);
			this.pictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseDown);
			// 
			// ViewerBox
			// 
			this.AutoScroll = true;
			this.Controls.Add(this.pictureBox);
			this.Size = new System.Drawing.Size(296, 304);
			this.ResumeLayout(false);

		}
		#endregion

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			this.SetPictureBoxPosition();
		}

		private void SetPictureBoxPosition()
		{
			this.pictureBox.Location = this.GetPictureBoxPosition(this.pictureBox.Size);
		}

		private Point GetPictureBoxPosition(Size psz)
		{
			Point ret = new Point();
			Size sz = this.ClientRectangle.Size;
			if (this.pictureBox.Visible && sz.Width > psz.Width)
			{
				ret.X = (sz.Width - psz.Width) / 2;
			}
			if (this.pictureBox.Visible && sz.Height > psz.Height)
			{
				ret.Y = (sz.Height - psz.Height) / 2;
			}
			Point p = this.AutoScrollPosition;
			ret.Offset(p.X, p.Y);
			return ret;
		}

		private Point autoScrollPoint = Point.Empty;
		private MouseButtons pictureBox_MouseButton = MouseButtons.None;
		private Point pictureBox_MouseDownPoint = Point.Empty;
		private Point pictureBox_MouseMovePoint = Point.Empty;

		private void pictureBox_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (this.pictureBox_MouseButton != MouseButtons.None)
			{
				this.EndMove();
			}
			else if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle)
			{
				this.pictureBox_MouseButton = e.Button;
				this.pictureBox_MouseDownPoint = Cursor.Position;
				this.autoScrollPoint = new Point(-this.AutoScrollPosition.X, -this.AutoScrollPosition.Y);
				Cursor.Current = Cursors.Hand;
			}
		}

		private void pictureBox_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (this.pictureBox_MouseButton != MouseButtons.None)
			{
				Point p = Cursor.Position;
				if (this.pictureBox_MouseMovePoint == p) return;

				this.pictureBox_MouseMovePoint = p;
				this.AutoScrollPosition = new Point(
					this.autoScrollPoint.X + (this.pictureBox_MouseDownPoint.X - p.X),
					this.autoScrollPoint.Y + (this.pictureBox_MouseDownPoint.Y - p.Y));
			}
		}

		private void pictureBox_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (this.pictureBox_MouseButton == e.Button)
			{
				this.EndMove();
			}
		}

		private void EndMove()
		{
			this.pictureBox_MouseButton = MouseButtons.None;
			Cursor.Current = Cursors.Default;
		}

		private void pictureBox_MouseWheel(object sender, MouseEventArgs e)
		{
			//if ((Control.ModifierKeys & Keys.Control) != 0) return;

			if (e.Delta < 0)
			{
				this.SetZoom(this.zoom - 1, e.X, e.Y);
			}
			else if (e.Delta > 0)
			{
				this.SetZoom(this.zoom + 1, e.X, e.Y);
			}
		}

		#region Zoom

		public event EventHandler ZoomChanged;

		public void SetZoom(int zoom, int px, int py)
		{
			if (zoom < 0 || zoom >= zooms.Length) return;

			int oldZoom = this.Zoom, z = zooms[zoom];
			Size sz = this.pictureBox.Image.Size;
			Size psz = new Size(sz.Width * z / 100, sz.Height * z / 100);
			if (psz.Width > 5000 || psz.Height > 5000) return;

			this.zoom = zoom;
			Point offset = this.PointToClient(this.pictureBox.PointToScreen(new Point(px, py)));
			this.pictureBox.Bounds = new Rectangle(this.GetPictureBoxPosition(psz), psz);
			this.AutoScrollPosition = new Point(
				px * z / oldZoom - offset.X, py * z / oldZoom - offset.Y);
			if (this.ZoomChanged != null) this.ZoomChanged(this, EventArgs.Empty);
		}

		public int Zoom
		{
			get
			{
				return zooms[this.zoom];
			}
		}

		#endregion

		public Image Image
		{
			get
			{
				return this.pictureBox.Image;
			}

			set
			{
				this.zoom = DefaultZoom;
				this.pictureBox.Image = value;
				if (value != null)
				{
					int w = Image.Width, h = Image.Height;
					Size cl = this.ClientSize;
					if (this.enableFit)
					{
						for (int i = 0; i < zooms.Length; i++)
						{
                            this.zoom = i;
							int ww = w * this.Zoom / 100, hh = h * this.Zoom / 100;
							if (ww > cl.Width || hh > cl.Height)
							{
								if (i > 0) this.zoom = i - 1;
								break;
							}
						}
					}
					Size sz = new Size(w * this.Zoom / 100, h * this.Zoom / 100);
					this.pictureBox.Bounds = new Rectangle(Point.Empty, sz);
					this.pictureBox.Visible = true;
				}
				else
				{
					this.pictureBox.Visible = false;
				}
				this.AutoScrollPosition = Point.Empty;
				this.SetPictureBoxPosition();
				if (this.ZoomChanged != null) this.ZoomChanged(this, EventArgs.Empty);
			}
		}

		public PictureBox PictureBox
		{
			get
			{
				return this.pictureBox;
			}
		}

        private bool enableZoom = false;
		
		public bool EnableZoom
		{
			get
			{
				return this.enableZoom;
			}

			set
			{
				this.enableZoom = value;
			}
		}

		private bool enableFit = false;
		
		public bool EnableFit
		{
			get
			{
				return this.enableFit;
			}

			set
			{
				this.enableFit = value;
			}
		}

		#region IMessageFilter メンバ

		public bool PreFilterMessage(ref Message m)
		{
			const int WM_MOUSEWHEEL    = 0x020A;
			const int MK_CONTROL       = 0x0008;

			if (m.Msg != WM_MOUSEWHEEL) return false;

			var p = Cursor.Position;
			int wp = m.WParam.ToInt32();
			if (enableZoom && (wp & MK_CONTROL) == 0)
			{
				Rectangle rect = this.RectangleToScreen(this.ClientRectangle);
				if (rect.Contains(p))
				{
					Point pp = this.pictureBox.PointToClient(p);
					var e = new MouseEventArgs(MouseButtons.None, 0, pp.X, pp.Y, wp >> 16);
					this.pictureBox_MouseWheel(this.pictureBox, e);
					return true;
				}
			}

			IntPtr hWnd = WindowFromPoint(p);
			if (hWnd == m.HWnd || hWnd == IntPtr.Zero) return false;

			SendMessage(hWnd, m.Msg, m.WParam, m.LParam);
			return true;
		}

        [DllImport("User32.dll")]
        public static extern IntPtr SendMessage(
            IntPtr hWnd,    // 送信先ウィンドウのハンドル
            int Msg,        // メッセージ
            IntPtr wParam,  // メッセージの最初のパラメータ
            IntPtr lParam   // メッセージの 2 番目のパラメータ
        );

        [DllImport("User32.dll")]
        public static extern IntPtr WindowFromPoint(
            Point p  // 座標
        );

		#endregion
	}
}
