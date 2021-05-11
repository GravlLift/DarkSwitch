namespace System.Drawing
{
    static class IconExtensions
    {
        public  static Icon Invert(this Icon ico)
        {
            var pic = new Bitmap(128, 128);
            using (Graphics g = Graphics.FromImage(pic))
            {
                g.DrawImage(ico.ToBitmap(), 0, 0, 128, 128);
            }

            for (int y = 0; (y <= (pic.Height - 1)); y++)
            {
                for (int x = 0; (x <= (pic.Width - 1)); x++)
                {
                    Color inv = pic.GetPixel(x, y);
                    inv = Color.FromArgb(inv.A, (255 - inv.R), (255 - inv.G), (255 - inv.B));
                    pic.SetPixel(x, y, inv);
                }
            }
            return Icon.FromHandle(pic.GetHicon());
        }
    }
}
