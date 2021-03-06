﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Dawn.Engine.Define;
using Dawn.Engine;

namespace Dawn.Engine.Manager.Processor.FontManager
{
	public class FontHelper : EngineObject
	{
		public override string ObjectClassName() { return Define.EngineClassName.FontHelper(); }

		protected Texture2D[] tex;
		Dictionary<string, FontManager.Helper.CharacterObject> characters;
		Dictionary<FontManager.Helper.FontPosition, string> bcharacters;
		protected Resource.Font _font;

		protected int texRow;
		protected int texCol;

		protected float texRowPixels, texColPixels;

		protected System.Drawing.Graphics graphics;
		protected System.Drawing.Brush brush;
		protected System.Drawing.Bitmap bitmap;
		protected System.Windows.Forms.TextRenderer renderer;
		protected System.Drawing.IDeviceContext hdc;
		private Random random;

		//protected 
		public FontHelper(Resource.Font font)
		{
			_font = font;

			texRowPixels = _font.MaxCharacterHeight();
			texColPixels = _font.MaxCharacterWidth();
			texCol = (int)(EngineConst.FontHelper_TextureWidth() / _font.MaxCharacterWidth());
			texRow = (int)(EngineConst.FontHelper_TextureHeight() / _font.MaxCharacterHeight());

			tex = new Texture2D[EngineConst.FontHelper_TextureNum()];
			for (int i = 0; i < EngineConst.FontHelper_TextureNum(); i++)
			{
				tex[i] = new Texture2D(DGE.Graphics.Device, EngineConst.FontHelper_TextureWidth(), EngineConst.FontHelper_TextureHeight());

			}
			characters = new Dictionary<string, Helper.CharacterObject>(texCol * texRow * EngineConst.FontHelper_TextureNum());
			bcharacters = new Dictionary<Helper.FontPosition, string>(texCol * texRow * EngineConst.FontHelper_TextureNum(), new Helper.FontPositionComparer());
			//bitmap = new System.Drawing.Bitmap(EngineConst.FontHelper_TextureWidth(), EngineConst.FontHelper_TextureHeight());
			brush = new System.Drawing.SolidBrush(_font.font.Color);
			random = new Random();

			DGE.Graphics.WhenDeviceChanging += Graphics_WhenDeviceChanging;
			DGE.Graphics.WhenDeviceChanged += Graphics_WhenDeviceChanged;
		}

		void Graphics_WhenDeviceChanged(object sender, EventArgs e)
		{
			for (int i = 0; i < EngineConst.FontHelper_TextureNum(); i++)
			{
				DGE.Cache.TextureCache.GetTexture(tex[i]);
			}
		}

		void Graphics_WhenDeviceChanging(object sender, EventArgs e)
		{
			for (int i = 0; i < EngineConst.FontHelper_TextureNum(); i++)
			{
				DGE.Cache.TextureCache.SaveTexture(tex[i]);
			}
		}

		protected Texture2D FillTexture(ref System.Drawing.Bitmap bitmap)
		{
			Texture2D tmpTex = new Texture2D(DGE.Graphics.Device, (int)bitmap.Width, (int)bitmap.Height);
			System.Drawing.Imaging.BitmapData colorMapTex = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			Byte[] colorMap = new Byte[tmpTex.Width * tmpTex.Height * 4];
			int bound = Math.Abs(colorMapTex.Stride) * colorMapTex.Height;
			IntPtr ptr = colorMapTex.Scan0;
			System.Runtime.InteropServices.Marshal.Copy(ptr, colorMap, 0, bound);
			tmpTex.SetData<Byte>(colorMap);
			return tmpTex;
		}

		protected void setGraphics(ref System.Drawing.Bitmap bitmap)
		{
			graphics = System.Drawing.Graphics.FromImage(bitmap);
			//graphics.PageUnit = System.Drawing.GraphicsUnit.Pixel;
			graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
		}
		protected virtual Texture2D _DrawCharacter(string character, Helper.CharacterObject obj)
		{
			System.Drawing.Bitmap tmpBitmap = new System.Drawing.Bitmap((int)texColPixels, (int)texRowPixels);
			setGraphics(ref tmpBitmap);
			graphics.Clear(System.Drawing.Color.Transparent);
			//graphics.DrawString(character, _font.GetFont(), brush, (float)position.X, (float)position.Y);
			System.Windows.Forms.TextRenderer.DrawText(graphics, character, _font.GetFont(), new System.Drawing.Point(0, 0), _font.font.Color, System.Windows.Forms.TextFormatFlags.NoPadding);

			return FillTexture(ref tmpBitmap);
		}

		protected Texture2D NoneTexture()
		{
			Texture2D tmpTex = new Texture2D(DGE.Graphics.Device, (int)texColPixels, (int)texRowPixels);
			return tmpTex;
		}
		protected void _ClearCharacter(Helper.CharacterObject obj)
		{
			GraphicsDevice graphicsDevice = DGE.Graphics.Device;
			RenderTarget2D rt = new RenderTarget2D(graphicsDevice, EngineConst.FontHelper_TextureWidth(), EngineConst.FontHelper_TextureHeight());

			RenderTargetBinding[] old = graphicsDevice.GetRenderTargets();

			graphicsDevice.SetRenderTarget(rt);

			graphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);


			SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
			BeginCanvas(spriteBatch);

			spriteBatch.Draw(tex[obj.position.TexID], new Vector2(0, 0), Color.White);
			spriteBatch.Draw(NoneTexture(), new Vector2(obj.position.Col * texColPixels, obj.position.Row * texRowPixels), Color.White);
			spriteBatch.End();
			spriteBatch.Dispose();

			graphicsDevice.SetRenderTargets(old);


			graphicsDevice.Clear(Color.Transparent);

			RenderTargetBinding binding = new RenderTargetBinding(rt);
			tex[obj.position.TexID] = binding.RenderTarget as Texture2D;
		}
		protected void _NewCharacter(string character, Helper.CharacterObject obj)
		{
			GraphicsDevice graphicsDevice = DGE.Graphics.Device;
			RenderTarget2D rt;

			rt = new RenderTarget2D(graphicsDevice, EngineConst.FontHelper_TextureWidth(), EngineConst.FontHelper_TextureHeight());

			RenderTargetBinding[] old = graphicsDevice.GetRenderTargets();

			graphicsDevice.SetRenderTarget(rt);

			graphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);

			SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
			BeginCanvas(spriteBatch);

			spriteBatch.Draw(tex[obj.position.TexID], new Vector2(0, 0), Color.White);
			spriteBatch.Draw(_DrawCharacter(character, obj), new Vector2(obj.position.Col * texColPixels, obj.position.Row * texRowPixels), Color.White);

			spriteBatch.End();
			spriteBatch.Dispose();

			graphicsDevice.SetRenderTargets(old);

			graphicsDevice.Clear(Color.Transparent);

			RenderTargetBinding binding = new RenderTargetBinding(rt);
			tex[obj.position.TexID] = binding.RenderTarget as Texture2D;
		}

		protected void NewCharacter(string character, Helper.CharacterObject obj)
		{
			_NewCharacter(character, obj);
		}

		protected Helper.CharacterObject FindCharacter(string character)
		{
			Helper.CharacterObject obj;
			characters.TryGetValue(character, out obj);
			return obj;
		}

		protected Helper.CharacterObject GenerateCharacter(string character)
		{
			Helper.CharacterObject obj;
			obj = FindCharacter(character);
			if (obj == null)
			{
				obj = new Helper.CharacterObject { position = null, Width = -1, Height = -1, character = character };
				characters.Add(character, obj);
			}
			return obj;
		}
		protected Helper.CharacterObject _GetCharacter(string character)
		{
			Helper.CharacterObject obj;
			obj = GenerateCharacter(character);
			if (obj.position == null)
			{
				Helper.FontPosition pos = new Helper.FontPosition();

				pos.TexID = random.Next(0, EngineConst.FontHelper_TextureNum() - 1);
				pos.Row = random.Next(0, texRow - 1);
				pos.Col = random.Next(0, texCol - 1);
				Helper.FontPosition tmp = new Helper.FontPosition { TexID = pos.TexID, Row = pos.Row, Col = pos.Col };
				bool useFlag = false;
				Helper.FontPosition tmp2 = new Helper.FontPosition { TexID = pos.TexID, Row = pos.Row, Col = pos.Col };
				while (bcharacters.ContainsKey(pos))
				{
					pos.Col++;
					if (pos.Col >= texCol)
					{
						pos.Col = 0;
						pos.Row++;
					}
					if (pos.Row >= texRow)
					{
						pos.Row = 0;
						pos.TexID++;
					}
					if (pos.TexID >= EngineConst.FontHelper_TextureNum())
					{
						pos.TexID = 0;
					}
					if (tmp.Row == pos.Row && tmp.Col == pos.Col && tmp.TexID == pos.TexID)
					{
						string tmpChar;
						Helper.CharacterObject cobj;

						bcharacters.TryGetValue(pos, out tmpChar);
						characters.TryGetValue(tmpChar, out cobj);
						_ClearCharacter(cobj);
						bcharacters.Remove(pos);
						bcharacters.Add(pos, character);
						characters.Remove(tmpChar);
						useFlag = true;
						break;
					}
				}
				if (!useFlag)
				{
					bcharacters.Add(pos, character);
				}
				obj.position = pos;
				NewCharacter(character, obj);
			}
			return obj;
		}

		protected void DrawCharacterToTexture(float x, float y, string character, SpriteBatch canvas)
		{
			Helper.CharacterObject charobj = _GetCharacter(character);
			Helper.FontPosition pos = charobj.position;
			canvas.Draw(tex[pos.TexID], new Vector2(x, y), new Rectangle((int)(pos.Col * texColPixels), (int)(pos.Row * texRowPixels), (int)texColPixels, (int)texRowPixels), Color.White);
		}

		protected float[] MeasureString(string str)
		{
			float[] widths;
			widths = new float[str.Length];
			for (int i = 0; i < str.Length; i++)
			{
				Helper.CharacterObject obj = GenerateCharacter(str.Substring(i, 1));
				float tmpWidth;
				if (obj.Width == -1)
				{
					tmpWidth = _font.CharacterWidth(str.Substring(i, 1));
					obj.Width = tmpWidth;
				}
				else
				{
					tmpWidth = obj.Width;
				}
				if (obj.Height == -1)
				{
					obj.Height = _font.CharacterWidth(str.Substring(i, 1));
				}

				widths[i] = tmpWidth;
			}
			return widths;
		}

		public float StringWidth(string str)
		{
			return MeasureString(str).Sum();
		}
		public float StringHeight()
		{
			return _font.MaxCharacterHeight();
		}
		private void BeginCanvas(SpriteBatch canvas)
		{
			canvas.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
		}
		public Dawn.Engine.Resource.Texture DrawStringToTexture(string str)
		{
			//System.Diagnostics.Trace.WriteLine("Dawn> Render String...Setting Target");
			float[] strWidth = MeasureString(str);

			GraphicsDevice graphicsDevice = DGE.Graphics.Device;
			if (str == "") return Dawn.Engine.Resource.Texture.CreateTexture(1, 1);
			RenderTarget2D rt = new RenderTarget2D(graphicsDevice, (int)strWidth.Sum(), (int)_font.MaxCharacterHeight());

			RenderTargetBinding[] old = graphicsDevice.GetRenderTargets();

			graphicsDevice.SetRenderTarget(rt);

			graphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);
			//System.Diagnostics.Trace.WriteLine("Dawn> Render String...Processing");

			SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
			BeginCanvas(spriteBatch);

			DrawString(str, spriteBatch, 0, 0);

			spriteBatch.End();
			spriteBatch.Dispose();

			graphicsDevice.SetRenderTargets(old);


			graphicsDevice.Clear(Color.Transparent);

			RenderTargetBinding binding = new RenderTargetBinding(rt);
			return Dawn.Engine.Resource.Texture.CreateTexture(binding.RenderTarget as Texture2D);

		}

		public void DrawString(string str, float x, float y)
		{
			DrawString(str, DGE.Graphics.Canvas, x, y);
		}

		public void DrawString(string str, SpriteBatch canvas, float x, float y)
		{
			float x1 = x;
			float[] strWidth = MeasureString(str);
			for (int i = 0; i < str.Length; i++)
			{
				//System.Diagnostics.Trace.WriteLine("Dawn> Render String...Character #" + i.ToString());
				DrawCharacterToTexture(x1, y, str.Substring(i, 1), canvas);
				x1 += strWidth[i];
			}
		}

		public void DrawStringCommand(string str, float x, float y)
		{
			float y1 = y;
			float y1Add = _font.MaxCharacterHeight();
			char[] ch = new char[] { '\n', '\r' };
			string[] s = str.Split(ch);
			for (int i = 0; i < s.Length; i++)
			{
				DrawString(s[i], x, y1);
				y1 += y1Add;
			}
		}
	}
}
