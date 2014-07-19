﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Dawn.Engine.Resource
{
	public class Texture : Resource
	{
        public override string ObjectClassName() { return Define.EngineClassName.TextureResource(); }

		Texture2D tex;
        public Texture()
            : base()
        {
        }

        public Texture(string filename)
            : base(filename)
        {
        }

        ~Texture()
        {
            Dispose();
        }

        public override void Load()
        {
            base.Load();
			tex = DGE.Data.Content.Load<Texture2D>(_filename);
        }
        public override void Unload()
        {
			tex.Dispose();
            base.Unload();
        }
        public Texture2D GetTexture()
        {
            if (isLoad())
            {
				return tex;
            }
            else
            {
                DGE.Debug.Error(this, Define.EngineErrorName.Resource_CannotGetResource(), GetErrorDetail());
                return null;
            }
        }

		public int Width()
		{
			if (isLoad())
			{
				return tex.Width;
			}
			else
			{
				DGE.Debug.Error(this, Define.EngineErrorName.Resource_CannotGetResource(), GetErrorDetail());
				return -1;
			}
		}
		public int Height()
		{
			if (isLoad())
			{
				return tex.Height;
			}
			else
			{
				DGE.Debug.Error(this, Define.EngineErrorName.Resource_CannotGetResource(), GetErrorDetail());
				return -1;
			}
		}
		public static Texture CreateTexture(int width, int height)
		{
			Texture tex = new Texture();
			tex.canChange = false;
			tex._isLoad = true;
			tex.tex = new Texture2D(DGE.Graphics.Device, width, height);
			DGE.TextureCache.ManageTexture(tex.tex);
			return tex;
		}
	}
}
