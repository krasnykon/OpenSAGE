using OpenSage.Mathematics;

namespace OpenSage.Tests
{
    public class MockPanel : IPanel
    {
        private int _width;
        private int _height;

        public MockPanel(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public Rectangle Frame => new Rectangle(0, 0, _width, _height);
        public Rectangle ClientBounds => new Rectangle(0, 0, _width, _height);
    }
}