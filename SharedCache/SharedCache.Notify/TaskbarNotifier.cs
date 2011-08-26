#region Copyright (c) Roni Schuetz - All Rights Reserved
// * --------------------------------------------------------------------- *
// *                              Roni Schuetz                             *
// *              Copyright (c) 2008 All Rights reserved                   *
// *                                                                       *
// * Shared Cache high-performance, distributed caching and    *
// * replicated caching system, generic in nature, but intended to         *
// * speeding up dynamic web and / or win applications by alleviating      *
// * database load.                                                        *
// *                                                                       *
// * This Software is written by Roni Schuetz (schuetz AT gmail DOT com)   *
// *                                                                       *
// * This library is free software; you can redistribute it and/or         *
// * modify it under the terms of the GNU Lesser General Public License    *
// * as published by the Free Software Foundation; either version 2.1      *
// * of the License, or (at your option) any later version.                *
// *                                                                       *
// * This library is distributed in the hope that it will be useful,       *
// * but WITHOUT ANY WARRANTY; without even the implied warranty of        *
// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU      *
// * Lesser General Public License for more details.                       *
// *                                                                       *
// * You should have received a copy of the GNU Lesser General Public      *
// * License along with this library; if not, write to the Free            *
// * Software Foundation, Inc., 59 Temple Place, Suite 330,                *
// * Boston, MA 02111-1307 USA                                             *
// *                                                                       *
// *       THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.        *
// * --------------------------------------------------------------------- *
#endregion 

// C# TaskbarNotifier Class v1.0
// by John O'Byrne - 02 december 2002
// 01 april 2003 : Small fix in the OnMouseUp handler
// 11 january 2003 : Patrick Vanden Driessche <pvdd@devbrains.be> added a few enhancements
//           Small Enhancements/Bugfix
//           Small bugfix: When Content text measures larger than the corresponding ContentRectangle
//                         the focus rectangle was not correctly drawn. This has been solved.
//           Added KeepVisibleOnMouseOver
//           Added ReShowOnMouseOver
//           Added If the Title or Content are too long to fit in the corresponding Rectangles,
//                 the text is truncateed and the ellipses are appended (StringTrimming).

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace CustomUIControls
{
	/// <summary>
	/// TaskbarNotifier allows to display MSN style/Skinned instant messaging popups
	/// </summary>
	public class TaskbarNotifier : System.Windows.Forms.Form
	{
		#region TaskbarNotifier Protected Members
		/// <summary>
		/// 
		/// </summary>
		protected Bitmap BackgroundBitmap = null;
		/// <summary>
		/// 
		/// </summary>
		protected Bitmap CloseBitmap = null;
		/// <summary>
		/// 
		/// </summary>
		protected Point CloseBitmapLocation;
		/// <summary>
		/// 
		/// </summary>
		protected Size CloseBitmapSize;
		/// <summary>
		/// 
		/// </summary>
		protected Rectangle RealTitleRectangle;
		/// <summary>
		/// 
		/// </summary>
		protected Rectangle RealContentRectangle;
		/// <summary>
		/// 
		/// </summary>
		protected Rectangle WorkAreaRectangle;
		/// <summary>
		/// 
		/// </summary>
		protected Timer timer = new Timer();
		/// <summary>
		/// 
		/// </summary>
		protected TaskbarStates taskbarState = TaskbarStates.hidden;
		/// <summary>
		/// 
		/// </summary>
		protected string titleText;
		/// <summary>
		/// 
		/// </summary>
		protected string contentText;
		/// <summary>
		/// 
		/// </summary>
		protected Color normalTitleColor = Color.FromArgb(255,0,0);
		/// <summary>
		/// 
		/// </summary>
		protected Color hoverTitleColor = Color.FromArgb(255,0,0);
		/// <summary>
		/// 
		/// </summary>
		protected Color normalContentColor = Color.FromArgb(0,0,0);
		/// <summary>
		/// 
		/// </summary>
		protected Color hoverContentColor = Color.FromArgb(0,0,0x66);
		/// <summary>
		/// 
		/// </summary>
		protected Font normalTitleFont = new Font("Arial",12,FontStyle.Regular,GraphicsUnit.Pixel);
		/// <summary>
		/// 
		/// </summary>
		protected Font hoverTitleFont = new Font("Arial",12,FontStyle.Bold,GraphicsUnit.Pixel);
		/// <summary>
		/// 
		/// </summary>
		protected Font normalContentFont = new Font("Arial",11,FontStyle.Regular,GraphicsUnit.Pixel);
		/// <summary>
		/// 
		/// </summary>
		protected Font hoverContentFont = new Font("Arial",11,FontStyle.Regular,GraphicsUnit.Pixel);
		/// <summary>
		/// 
		/// </summary>
		protected int nShowEvents;
		/// <summary>
		/// 
		/// </summary>
		protected int nHideEvents;
		/// <summary>
		/// 
		/// </summary>
		protected int nVisibleEvents;
		/// <summary>
		/// 
		/// </summary>
		protected int nIncrementShow;
		/// <summary>
		/// 
		/// </summary>
		protected int nIncrementHide;
		/// <summary>
		/// 
		/// </summary>
		protected bool bIsMouseOverPopup = false;
		/// <summary>
		/// 
		/// </summary>
		protected bool bIsMouseOverClose = false;
		/// <summary>
		/// 
		/// </summary>
		protected bool bIsMouseOverContent = false;
		/// <summary>
		/// 
		/// </summary>
		protected bool bIsMouseOverTitle = false;
		/// <summary>
		/// 
		/// </summary>
		protected bool bIsMouseDown = false;
		/// <summary>
		/// 
		/// </summary>
		protected bool bKeepVisibleOnMouseOver = true;			// Added Rev 002
		/// <summary>
		/// 
		/// </summary>
		protected bool bReShowOnMouseOver = false;				// Added Rev 002
		#endregion
						
		#region TaskbarNotifier Public Members
		/// <summary>
		/// 
		/// </summary>
		public Rectangle TitleRectangle;
		/// <summary>
		/// 
		/// </summary>
		public Rectangle ContentRectangle;
		/// <summary>
		/// 
		/// </summary>
		public bool TitleClickable = false;
		/// <summary>
		/// 
		/// </summary>
		public bool ContentClickable = true;
		/// <summary>
		/// 
		/// </summary>
		public bool CloseClickable = true;
		/// <summary>
		/// 
		/// </summary>
		public bool EnableSelectionRectangle = true;
		/// <summary>
		/// 
		/// </summary>
		public event EventHandler CloseClick = null;
		/// <summary>
		/// 
		/// </summary>
		public event EventHandler TitleClick = null;
		/// <summary>
		/// 
		/// </summary>
		public event EventHandler ContentClick = null;
		#endregion
	
		#region TaskbarNotifier Enums
		/// <summary>
		/// List of the different popup animation status
		/// </summary>
		public enum TaskbarStates
		{
			/// <summary>
			/// 
			/// </summary>
			hidden = 0,
			/// <summary>
			/// 
			/// </summary>
			appearing = 1,
			/// <summary>
			/// 
			/// </summary>
			visible = 2,
			/// <summary>
			/// 
			/// </summary>
			disappearing = 3
		}
		#endregion

		#region TaskbarNotifier Constructor
		/// <summary>
		/// The Constructor for TaskbarNotifier
		/// </summary>
		public TaskbarNotifier()
		{
			// Window Style
			FormBorderStyle = FormBorderStyle.None;
			WindowState = FormWindowState.Minimized;
			base.Show();
			base.Hide();
			WindowState = FormWindowState.Normal;
			ShowInTaskbar = false;
			TopMost = true;
			MaximizeBox = false;
			MinimizeBox = false;
			ControlBox = false;
			
			timer.Enabled = true;
			timer.Tick += new EventHandler(OnTimer);
		}
		#endregion

		#region TaskbarNotifier Properties
		/// <summary>
		/// Get the current TaskbarState (hidden, showing, visible, hiding)
		/// </summary>
		public TaskbarStates TaskbarState
		{
			get
			{
				return taskbarState;
			}
		}

		/// <summary>
		/// Get/Set the popup Title Text
		/// </summary>
		public string TitleText
		{
			get
			{
				return titleText;
			}
			set
			{
				titleText=value;
				Refresh();
			}
		}

		/// <summary>
		/// Get/Set the popup Content Text
		/// </summary>
		public string ContentText
		{
			get
			{
				return contentText;
			}
			set
			{
				contentText=value;
				Refresh();
			}
		}

		/// <summary>
		/// Get/Set the Normal Title Color
		/// </summary>
		public Color NormalTitleColor
		{
			get
			{
				return normalTitleColor;
			}
			set
			{
				normalTitleColor = value;
				Refresh();
			}
		}

		/// <summary>
		/// Get/Set the Hover Title Color
		/// </summary>
		public Color HoverTitleColor
		{
			get
			{
				return hoverTitleColor;
			}
			set
			{
				hoverTitleColor = value;
				Refresh();
			}
		}

		/// <summary>
		/// Get/Set the Normal Content Color
		/// </summary>
		public Color NormalContentColor
		{
			get
			{
				return normalContentColor;
			}
			set
			{
				normalContentColor = value;
				Refresh();
			}
		}

		/// <summary>
		/// Get/Set the Hover Content Color
		/// </summary>
		public Color HoverContentColor
		{
			get
			{
				return hoverContentColor;
			}
			set
			{
				hoverContentColor = value;
				Refresh();
			}
		}

		/// <summary>
		/// Get/Set the Normal Title Font
		/// </summary>
		public Font NormalTitleFont
		{
			get
			{
				return normalTitleFont;
			}
			set
			{
				normalTitleFont = value;
				Refresh();
			}
		}

		/// <summary>
		/// Get/Set the Hover Title Font
		/// </summary>
		public Font HoverTitleFont
		{
			get
			{
				return hoverTitleFont;
			}
			set
			{
				hoverTitleFont = value;
				Refresh();
			}
		}

		/// <summary>
		/// Get/Set the Normal Content Font
		/// </summary>
		public Font NormalContentFont
		{
			get
			{
				return normalContentFont;
			}
			set
			{
				normalContentFont = value;
				Refresh();
			}
		}

		/// <summary>
		/// Get/Set the Hover Content Font
		/// </summary>
		public Font HoverContentFont
		{
			get
			{
				return hoverContentFont;
			}
			set
			{
				hoverContentFont = value;
				Refresh();
			}
		}

		/// <summary>
		/// Indicates if the popup should remain visible when the mouse pointer is over it.
		/// Added Rev 002
		/// </summary>
		public bool KeepVisibleOnMousOver
		{
			get
			{
				return bKeepVisibleOnMouseOver;
			}
			set
			{
				bKeepVisibleOnMouseOver=value;
			}
		}

		/// <summary>
		/// Indicates if the popup should appear again when mouse moves over it while it's disappearing.
		/// Added Rev 002
		/// </summary>
		public bool ReShowOnMouseOver
		{
			get
			{
				return bReShowOnMouseOver;
			}
			set
			{
				bReShowOnMouseOver=value;
			}
		}

		#endregion

		#region TaskbarNotifier Public Methods
		[DllImport("user32.dll")]
		private static extern Boolean ShowWindow(IntPtr hWnd,Int32 nCmdShow);
		/// <summary>
		/// Displays the popup for a certain amount of time
		/// </summary>
		/// <param name="strTitle">The string which will be shown as the title of the popup</param>
		/// <param name="strContent">The string which will be shown as the content of the popup</param>
		/// <param name="nTimeToShow">Duration of the showing animation (in milliseconds)</param>
		/// <param name="nTimeToStay">Duration of the visible state before collapsing (in milliseconds)</param>
		/// <param name="nTimeToHide">Duration of the hiding animation (in milliseconds)</param>
		/// <returns>Nothing</returns>
		public void Show(string strTitle, string strContent, int nTimeToShow, int nTimeToStay, int nTimeToHide)
		{
			WorkAreaRectangle = Screen.GetWorkingArea(WorkAreaRectangle);
			titleText = strTitle;
			contentText = strContent;
			nVisibleEvents = nTimeToStay;
			CalculateMouseRectangles();

			// We calculate the pixel increment and the timer value for the showing animation
			int nEvents;
			if (nTimeToShow > 10)
			{
				nEvents = Math.Min((nTimeToShow / 10), BackgroundBitmap.Height);
				nShowEvents = nTimeToShow / nEvents;
				nIncrementShow = BackgroundBitmap.Height / nEvents;
			}
			else
			{
				nShowEvents = 10;
				nIncrementShow = BackgroundBitmap.Height;
			}

			// We calculate the pixel increment and the timer value for the hiding animation
			if( nTimeToHide > 10)
			{
				nEvents = Math.Min((nTimeToHide / 10), BackgroundBitmap.Height);
				nHideEvents = nTimeToHide / nEvents;
				nIncrementHide = BackgroundBitmap.Height / nEvents;
			}
			else
			{
				nHideEvents = 10;
				nIncrementHide = BackgroundBitmap.Height;
			}

			switch (taskbarState)
			{
				case TaskbarStates.hidden:
					taskbarState = TaskbarStates.appearing;
					SetBounds(WorkAreaRectangle.Right-BackgroundBitmap.Width-17, WorkAreaRectangle.Bottom-1, BackgroundBitmap.Width, 0);
					timer.Interval = nShowEvents;
					timer.Start();
					// We Show the popup without stealing focus
					ShowWindow(this.Handle, 4);
					break;

				case TaskbarStates.appearing:
					Refresh();
					break;

				case TaskbarStates.visible:
					timer.Stop();
					timer.Interval = nVisibleEvents;
					timer.Start();
					Refresh();
					break;

				case TaskbarStates.disappearing:
					timer.Stop();
					taskbarState = TaskbarStates.visible;
					SetBounds(WorkAreaRectangle.Right-BackgroundBitmap.Width-17, WorkAreaRectangle.Bottom-BackgroundBitmap.Height-1, BackgroundBitmap.Width, BackgroundBitmap.Height);
					timer.Interval = nVisibleEvents;
					timer.Start();
					Refresh();
					break;
			}
		}

		/// <summary>
		/// Hides the popup
		/// </summary>
		/// <returns>Nothing</returns>
		public new void Hide()
		{
			if (taskbarState != TaskbarStates.hidden)
			{
				timer.Stop();
				taskbarState = TaskbarStates.hidden;
				base.Hide();
			}
		}

		/// <summary>
		/// Sets the background bitmap and its transparency color
		/// </summary>
		/// <param name="strFilename">Path of the Background Bitmap on the disk</param>
		/// <param name="transparencyColor">Color of the Bitmap which won't be visible</param>
		/// <returns>Nothing</returns>
		public void SetBackgroundBitmap(string strFilename, Color transparencyColor)
		{
			BackgroundBitmap = new Bitmap(strFilename);
			Width = BackgroundBitmap.Width;
			Height = BackgroundBitmap.Height;
			Region = BitmapToRegion(BackgroundBitmap, transparencyColor);
		}

		/// <summary>
		/// Sets the background bitmap and its transparency color
		/// </summary>
		/// <param name="image">Image/Bitmap object which represents the Background Bitmap</param>
		/// <param name="transparencyColor">Color of the Bitmap which won't be visible</param>
		/// <returns>Nothing</returns>
		public void SetBackgroundBitmap(Image image, Color transparencyColor)
		{
			BackgroundBitmap = new Bitmap(image);
			Width = BackgroundBitmap.Width;
			Height = BackgroundBitmap.Height;
			Region = BitmapToRegion(BackgroundBitmap, transparencyColor);
		}

		/// <summary>
		/// Sets the 3-State Close Button bitmap, its transparency color and its coordinates
		/// </summary>
		/// <param name="strFilename">Path of the 3-state Close button Bitmap on the disk (width must a multiple of 3)</param>
		/// <param name="transparencyColor">Color of the Bitmap which won't be visible</param>
		/// <param name="position">Location of the close button on the popup</param>
		/// <returns>Nothing</returns>
		public void SetCloseBitmap(string strFilename, Color transparencyColor, Point position)
		{
			CloseBitmap = new Bitmap(strFilename);
			CloseBitmap.MakeTransparent(transparencyColor);
			CloseBitmapSize = new Size(CloseBitmap.Width/3, CloseBitmap.Height);
			CloseBitmapLocation = position;
		}

		/// <summary>
		/// Sets the 3-State Close Button bitmap, its transparency color and its coordinates
		/// </summary>
		/// <param name="image">Image/Bitmap object which represents the 3-state Close button Bitmap (width must be a multiple of 3)</param>
		/// <param name="transparencyColor">Color of the Bitmap which won't be visible</param>
		/// /// <param name="position">Location of the close button on the popup</param>
		/// <returns>Nothing</returns>
		public void SetCloseBitmap(Image image, Color transparencyColor, Point position)
		{
			CloseBitmap = new Bitmap(image);
			CloseBitmap.MakeTransparent(transparencyColor);
			CloseBitmapSize = new Size(CloseBitmap.Width/3, CloseBitmap.Height);
			CloseBitmapLocation = position;
		}
		#endregion

		#region TaskbarNotifier Protected Methods
		/// <summary>
		/// Draws the close button.
		/// </summary>
		/// <param name="grfx">The GRFX.</param>
		protected void DrawCloseButton(Graphics grfx)
		{
			if (CloseBitmap != null)
			{	
				Rectangle rectDest = new Rectangle(CloseBitmapLocation, CloseBitmapSize);
				Rectangle rectSrc;

				if (bIsMouseOverClose)
				{
					if (bIsMouseDown)
						rectSrc = new Rectangle(new Point(CloseBitmapSize.Width*2, 0), CloseBitmapSize);
					else
						rectSrc = new Rectangle(new Point(CloseBitmapSize.Width, 0), CloseBitmapSize);
				}		
				else
					rectSrc = new Rectangle(new Point(0, 0), CloseBitmapSize);
					

				grfx.DrawImage(CloseBitmap, rectDest, rectSrc, GraphicsUnit.Pixel);
			}
		}

		/// <summary>
		/// Draws the text.
		/// </summary>
		/// <param name="grfx">The GRFX.</param>
		protected void DrawText(Graphics grfx)
		{
			if (titleText != null && titleText.Length != 0)
			{
				StringFormat sf = new StringFormat();
				sf.Alignment = StringAlignment.Near;
				sf.LineAlignment = StringAlignment.Center;
				sf.FormatFlags = StringFormatFlags.NoWrap;
				sf.Trimming = StringTrimming.EllipsisCharacter;				// Added Rev 002
				if (bIsMouseOverTitle)
					grfx.DrawString(titleText, hoverTitleFont, new SolidBrush(hoverTitleColor), TitleRectangle, sf);
				else
					grfx.DrawString(titleText, normalTitleFont, new SolidBrush(normalTitleColor), TitleRectangle, sf);
			}

			if (contentText != null && contentText.Length != 0)
			{
				StringFormat sf = new StringFormat();
				sf.Alignment = StringAlignment.Center;
				sf.LineAlignment = StringAlignment.Center;
				sf.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
				sf.Trimming = StringTrimming.Word;							// Added Rev 002
				
				if (bIsMouseOverContent)
				{
					grfx.DrawString(contentText, hoverContentFont, new SolidBrush(hoverContentColor), ContentRectangle, sf);
					if (EnableSelectionRectangle)
						ControlPaint.DrawBorder3D(grfx, RealContentRectangle, Border3DStyle.Etched, Border3DSide.Top | Border3DSide.Bottom | Border3DSide.Left | Border3DSide.Right);
					
				}
				else
					grfx.DrawString(contentText, normalContentFont, new SolidBrush(normalContentColor), ContentRectangle, sf);
			}
		}

		/// <summary>
		/// Calculates the mouse rectangles.
		/// </summary>
		protected void CalculateMouseRectangles()
		{
			Graphics grfx = CreateGraphics();
			StringFormat sf = new StringFormat();
			sf.Alignment = StringAlignment.Center;
			sf.LineAlignment = StringAlignment.Center;
			sf.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
			SizeF sizefTitle = grfx.MeasureString(titleText, hoverTitleFont, TitleRectangle.Width, sf);
			SizeF sizefContent = grfx.MeasureString(contentText, hoverContentFont, ContentRectangle.Width, sf);
			grfx.Dispose();

			// Added Rev 002
	        //We should check if the title size really fits inside the pre-defined title rectangle
			if (sizefTitle.Height > TitleRectangle.Height)
			{
				RealTitleRectangle = new Rectangle(TitleRectangle.Left, TitleRectangle.Top, TitleRectangle.Width , TitleRectangle.Height );
			} 
			else
			{
				RealTitleRectangle = new Rectangle(TitleRectangle.Left, TitleRectangle.Top, (int)sizefTitle.Width, (int)sizefTitle.Height);
			}
			RealTitleRectangle.Inflate(0,2);

			// Added Rev 002
			//We should check if the Content size really fits inside the pre-defined Content rectangle
			if (sizefContent.Height > ContentRectangle.Height)
			{
				RealContentRectangle = new Rectangle((ContentRectangle.Width-(int)sizefContent.Width)/2+ContentRectangle.Left, ContentRectangle.Top, (int)sizefContent.Width, ContentRectangle.Height );
			}
			else
			{
				RealContentRectangle = new Rectangle((ContentRectangle.Width-(int)sizefContent.Width)/2+ContentRectangle.Left, (ContentRectangle.Height-(int)sizefContent.Height)/2+ContentRectangle.Top, (int)sizefContent.Width, (int)sizefContent.Height);
			}
			RealContentRectangle.Inflate(0,2);
		}

		/// <summary>
		/// Bitmaps to region.
		/// </summary>
		/// <param name="bitmap">The bitmap.</param>
		/// <param name="transparencyColor">Color of the transparency.</param>
		/// <returns></returns>
		protected Region BitmapToRegion(Bitmap bitmap, Color transparencyColor)
		{
			if (bitmap == null)
				throw new ArgumentNullException("Bitmap", "Bitmap cannot be null!");

			int height = bitmap.Height;
			int width = bitmap.Width;

			GraphicsPath path = new GraphicsPath();

			for (int j=0; j<height; j++ )
				for (int i=0; i<width; i++)
				{
					if (bitmap.GetPixel(i, j) == transparencyColor)
						continue;

					int x0 = i;

					while ((i < width) && (bitmap.GetPixel(i, j) != transparencyColor))
						i++;

					path.AddRectangle(new Rectangle(x0, j, i-x0, 1));
				}

			Region region = new Region(path);
			path.Dispose();
			return region;
		}
		#endregion

		#region TaskbarNotifier Events Overrides
		/// <summary>
		/// Called when [timer].
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <param name="ea">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void OnTimer(Object obj, EventArgs ea)
		{
			switch (taskbarState)
			{
				case TaskbarStates.appearing:
					if (Height < BackgroundBitmap.Height)
						SetBounds(Left, Top-nIncrementShow ,Width, Height + nIncrementShow);
					else
					{
						timer.Stop();
						Height = BackgroundBitmap.Height;
						timer.Interval = nVisibleEvents;
						taskbarState = TaskbarStates.visible;
						timer.Start();
					}
					break;

				case TaskbarStates.visible:
					timer.Stop();
					timer.Interval = nHideEvents;
					// Added Rev 002
					if ((bKeepVisibleOnMouseOver && !bIsMouseOverPopup ) || (!bKeepVisibleOnMouseOver))
					{
						taskbarState = TaskbarStates.disappearing;
					} 
					//taskbarState = TaskbarStates.disappearing;		// Rev 002
					timer.Start();
					break;

				case TaskbarStates.disappearing:
					// Added Rev 002
					if (bReShowOnMouseOver && bIsMouseOverPopup) 
					{
						taskbarState = TaskbarStates.appearing;
					} 
					else 
					{
						if (Top < WorkAreaRectangle.Bottom)
							SetBounds(Left, Top + nIncrementHide, Width, Height - nIncrementHide);
						else
							Hide();
					}
					break;
			}
			
		}

		/// <summary>
		/// Raises the <see cref="E:MouseEnter"/> event.
		/// </summary>
		/// <param name="ea">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected override void OnMouseEnter(EventArgs ea)
		{
			base.OnMouseEnter(ea);
			bIsMouseOverPopup = true;
			Refresh();
		}

		/// <summary>
		/// Raises the <see cref="E:MouseLeave"/> event.
		/// </summary>
		/// <param name="ea">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected override void OnMouseLeave(EventArgs ea)
		{
			base.OnMouseLeave(ea);
			bIsMouseOverPopup = false;
			bIsMouseOverClose = false;
			bIsMouseOverTitle = false;
			bIsMouseOverContent = false;
			Refresh();
		}

		/// <summary>
		/// Raises the <see cref="E:MouseMove"/> event.
		/// </summary>
		/// <param name="mea">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		protected override void OnMouseMove(MouseEventArgs mea)
		{
			base.OnMouseMove(mea);

			bool bContentModified = false;
			
			if ( (mea.X > CloseBitmapLocation.X) && (mea.X < CloseBitmapLocation.X + CloseBitmapSize.Width) && (mea.Y > CloseBitmapLocation.Y) && (mea.Y < CloseBitmapLocation.Y + CloseBitmapSize.Height) && CloseClickable )
			{
				if (!bIsMouseOverClose)
				{
					bIsMouseOverClose = true;
					bIsMouseOverTitle = false;
					bIsMouseOverContent = false;
					Cursor = Cursors.Hand;
					bContentModified = true;
				}
			}
			else if (RealContentRectangle.Contains(new Point(mea.X, mea.Y)) && ContentClickable)
			{
				if (!bIsMouseOverContent)
				{
					bIsMouseOverClose = false;
					bIsMouseOverTitle = false;
					bIsMouseOverContent = true;
					Cursor = Cursors.Hand;
					bContentModified = true;
				}
			}
			else if (RealTitleRectangle.Contains(new Point(mea.X, mea.Y)) && TitleClickable)
			{
				if (!bIsMouseOverTitle)
				{
					bIsMouseOverClose = false;
					bIsMouseOverTitle = true;
					bIsMouseOverContent = false;
					Cursor = Cursors.Hand;
					bContentModified = true;
				}
			}
			else
			{
				if (bIsMouseOverClose || bIsMouseOverTitle || bIsMouseOverContent)
					bContentModified = true;

				bIsMouseOverClose = false;
				bIsMouseOverTitle = false;
				bIsMouseOverContent = false;
				Cursor = Cursors.Default;
			}

			if (bContentModified)
				Refresh();
		}

		/// <summary>
		/// Raises the <see cref="E:MouseDown"/> event.
		/// </summary>
		/// <param name="mea">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		protected override void OnMouseDown(MouseEventArgs mea)
		{
			base.OnMouseDown(mea);
			bIsMouseDown = true;
			
			if (bIsMouseOverClose)
				Refresh();
		}

		/// <summary>
		/// Raises the <see cref="E:MouseUp"/> event.
		/// </summary>
		/// <param name="mea">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		protected override void OnMouseUp(MouseEventArgs mea)
		{
			base.OnMouseUp(mea);
			bIsMouseDown = false;

			if (bIsMouseOverClose)
			{
				Hide();
							
				if (CloseClick != null)
					CloseClick(this, new EventArgs());
			}
			else if (bIsMouseOverTitle)
			{
				if (TitleClick != null)
					TitleClick(this, new EventArgs());
			}
			else if (bIsMouseOverContent)
			{
				if (ContentClick != null)
					ContentClick(this, new EventArgs());
			}
		}

		/// <summary>
		/// Raises the <see cref="E:PaintBackground"/> event.
		/// </summary>
		/// <param name="pea">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
		protected override void OnPaintBackground(PaintEventArgs pea)
		{
			Graphics grfx = pea.Graphics;
			grfx.PageUnit = GraphicsUnit.Pixel;
			
			Graphics offScreenGraphics;
			Bitmap offscreenBitmap;
			
			offscreenBitmap = new Bitmap(BackgroundBitmap.Width, BackgroundBitmap.Height);
			offScreenGraphics = Graphics.FromImage(offscreenBitmap);
			
			if (BackgroundBitmap != null)
			{
				offScreenGraphics.DrawImage(BackgroundBitmap, 0, 0, BackgroundBitmap.Width, BackgroundBitmap.Height);
			}
			
			DrawCloseButton(offScreenGraphics);
			DrawText(offScreenGraphics);

			grfx.DrawImage(offscreenBitmap, 0, 0);
		}
		#endregion

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TaskbarNotifier));
			this.SuspendLayout();
			// 
			// TaskbarNotifier
			// 
			this.ClientSize = new System.Drawing.Size(292, 273);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "TaskbarNotifier";
			this.ResumeLayout(false);

		}
	}
}
