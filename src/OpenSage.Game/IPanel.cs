using OpenSage.Mathematics;

namespace OpenSage
{
    public interface IPanel
    {
        public Rectangle Frame { get; }
        public Rectangle ClientBounds { get; }
    }
}